using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using PMS.Domain.Enums;
using PMS.Domain.Interfaces;

namespace PMS.Application.Services
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly AutoMapper.IMapper _mapper;

        public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
        }

        public async Task<ServiceResult<TaskDto>> CreateAsync(long projectId, CreateTaskDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return ServiceResult<TaskDto>.Failure("Project not found");

            var entity = new TaskItem
            {
                ProjectId = projectId,
                ParentTaskId = dto.ParentTaskId,
                Title = dto.Title,
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                Priority = dto.Priority,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _taskRepository.AddAsync(entity);
            return ServiceResult<TaskDto>.Success(_mapper.Map<TaskDto>(created));
        }

        public async Task<ServiceResult<TaskDto>> GetByIdAsync(long id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return ServiceResult<TaskDto>.Missing();
            return ServiceResult<TaskDto>.Success(_mapper.Map<TaskDto>(task));
        }

        public async Task<ServiceResult<IEnumerable<TaskDto>>> ListByProjectAsync(long projectId)
        {
            var tasks = await _taskRepository.GetAllAsync();
            var list = tasks.Where(t => t.ProjectId == projectId).Select(t => _mapper.Map<TaskDto>(t));
            return ServiceResult<IEnumerable<TaskDto>>.Success(list);
        }

        public async Task<ServiceResult<TaskDto>> UpdateAsync(long id, UpdateTaskDto dto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return ServiceResult<TaskDto>.Missing();

            task.Title = dto.Title;
            task.Description = dto.Description;
            task.AssignedTo = dto.AssignedTo;
            task.Priority = dto.Priority;
            task.StartDate = dto.StartDate;
            task.DueDate = dto.DueDate;

            var updated = await _taskRepository.UpdateAsync(task);
            return ServiceResult<TaskDto>.Success(_mapper.Map<TaskDto>(updated));
        }

        public async Task<ServiceResult<TaskDto>> CreateSubtaskAsync(long parentTaskId, CreateTaskDto dto)
        {
            var parent = await _taskRepository.GetByIdAsync(parentTaskId);
            if (parent == null) return ServiceResult<TaskDto>.Failure("Parent task not found");

            var entity = new TaskItem
            {
                ProjectId = parent.ProjectId,
                ParentTaskId = parentTaskId,
                Title = dto.Title,
                Description = dto.Description,
                AssignedTo = dto.AssignedTo,
                Priority = dto.Priority,
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _taskRepository.AddAsync(entity);
            return ServiceResult<TaskDto>.Success(_mapper.Map<TaskDto>(created));
        }

        public async Task<ServiceResult<TaskDto>> ChangeStatusAsync(long id, ChangeTaskStatusDto dto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null) return ServiceResult<TaskDto>.Missing();

            if (!Enum.TryParse<TaskState>(dto.NewStatus, true, out var newState))
                return ServiceResult<TaskDto>.Failure("Invalid status");

            task.Status = newState;
            if (newState == TaskState.Completed) task.CompletedDate = DateTime.UtcNow;

            var updated = await _taskRepository.UpdateAsync(task);
            return ServiceResult<TaskDto>.Success(_mapper.Map<TaskDto>(updated));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(long id)
        {
            var existed = await _taskRepository.GetByIdAsync(id);
            if (existed == null) return ServiceResult<bool>.Missing();

            var removed = await _taskRepository.DeleteAsync(id);
            return removed ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Delete failed");
        }
       
    }
}
