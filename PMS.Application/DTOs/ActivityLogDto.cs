using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Application.DTOs
{
    public class ActivityLogDto
    {
        public long Id { get; set; }

        public Guid ChangeSetId { get; set; }

        public string EntityName { get; set; } = "";

        public string EntityId { get; set; } = "";

        public string Action { get; set; } = "";

        public string? FieldName { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public long? UserId { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
