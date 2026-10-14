using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.Common;
using PMS.Application.Interfaces;
using System;
using System.Linq;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ITaskService _taskService;
        private readonly IUserService _userService;

        public DashboardController(IProjectService projectService, ITaskService taskService, IUserService userService)
        {
            _projectService = projectService;
            _taskService = taskService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // Active projects count
            var projectsResult = await _projectService.ListAsync();
            var activeProjects = 0;
            if (projectsResult.Succeeded && projectsResult.Data != null)
            {
                activeProjects = projectsResult.Data.Count();
            }

            // Open tasks count
            var openTasksResult = await _taskService.ListAsync(page:1, pageSize:100, status: "Open");
            var openTasks = 0;
            if (openTasksResult.Succeeded && openTasksResult.Data != null)
            {
                openTasks = (int)openTasksResult.Data.TotalCount;
            }
            // Open tasks count
            var inProgressTasksResult = await _taskService.ListAsync(page: 1, pageSize: 100, status: "InProgress");
            if (inProgressTasksResult.Succeeded && inProgressTasksResult.Data != null)
            {
                openTasks += (int)inProgressTasksResult.Data.TotalCount;
            }

            // Due this week: count tasks with DueDate within next 7 days
            var now = DateTime.UtcNow.Date;
            var weekEnd = now.AddDays(7);
            var dueThisWeekResult = await _taskService.ListAsync(page:1, pageSize:1, dueFrom: now, dueTo: weekEnd);
            var dueThisWeek = 0;
            if (dueThisWeekResult.Succeeded && dueThisWeekResult.Data != null)
            {
                dueThisWeek = (int)dueThisWeekResult.Data.TotalCount;
            }

            // Team members: total users
            var usersResult = await _userService.GetAllAsync();
            var teamMembers = 0;
            if (usersResult.Succeeded && usersResult.Data != null)
            {
                teamMembers = usersResult.Data.Count;
            }

            var dto = new
            {
                ActiveProjects = activeProjects,
                OpenTasks = openTasks,
                DueThisWeek = dueThisWeek,
                TeamMembers = teamMembers
            };

            return Ok(dto);
        }
    }
}
