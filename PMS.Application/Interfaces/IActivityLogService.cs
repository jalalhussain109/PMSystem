using PMS.Domain.Entities;

namespace PMS.Application.Interfaces
{
    public interface IActivityLogService
    {
        Task<List<ActivityLog>> GetHistoryAsync(string entityName, string entityId, CancellationToken cancellationToken = default);
    }
}
