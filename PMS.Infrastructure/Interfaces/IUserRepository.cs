using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetByIdAsync(long id);
        Task<IEnumerable<ApplicationUser>> ListAsync();
        Task<ApplicationUser> AddAsync(ApplicationUser entity);
        Task<ApplicationUser> UpdateAsync(ApplicationUser entity);
        Task<bool> DeleteAsync(long id);
    }
}
