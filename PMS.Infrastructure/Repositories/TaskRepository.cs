using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories
{
    public class TaskRepository : GenericRepository<TaskItem, long>, ITaskRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TaskRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public new async Task<TaskItem> AddAsync(TaskItem entity)
        {
            entity.CreatedDate = System.DateTime.UtcNow;
            return await base.AddAsync(entity);
        }

        public new async Task<bool> DeleteAsync(long id)
        {
            return await base.DeleteAsync(id);
        }

        public new async Task<TaskItem?> GetByIdAsync(long id)
        {
            return await _dbContext.Tasks
                .Include(t => t.SubTasks)
                .Include(t => t.AssignedUser)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public new async Task<IEnumerable<TaskItem>> GetAllAsync()
        {
            return await _dbContext.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.Project)
                .AsNoTracking()
                .ToListAsync();
        }

        public new async Task<TaskItem> UpdateAsync(TaskItem entity)
        {
            return await base.UpdateAsync(entity);
        }
    }
}
