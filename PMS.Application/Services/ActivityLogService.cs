using AutoMapper;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;

namespace PMS.Application.Services
{


    public class ActivityLogService : IActivityLogService
    {
        private readonly IActivityLogRepository _activityLogRepository;
        private readonly IMapper _mapper;

        public ActivityLogService(IActivityLogRepository activityLogRepository, IMapper mapper)
        {
            this._activityLogRepository = activityLogRepository;
            this._mapper = mapper;
        }

        public async Task<List<ActivityLogDto>> GetHistoryAsync(string entityName, string entityId, CancellationToken cancellationToken = default)
        {
            var result = await _activityLogRepository.GetHistoryAsync(entityName, entityId, cancellationToken);

            if (result != null)
            {
                return result.Select(_mapper.Map<ActivityLogDto>).ToList();
            }

            return new List<ActivityLogDto>();
        }
    }
}
