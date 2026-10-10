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
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly AutoMapper.IMapper _mapper;

        public ProjectService(IProjectRepository projectRepository, AutoMapper.IMapper mapper)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<ProjectDto>> CreateAsync(CreateProjectDto dto)
        {
            var project = new Project
            {
                ProjectCode = dto.ProjectCode,
                ProjectName = dto.ProjectName,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Priority = dto.Priority,
                ProjectManagerId = dto.ProjectManagerId,
                CreatedDate = DateTime.UtcNow
            };

            var created = await _projectRepository.AddAsync(project);
            return ServiceResult<ProjectDto>.Success(_mapper.Map<ProjectDto>(created));
        }

        public async Task<ServiceResult<ProjectDto>> GetByIdAsync(long id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return ServiceResult<ProjectDto>.Missing();
            return ServiceResult<ProjectDto>.Success(_mapper.Map<ProjectDto>(project));
        }

        public async Task<ServiceResult<IEnumerable<ProjectDto>>> ListAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            var list = projects.Select(p => _mapper.Map<ProjectDto>(p));
            return ServiceResult<IEnumerable<ProjectDto>>.Success(list);
        }

        public async Task<ServiceResult<ProjectDto>> UpdateAsync(long id, UpdateProjectDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return ServiceResult<ProjectDto>.Missing();

            project.ProjectCode = dto.ProjectCode;
            project.ProjectName = dto.ProjectName;
            project.Description = dto.Description;
            project.StartDate = dto.StartDate;
            project.EndDate = dto.EndDate;
            project.Priority = dto.Priority;
            project.ProjectManagerId = dto.ProjectManagerId;
            project.ModifiedDate = DateTime.UtcNow;

            var updated = await _projectRepository.UpdateAsync(project);
            return ServiceResult<ProjectDto>.Success(_mapper.Map<ProjectDto>(updated));
        }

        public async Task<ServiceResult<ProjectDto>> ChangeStatusAsync(long projectId, ProjectStatusDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return ServiceResult<ProjectDto>.Missing();

            if (!Enum.TryParse<ProjectStatus>(dto.NewStatus, true, out var newStatus))
                return ServiceResult<ProjectDto>.Failure("Invalid status");

            if (!IsValidTransition(project.Status, newStatus))
                return ServiceResult<ProjectDto>.Failure($"Invalid status transition from {project.Status} to {newStatus}");

            project.Status = newStatus;
            project.ModifiedBy = dto.PerformedBy;
            project.ModifiedDate = DateTime.UtcNow;

            var updated = await _projectRepository.UpdateAsync(project);
            return ServiceResult<ProjectDto>.Success(_mapper.Map<ProjectDto>(updated));
        }

        public async Task<ServiceResult<ProjectDto>> AddMemberAsync(long projectId, AddMemberDto dto)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return ServiceResult<ProjectDto>.Missing();

            var member = new ProjectMember
            {
                ProjectId = projectId,
                UserId = dto.UserId,
                Role = dto.Role,
                IsActive = true
            };

            project.Members.Add(member);
            var updated = await _projectRepository.UpdateAsync(project);
            return ServiceResult<ProjectDto>.Success(_mapper.Map<ProjectDto>(updated));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(long id)
        {
            var deleted = await _projectRepository.DeleteAsync(id);
            return deleted ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Delete failed");
        }

        public async Task<ServiceResult<bool>> RemoveMemberAsync(long projectId, long userId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId);
            if (project == null) return ServiceResult<bool>.Missing();

            var member = project.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null) return ServiceResult<bool>.Missing();

            member.IsActive = false;
            project.ModifiedDate = DateTime.UtcNow;
            var updated = await _projectRepository.UpdateAsync(project);
            return updated != null ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Remove member failed");
        }


        private bool IsValidTransition(ProjectStatus current, ProjectStatus next)
        {
            if (current == next) return true;

            return (current, next) switch
            {
                (ProjectStatus.Draft, ProjectStatus.Planning) => true,
                (ProjectStatus.Planning, ProjectStatus.InProgress) => true,
                (ProjectStatus.InProgress, ProjectStatus.Completed) => true,
                (ProjectStatus.InProgress, ProjectStatus.OnHold) => true,
                (ProjectStatus.OnHold, ProjectStatus.InProgress) => true,
                (ProjectStatus.Draft, ProjectStatus.Cancelled) => true,
                (ProjectStatus.Planning, ProjectStatus.Cancelled) => true,
                (ProjectStatus.InProgress, ProjectStatus.Cancelled) => true,
                _ => false
            };
        }
    }
}
