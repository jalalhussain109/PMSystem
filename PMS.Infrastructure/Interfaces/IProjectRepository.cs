using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Domain.Entities;

namespace PMS.Infrastructure.Interfaces
{
    public interface IProjectRepository
    {
        Task<Project?> GetByIdAsync(long id);
        Task<IEnumerable<Project>> ListAsync();
        Task<Project> AddAsync(Project project);
        Task<Project> UpdateAsync(Project project);
        Task<bool> DeleteAsync(long id);
    }
}
