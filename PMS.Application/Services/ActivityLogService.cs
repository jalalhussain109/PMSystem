using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;

namespace PMS.Application.Services
{


    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository activityLogRepository;

        public ActivityLogService(IActivityLogRepository activityLogRepository)
        {
            this.activityLogRepository = activityLogRepository;
        }

        public async Task<List<ActivityLog>> GetHistoryAsync(
            string entityName,
            string entityId,
            CancellationToken cancellationToken = default)
        {
            return await activityLogRepository.GetHistoryAsync(entityName, entityId, cancellationToken);
        }
    }
}
