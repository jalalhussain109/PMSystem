using Microsoft.AspNetCore.Identity;

namespace PMS.Domain.Entities;

public class ApplicationUser : IdentityUser<long>
{
    public string? DisplayName { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public long? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public long? ModifiedBy { get; set; }

    public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();

    public virtual ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
}