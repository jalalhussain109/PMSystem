
using PMS.Domain.Entities;
using PMS.Domain.Models;


namespace PMS.Domain.Interfaces
{
    public interface IApiLoggingRepository : IGenericRepository<ApiLog, long>
    {
        Task<ApiLogResponseModel> LogRequestAsync(ApiLog model);
        Task<ApiLogResponseModel> LogResponseAsync(ApiLog model);
    }
}
