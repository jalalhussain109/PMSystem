using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs;

namespace PMS.Application.Interfaces;

public interface ITaskService
{
    Task<ServiceResult<TaskDto>> CreateAsync(long projectId, CreateTaskDto dto);
    Task<ServiceResult<TaskDto>> GetByIdAsync(long id);
    Task<ServiceResult<IEnumerable<TaskDto>>> ListByProjectAsync(long projectId);
    Task<ServiceResult<TaskDto>> UpdateAsync(long id, UpdateTaskDto dto);
    Task<ServiceResult<TaskDto>> CreateSubtaskAsync(long parentTaskId, CreateTaskDto dto);
    Task<ServiceResult<TaskDto>> ChangeStatusAsync(long id, ChangeTaskStatusDto dto);
    Task<ServiceResult<bool>> DeleteAsync(long id);
}
