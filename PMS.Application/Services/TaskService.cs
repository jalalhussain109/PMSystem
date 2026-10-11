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

        public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository, AutoMapper.IMapper mapper)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<PagedResult<TaskDto>>> ListAsync(int page = 1, int pageSize = 20, string? sortBy = null, string? sortDir = null, string? status = null, long? assignedTo = null, string? priority = null, DateTime? dueFrom = null, DateTime? dueTo = null, string? search = null, long? projectId = null, bool includeSubtasks = true, DateTime? startFrom = null)
        {
            // validate paging
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var tasks = (await _taskRepository.GetAllAsync()).AsQueryable();

            var checkList = tasks.ToList();

            // filters
            if (projectId.HasValue)
                tasks = tasks.Where(t => t.ProjectId == projectId.Value);

            if (!includeSubtasks)
                tasks = tasks.Where(t => t.ParentTaskId == null);

            if (assignedTo.HasValue)
                tasks = tasks.Where(t => t.AssignedTo == assignedTo.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TaskState>(status, true, out var s))
                tasks = tasks.Where(t => t.Status == s);

            if (!string.IsNullOrWhiteSpace(priority) && Enum.TryParse<Priority>(priority, true, out var p))
                tasks = tasks.Where(t => t.Priority == p);

            if (dueFrom.HasValue)
                tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date >= dueFrom.Value.Date);

            if (dueTo.HasValue)
                tasks = tasks.Where(t => t.DueDate.HasValue && t.DueDate.Value.Date <= dueTo.Value.Date);

            if (startFrom.HasValue)
                tasks = tasks.Where(t => t.StartDate.HasValue && t.StartDate.Value.Date >= startFrom.Value.Date);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLowerInvariant();
                tasks = tasks.Where(t => (t.Title != null && t.Title.ToLowerInvariant().Contains(q)) || (t.Description != null && t.Description.ToLowerInvariant().Contains(q)));
            }

            // sorting
            var sort = (sortBy ?? "createdDate").ToLowerInvariant();
            var dir = (sortDir ?? "desc").ToLowerInvariant();

            tasks = (sort, dir) switch
            {
                ("title", "asc") => tasks.OrderBy(t => t.Title),
                ("title", _) => tasks.OrderByDescending(t => t.Title),
                ("startdate", "asc") => tasks.OrderBy(t => t.StartDate),
                ("startdate", _) => tasks.OrderByDescending(t => t.StartDate),
                ("duedate", "asc") => tasks.OrderBy(t => t.DueDate),
                ("duedate", _) => tasks.OrderByDescending(t => t.DueDate),
                ("priority", "asc") => tasks.OrderBy(t => t.Priority),
                ("priority", _) => tasks.OrderByDescending(t => t.Priority),
                ("status", "asc") => tasks.OrderBy(t => t.Status),
                ("status", _) => tasks.OrderByDescending(t => t.Status),
                ("projectname", "asc") => tasks.OrderBy(t => t.Project != null ? t.Project.ProjectName : string.Empty),
                ("projectname", _) => tasks.OrderByDescending(t => t.Project != null ? t.Project.ProjectName : string.Empty),
                ("assigneename", "asc") => tasks.OrderBy(t => t.AssignedUser != null ? (t.AssignedUser.DisplayName ?? t.AssignedUser.UserName) : string.Empty),
                ("assigneename", _) => tasks.OrderByDescending(t => t.AssignedUser != null ? (t.AssignedUser.DisplayName ?? t.AssignedUser.UserName) : string.Empty),
                ("createddate", "asc") => tasks.OrderBy(t => t.CreatedDate),
                _ => tasks.OrderByDescending(t => t.CreatedDate),
            };

            var total = tasks.LongCount();

            var items = tasks.Skip((page - 1) * pageSize).Take(pageSize).ToList().Select(t => _mapper.Map<TaskDto>(t)).ToList();

            var result = new PagedResult<TaskDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)System.Math.Ceiling(total / (double)pageSize)
            };

            return ServiceResult<PagedResult<TaskDto>>.Success(result);
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
