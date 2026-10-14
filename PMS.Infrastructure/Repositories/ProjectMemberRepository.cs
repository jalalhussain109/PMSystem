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
    public class ProjectMemberRepository : GenericRepository<ProjectMember, long>, IProjectMemberRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public ProjectMemberRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public new async Task<ProjectMember> AddAsync(ProjectMember member)
        {
            member.CreatedDate = DateTime.UtcNow;
            return await base.AddAsync(member);
        }

        public new async Task<bool> DeleteAsync(long id)
        {
            return await base.DeleteAsync(id);
        }

        public new async Task<ProjectMember?> GetByIdAsync(long id)
        {
            return await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Project)
                .FirstOrDefaultAsync(pm => pm.Id == id);
        }

        public async Task<IEnumerable<ProjectMember>> ListAsync()
        {
            return await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Project)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectMember>> ListByProjectAsync(long projectId)
        {
            return await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Project)
                .Where(pm => pm.ProjectId == projectId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<ProjectMember?> GetByProjectAndUserAsync(long projectId, long userId)
        {
            return await _dbContext.ProjectMembers
                .Include(pm => pm.User)
                .Include(pm => pm.Project)
                .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.UserId == userId);
        }

        public new async Task<ProjectMember> UpdateAsync(ProjectMember member)
        {
            member.ModifiedDate = DateTime.UtcNow;
            return await base.UpdateAsync(member);
        }
    }
}
