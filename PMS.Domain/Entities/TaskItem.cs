using System;
using System.Collections.Generic;
using PMS.Domain.Enums;

namespace PMS.Domain.Entities
{
    public class TaskItem
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long? ParentTaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? AssignedTo { get; set; }
        public Priority Priority { get; set; } = Priority.Medium;
        public TaskState Status { get; set; } = TaskState.Open;
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual Project? Project { get; set; }
        public virtual TaskItem? ParentTask { get; set; }
        public virtual ICollection<TaskItem> SubTasks { get; set; } = new List<TaskItem>();
        public virtual ApplicationUser? AssignedUser { get; set; }
    }
}
