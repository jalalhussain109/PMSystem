using PMS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Interfaces
{
    public interface IActivityLogRepository
    {
        Task<List<ActivityLog>> GetHistoryAsync(string entityName, string entityId, CancellationToken cancellationToken = default);
    }
}
