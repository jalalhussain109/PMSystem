using System;
using System.Collections.Generic;
using PMS.Domain.Enums;

namespace PMS.Domain.Entities
{
    public class Project
    {
        public long Id { get; set; }
        public string ProjectCode { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
        public Priority Priority { get; set; } = Priority.Medium;
        public long? ProjectManagerId { get; set; }

        // Audit
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
