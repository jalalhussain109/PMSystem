using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PMS.Domain.Entities;
using PMS.Domain.Interfaces;
using PMS.Infrastructure.Data;

namespace PMS.Infrastructure.Repositories
{
    public class ProjectRepository : GenericRepository<Project, long>, IProjectRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProjectRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public new async Task<Project> AddAsync(Project project)
        {
            project.CreatedDate = DateTime.Now;
            return await base.AddAsync(project);
        }

        public new async Task<bool> DeleteAsync(long id)
        {
            return await base.DeleteAsync(id);
        }

        public new async Task<Project?> GetByIdAsync(long id)
        {
            return await _dbContext.Projects
                .FirstOrDefaultAsync(project => project.Id == id);
        }

        public new async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _dbContext.Projects
                .AsNoTracking()
                .ToListAsync();
        }

        public new async Task<Project> UpdateAsync(Project project)
        {
            project.ModifiedDate = DateTime.Now;
            return await base.UpdateAsync(project);
        }
    }
}
