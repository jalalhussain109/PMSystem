using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs;

namespace PMS.Application.Interfaces;

public interface IProjectService
{
    Task<ServiceResult<ProjectDto>> CreateAsync(CreateProjectDto dto);
    Task<ServiceResult<ProjectDto>> GetByIdAsync(long id);
    Task<ServiceResult<IEnumerable<ProjectDto>>> ListAsync();
    Task<ServiceResult<PagedResult<ProjectDto>>> ListPagedAsync(int page = 1, int pageSize = 20);
    Task<ServiceResult<ProjectDto>> UpdateAsync(long id, UpdateProjectDto dto);
    Task<ServiceResult<ProjectDto>> ChangeStatusAsync(long projectId, ProjectStatusDto dto);
    Task<ServiceResult<ProjectDto>> AddMemberAsync(long projectId, AddMemberDto dto);
    Task<ServiceResult<bool>> DeleteAsync(long id);
    Task<ServiceResult<bool>> RemoveMemberAsync(long projectId, long userId);
}
