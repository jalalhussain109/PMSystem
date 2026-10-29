using Microsoft.EntityFrameworkCore;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Infrastructure.Repositories
{
    public class ActivityLogRepository : IActivityLogRepository
    {
        private readonly ApplicationDbContext _context;

        public ActivityLogRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ActivityLog>> GetHistoryAsync(string entityName, string entityId, CancellationToken cancellationToken = default)
        {
            return await _context.ActivityLogs
                .AsNoTracking()
                .Where(x =>
                    x.EntityName == entityName &&
                    x.EntityId == entityId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new ActivityLog
                {
                    Id = x.Id,
                    ChangeSetId = x.ChangeSetId,
                    EntityName = x.EntityName,
                    EntityId = x.EntityId,
                    Action = x.Action,
                    FieldName = x.FieldName,
                    OldValue = x.OldValue,
                    NewValue = x.NewValue,
                    UserId = x.UserId,
                    CreatedAt = x.CreatedAt
                }).ToListAsync(cancellationToken);
        }

    }
}
