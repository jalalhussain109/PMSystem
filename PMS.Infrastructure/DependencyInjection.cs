using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PMS.Domain.Interfaces;
using PMS.Infrastructure.Data;
using PMS.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using PMS.Domain.Entities;
using PMS.Infrastructure.Repositories;

namespace PMS.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Infrastructure repositories
            services.AddScoped<IIdentityUserRepository, IdentityUserRepository>();

            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<PMS.Domain.Interfaces.IProjectMemberRepository, PMS.Infrastructure.Repositories.ProjectMemberRepository>();
            // Identity
            services.AddScoped<PMS.Application.Interfaces.IUserRoleService, PMS.Infrastructure.Identity.UserRoleService>();
           
            // Register open-generic repository mapping
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            return services;
        }
    }
}
