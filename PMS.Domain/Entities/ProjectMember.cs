using System;

namespace PMS.Domain.Entities
{
    public class ProjectMember
    {
        public long Id { get; set; }
        public long ProjectId { get; set; }
        public long UserId { get; set; } // ApplicationUser.Id
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Audit
        public long? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public long? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public virtual Project? Project { get; set; }
        public virtual ApplicationUser? User { get; set; }

    }
}
