using Microsoft.Extensions.Options;
using Microsoft.IO;
using PMS.Application.Configuration;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using System.Diagnostics;
using System.Text;

namespace PMS.API.Middleware
{
    public class RequestResponseLoggingMiddleware
    {
        private static readonly RecyclableMemoryStreamManager StreamManager = new();

        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly PMSLoggingOptions _options;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IOptions<PMSLoggingOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context, IApiLoggingRepository repo)
        {
          
            var endpoint = context.GetEndpoint();
          
            var model = new ApiLog
            {
                APIMethod = context.Request.Method,
                APIURL = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}",
                ClientIP = ResolveClientIp(context),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                CreatedBy = 0
            };

            var stopwatch = Stopwatch.StartNew();

            try
            {
                model.RequestBody = (_options.HttpRequestLogging)
                    ? await ReadRequestBodyAsync(context)
                    : null;
                var response = await repo.LogRequestAsync(model);
                model.Id = response.Id;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log request for {Path}", context.Request.Path);
            }


            if (!_options.HttpResponseLogging)
            {
                await _next(context);
                return;
            }

            var originalBodyStream = context.Response.Body;
            await using var responseBodyStream = StreamManager.GetStream();
            context.Response.Body = responseBodyStream;

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseText = await new StreamReader(responseBodyStream).ReadToEndAsync();
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                await responseBodyStream.CopyToAsync(originalBodyStream);
                context.Response.Body = originalBodyStream;

                if (model.Id != 0)
                {
                    model.ResponseBody = !string.IsNullOrEmpty(responseText) ? Truncate(responseText, _options.MaxBodyLength) : null;
                    model.StatusCode = context.Response.StatusCode;
                    model.DurationMs = stopwatch.ElapsedMilliseconds;
                    model.ModifiedBy = 0;
                    model.ModifiedDate = DateTime.UtcNow;

                    try
                    {
                        await repo.LogResponseAsync(model);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to log response for {Path}", context.Request.Path);
                    }
                }
            }
        }

        private async Task<string> ReadRequestBodyAsync(HttpContext context)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(
                context.Request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            return Truncate(body, _options.MaxBodyLength);
        }

        private static string Truncate(string value, int maxLength) =>
            string.IsNullOrEmpty(value) || value.Length <= maxLength ? value : value[..maxLength] + "...(truncated)";

        private static string ResolveClientIp(HttpContext context)
        {
            var forwarded = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            return !string.IsNullOrEmpty(forwarded)
                ? forwarded.Split(',')[0].Trim()
                : context.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
        }
    }
}
