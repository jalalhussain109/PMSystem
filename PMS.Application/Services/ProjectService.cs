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
        private readonly IProjectMemberRepository _projectMemberRepository;
        private readonly AutoMapper.IMapper _mapper;

        public ProjectService(IProjectRepository projectRepository, IProjectMemberRepository projectMemberRepository, AutoMapper.IMapper mapper)
        {
            _projectRepository = projectRepository;
            _projectMemberRepository = projectMemberRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<ProjectDto>> CreateAsync(CreateProjectDto dto)
        {
            // Save project first (N+1 approach) without members
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
            // If member ids provided, add them separately (optional, can be assigned later)
            var memberIds = (dto.MemberIds ?? Array.Empty<long>()).Distinct();
            if (memberIds.Any())
            {
                foreach (var userId in memberIds)
                {
                    var pm = new ProjectMember
                    {
                        ProjectId = created.Id,
                        UserId = userId,
                        Role = "Member",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _projectMemberRepository.AddAsync(pm);
                }

                // reload project
                created = await _projectRepository.GetByIdAsync(created.Id) ?? created;
            }

            var dtoOut = await BuildProjectDto(created);
            return ServiceResult<ProjectDto>.Success(dtoOut);
        }

        public async Task<ServiceResult<ProjectDto>> GetByIdAsync(long id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null) return ServiceResult<ProjectDto>.Missing();
            var dto = await BuildProjectDto(project);
            return ServiceResult<ProjectDto>.Success(dto);
        }

        public async Task<ServiceResult<IEnumerable<ProjectDto>>> ListAsync()
        {
            var projects = await _projectRepository.GetAllAsync();
            var list = new List<ProjectDto>();
            foreach (var p in projects)
            {
                list.Add(await BuildProjectDto(p));
            }
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
            if (dto.MemberIds != null)
            {
                // synchronize using repository
                var existing = (await _projectMemberRepository.ListByProjectAsync(project.Id)).ToList();
                var selected = (dto.MemberIds ?? Array.Empty<long>()).ToHashSet();

                // deactivate members not in selected
                foreach (var ex in existing)
                {
                    if (selected.Contains(ex.UserId))
                    {
                        if (!ex.IsActive)
                        {
                            ex.IsActive = true;
                            await _projectMemberRepository.UpdateAsync(ex);
                        }
                    }
                    else
                    {
                        if (ex.IsActive)
                        {
                            ex.IsActive = false;
                            await _projectMemberRepository.UpdateAsync(ex);
                        }
                    }
                }

                // add missing
                foreach (var userId in selected)
                {
                    if (existing.Any(e => e.UserId == userId)) continue;
                    var pm = new ProjectMember
                    {
                        ProjectId = project.Id,
                        UserId = userId,
                        Role = "Member",
                        IsActive = true,
                        CreatedDate = DateTime.UtcNow
                    };
                    await _projectMemberRepository.AddAsync(pm);
                }
            }

            project.ModifiedDate = DateTime.UtcNow;

            var updated = await _projectRepository.UpdateAsync(project);
            var dtoOut = await BuildProjectDto(updated);
            return ServiceResult<ProjectDto>.Success(dtoOut);
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

            var member = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, dto.UserId);
            if (member != null)
            {
                if (member.IsActive)
                    return ServiceResult<ProjectDto>.Success(await BuildProjectDto(project));

                member.IsActive = true;
                member.Role = dto.Role;
                await _projectMemberRepository.UpdateAsync(member);
            }
            else
            {
                var pm = new ProjectMember
                {
                    ProjectId = projectId,
                    UserId = dto.UserId,
                    Role = dto.Role,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await _projectMemberRepository.AddAsync(pm);
            }

            var reloaded = await _projectRepository.GetByIdAsync(projectId);
            return ServiceResult<ProjectDto>.Success(await BuildProjectDto(reloaded ?? project));
        }

        public async Task<ServiceResult<bool>> DeleteAsync(long id)
        {
            var deleted = await _projectRepository.DeleteAsync(id);
            return deleted ? ServiceResult<bool>.Success(true) : ServiceResult<bool>.Failure("Delete failed");
        }

        public async Task<ServiceResult<bool>> RemoveMemberAsync(long projectId, long userId)
        {
            var member = await _projectMemberRepository.GetByProjectAndUserAsync(projectId, userId);
            if (member == null) return ServiceResult<bool>.Missing();

            member.IsActive = false;
            await _projectMemberRepository.UpdateAsync(member);
            return ServiceResult<bool>.Success(true);
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

        private static void SynchronizeMembers(Project project, IEnumerable<long> memberIds)
        {
            var selectedIds = memberIds.ToHashSet();
            // Not used in current implementation as members are managed via repository
        }

        private async Task<ProjectDto> BuildProjectDto(Project project)
        {
            var members = (await _projectMemberRepository.ListByProjectAsync(project.Id)).Where(m => m.IsActive).Select(m => m.UserId).ToArray();
            return new ProjectDto(project.Id, project.ProjectCode, project.ProjectName, project.Description, project.StartDate, project.EndDate, project.Status, project.Priority, project.ProjectManagerId, project.CreatedDate, project.ModifiedDate, members);
        }
    }
}
