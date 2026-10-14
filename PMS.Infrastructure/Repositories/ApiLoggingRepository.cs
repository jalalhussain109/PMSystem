using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PMS.Application.Configuration;
using PMS.Application.DTOs;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using PMS.Domain.Models;
using System.Data;

namespace PMS.Infrastructure.Repositories
{
    public class ApiLoggingRepository : GenericRepository<ApiLog, long>, IApiLoggingRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ApiLoggingRepository> _logger;

        public ApiLoggingRepository(IOptions<PMSLoggingOptions> options, ILogger<ApiLoggingRepository> logger, ApplicationDbContext context) :base(context)
        {
            _logger = logger;
            _context = context;
        }

        public Task<ApiLogResponseModel> LogRequestAsync(ApiLog model) => SaveAsync(model, "INSERT");

        public Task<ApiLogResponseModel> LogResponseAsync(ApiLog model) => SaveAsync(model, "UPDATE");

        private async Task<ApiLogResponseModel> SaveAsync(ApiLog model, string action)
        {
            try
            {
                if (action == "INSERT")
                {
                    model.Id = 0;
                    model.CreatedDate = DateTime.UtcNow;

                    model = await base.AddAsync(model);

                    return new ApiLogResponseModel
                    {
                        Id = model.Id,
                        ErrorCode = 0,
                        ErrorMessage = string.Empty
                    };
                }

                if (action == "UPDATE")
                {
                    if (model.Id <= 0)
                    {
                        return new ApiLogResponseModel
                        {
                            ErrorCode = -2,
                            ErrorMessage = "@Id is required for UPDATE."
                        };
                    }

                    var existingLog = await base.GetByIdAsync(model.Id);

                    if (existingLog == null)
                    {
                        return new ApiLogResponseModel
                        {
                            ErrorCode = -3,
                            ErrorMessage = "No API log record was found for the supplied @Id."
                        };
                    }

                    existingLog.ResponseBody = model.ResponseBody;
                    existingLog.StatusCode = model.StatusCode;
                    existingLog.DurationMs = model.DurationMs;
                    existingLog.ModifiedBy = model.ModifiedBy;
                    existingLog.ModifiedDate = DateTime.UtcNow;

                    await base.UpdateAsync(existingLog);

                    return new ApiLogResponseModel
                    {
                        Id = existingLog.Id,
                        ErrorCode = 0,
                        ErrorMessage = string.Empty
                    };
                }

                return new ApiLogResponseModel
                {
                    ErrorCode = -1,
                    ErrorMessage = $"Unknown action: {action}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(  ex, "Failed to persist API log ({Action})", action);

                return new ApiLogResponseModel
                {
                    Id = 0,
                    ErrorCode = -1,
                    ErrorMessage = ex.Message
                };
            }
        }

    }
}
