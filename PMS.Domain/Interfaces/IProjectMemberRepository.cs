using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Domain.Entities;

namespace PMS.Domain.Interfaces
{
    public interface IProjectMemberRepository : IGenericRepository<ProjectMember, long>
    {
    
        Task<IEnumerable<ProjectMember>> ListAsync();
        Task<IEnumerable<ProjectMember>> ListByProjectAsync(long projectId);
        Task<ProjectMember?> GetByProjectAndUserAsync(long projectId, long userId);
    }
}
