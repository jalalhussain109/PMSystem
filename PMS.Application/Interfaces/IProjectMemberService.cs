using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Application.Common;
using PMS.Application.DTOs.ProjectMembers;

namespace PMS.Application.Interfaces
{
    public interface IProjectMemberService
    {
        Task<ServiceResult<ProjectMemberDto>> CreateAsync(CreateProjectMemberDto dto);
        Task<ServiceResult<ProjectMemberDto>> GetByIdAsync(long id);
        Task<ServiceResult<IEnumerable<ProjectMemberDto>>> ListAsync();
        Task<ServiceResult<ProjectMemberDto>> UpdateAsync(long id, UpdateProjectMemberDto dto);
        Task<ServiceResult<bool>> DeleteAsync(long id);
    }
}
