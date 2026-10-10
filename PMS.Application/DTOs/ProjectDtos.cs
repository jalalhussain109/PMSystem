using System;
using System.Collections.Generic;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs
{

    public record CreateProjectDto(string ProjectCode, string ProjectName, string? Description, DateTime? StartDate, DateTime? EndDate, Priority Priority, long? ProjectManagerId);

    public record UpdateProjectDto(string ProjectCode, string ProjectName, string? Description, DateTime? StartDate, DateTime? EndDate, Priority Priority, long? ProjectManagerId);

    public record ProjectDto(long Id, string ProjectCode, string ProjectName, string? Description, DateTime? StartDate, DateTime? EndDate, ProjectStatus Status, Priority Priority, long? ProjectManagerId, DateTime CreatedDate, DateTime? ModifiedDate);

    public record AddMemberDto(long UserId, string Role);
    public record ProjectStatusDto(string NewStatus, long PerformedBy);
}
