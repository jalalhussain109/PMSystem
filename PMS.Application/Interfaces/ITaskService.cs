using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs;

namespace PMS.Application.Interfaces;

public interface ITaskService
{
    Task<ServiceResult<TaskDto>> CreateAsync(long projectId, CreateTaskDto dto);
    Task<ServiceResult<TaskDto>> GetByIdAsync(long id);
    Task<ServiceResult<PagedResult<TaskDto>>> ListAsync(int page = 1, int pageSize = 20, string? sortBy = null, string? sortDir = null, string? status = null, long? assignedTo = null, string? priority = null, DateTime? dueFrom = null, DateTime? dueTo = null, string? search = null, long? projectId = null, bool includeSubtasks = true, DateTime? startFrom = null);
    Task<ServiceResult<IEnumerable<TaskDto>>> ListByProjectAsync(long projectId);
    Task<ServiceResult<TaskDto>> UpdateAsync(long id, UpdateTaskDto dto);
    Task<ServiceResult<TaskDto>> CreateSubtaskAsync(long parentTaskId, CreateTaskDto dto);
    Task<ServiceResult<TaskDto>> ChangeStatusAsync(long id, ChangeTaskStatusDto dto);
    Task<ServiceResult<bool>> DeleteAsync(long id);
}
