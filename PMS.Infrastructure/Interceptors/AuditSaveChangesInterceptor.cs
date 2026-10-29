using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using System.Globalization;

namespace PMS.Infrastructure.Interceptors
{
    public sealed class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUserService _currentUser;

        private bool _savingAudit;

        private readonly List<PendingAudit> _pendingAudits = new();

        private Guid _changeSetId;

        public AuditSaveChangesInterceptor(ICurrentUserService currentUser)
        {
            _currentUser = currentUser;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            if (!_savingAudit && eventData.Context is { } context)
            {
                CaptureChanges(context);
            }

            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_savingAudit && eventData.Context is { } context)
            {
                CaptureChanges(context);
            }

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }



        public bool HasPendingAudits => _pendingAudits.Count > 0;

        public List<ActivityLog> BuildAuditLogs()
        {
            var logs = new List<ActivityLog>();
            var now = DateTime.UtcNow;

            foreach (var audit in _pendingAudits)
            {
                var keyProperties = audit.Entry.Properties.Where(p => p.Metadata.IsPrimaryKey()).ToList();

                var entityId = string.Join(",", keyProperties.Select(p => ConvertValue(p.CurrentValue) ?? ""));

                if (audit.Changes.Count == 0)
                {
                    logs.Add(CreateLog(audit, entityId, null, null, null, now));
                }

                foreach (var change in audit.Changes)
                {
                    logs.Add(CreateLog(
                        audit,
                        entityId,
                        change.FieldName,
                        change.OldValue,
                        change.NewValue,
                        now));
                }
            }

            return logs;
        }

        private ActivityLog CreateLog(PendingAudit audit, string entityId, string? fieldName, string? oldValue, string? newValue, DateTime now)
        {
            return new ActivityLog
            {
                ChangeSetId = _changeSetId,
                EntityName = audit.EntityName,
                EntityId = entityId,
                Action = audit.Action,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                UserId = _currentUser.UserId,
                CreatedAt = now
            };
        }

        public void BeginAuditSave()
        {
            _savingAudit = true;
        }

        public void EndAuditSave()
        {
            _savingAudit = false;
            _pendingAudits.Clear();
        }

        private void CaptureChanges(DbContext context)
        {
            _pendingAudits.Clear();
            _changeSetId = Guid.NewGuid();

            context.ChangeTracker.DetectChanges();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is ActivityLog)
                    continue;

                if (entry.State == EntityState.Unchanged ||
                    entry.State == EntityState.Detached)
                    continue;

                var action = entry.State switch
                {
                    EntityState.Added => "Created",
                    EntityState.Modified => "Updated",
                    EntityState.Deleted => "Deleted",
                    _ => ""
                };

                if (string.IsNullOrEmpty(action))
                    continue;

                var audit = new PendingAudit
                {
                    Entry = entry,
                    Action = action,
                    EntityName = entry.Metadata.ClrType.Name
                };

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.IsPrimaryKey())
                        continue;

                    if (!ShouldAudit(entry))
                        continue;

                    var oldValue = ConvertValue(property.OriginalValue);
                    var newValue = ConvertValue(property.CurrentValue);

                    if (entry.State == EntityState.Added)
                    {
                        if (property.CurrentValue == null)
                            continue;
                        audit.Changes.Add(new FieldChange(
                            property.Metadata.Name,
                            null,
                            newValue));
                    }
                    else if (entry.State == EntityState.Deleted)
                    {
                        audit.Changes.Add(new FieldChange(
                            property.Metadata.Name,
                            oldValue,
                            null));
                    }
                    else if (entry.State == EntityState.Modified &&
                             property.IsModified &&
                             !Equals(property.OriginalValue,
                                     property.CurrentValue))
                    {
                        audit.Changes.Add(new FieldChange(
                            property.Metadata.Name,
                            oldValue,
                            newValue));
                    }
                }

                if (audit.Changes.Count > 0 ||
                    entry.State != EntityState.Modified)
                {
                    _pendingAudits.Add(audit);
                }
            }
        }

        private static bool ShouldAudit(EntityEntry entry)
        {
            return entry.Entity is Project  or ProjectMember or TaskItem;
        }

        private static bool ShouldIgnore(string propertyName)
        {
            return propertyName is
                "PasswordHash" or
                "SecurityStamp" or
                "ConcurrencyStamp" or
                "AccessFailedCount" or
                "LockoutEnd";
        }

        private static string? ConvertValue(object? value)
        {
            return value switch
            {
                null => null,
                DateTime date => date.ToString("O"),
                DateTimeOffset date => date.ToString("O"),
                byte[] bytes => Convert.ToBase64String(bytes),
                IFormattable formattable =>
                    formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }

        private sealed class PendingAudit
        {
            public required EntityEntry Entry { get; init; }

            public required string EntityName { get; init; }

            public required string Action { get; init; }

            public List<FieldChange> Changes { get; } = new();
        }

        private sealed record FieldChange(
            string FieldName,
            string? OldValue,
            string? NewValue);
    }
}
