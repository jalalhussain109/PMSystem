using PMS.Application.DTOs;
using PMS.Domain.Entities;

namespace PMS.Application.Interfaces
{
    public interface IActivityLogService
    {
        Task<List<ActivityLogDto>> GetHistoryAsync(string entityName, string entityId, CancellationToken cancellationToken = default);
    }
}
