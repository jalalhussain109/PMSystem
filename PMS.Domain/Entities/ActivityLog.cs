using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Domain.Entities
{
    public class ActivityLog
    {
        public long Id { get; set; }

        public Guid ChangeSetId { get; set; }

        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;
        // Created, Updated, Deleted

        public string? FieldName { get; set; }

        public string? OldValue { get; set; }
        public string? NewValue { get; set; }

        public long? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
