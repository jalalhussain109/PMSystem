using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Domain.Entities;

namespace PMS.Domain.Interfaces
{
    public interface IProjectMemberRepository
    {
        Task<ProjectMember?> GetByIdAsync(long id);
        Task<IEnumerable<ProjectMember>> ListAsync();
        Task<IEnumerable<ProjectMember>> ListByProjectAsync(long projectId);
        Task<ProjectMember?> GetByProjectAndUserAsync(long projectId, long userId);
        Task<ProjectMember> AddAsync(ProjectMember member);
        Task<ProjectMember> UpdateAsync(ProjectMember member);
        Task<bool> DeleteAsync(long id);
    }
}
