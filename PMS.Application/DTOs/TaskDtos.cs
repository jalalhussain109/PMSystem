using System;
using PMS.Domain.Enums;

namespace PMS.Application.DTOs
{
    public record CreateTaskDto(string Title, string? Description, long? AssignedTo, Priority Priority, DateTime? StartDate, DateTime? DueDate, long? ParentTaskId = null);

    public record UpdateTaskDto(string Title, string? Description, long? AssignedTo, Priority Priority, DateTime? StartDate, DateTime? DueDate);

    public record TaskDto(long Id, long ProjectId, long? ParentTaskId, string Title, string? Description, long? AssignedTo, Priority Priority, string Status, DateTime? StartDate, DateTime? DueDate, DateTime? CompletedDate);

    public record ChangeTaskStatusDto(string NewStatus);
}
