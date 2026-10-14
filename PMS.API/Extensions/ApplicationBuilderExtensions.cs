using PMS.Application.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PMS.API.Middleware;

namespace PMS.API.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiLogsMiddleware(this IApplicationBuilder app)
        {

            var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
            var loggingOptions = app.ApplicationServices
           .GetRequiredService<IOptions<PMSLoggingOptions>>().Value;
            //Enable custom request / response logging
            if (loggingOptions.HttpRequestLogging || loggingOptions.HttpResponseLogging)
            {
                app.UseMiddleware<RequestResponseLoggingMiddleware>();
            }
            return app;
        }
    }
}
