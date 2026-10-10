using System.Collections.Generic;
using System.Threading.Tasks;
using PMS.Domain.Entities;

namespace PMS.Domain.Repositories
{
    public interface ITaskRepository
    {
        Task<TaskItem?> GetByIdAsync(long id);
        Task<IEnumerable<TaskItem>> ListAsync();
        Task<TaskItem> AddAsync(TaskItem entity);
        Task<TaskItem> UpdateAsync(TaskItem entity);
        Task<bool> DeleteAsync(long id);
    }
}
