using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpPost("projects/{projectId:long}/tasks")]
        public async Task<IActionResult> CreateTask(long projectId, [FromBody] CreateTaskDto dto)
        {
            var result = await _taskService.CreateAsync(projectId, dto);
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            var created = result.Data!;
            return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
        }

        [HttpGet("tasks/{id:long}")]
        public async Task<IActionResult> GetTask(long id)
        {
            var result = await _taskService.GetByIdAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        [HttpPut("tasks/{id:long}")]
        public async Task<IActionResult> UpdateTask(long id, [FromBody] UpdateTaskDto dto)
        {
            var result = await _taskService.UpdateAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        [HttpPost("tasks/{id:long}/subtasks")]
        public async Task<IActionResult> CreateSubtask(long id, [FromBody] CreateTaskDto dto)
        {
            var result = await _taskService.CreateSubtaskAsync(id, dto);
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            var created = result.Data!;
            return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
        }

        [HttpPut("tasks/{id:long}/status")]
        public async Task<IActionResult> ChangeStatus(long id, [FromBody] ChangeTaskStatusDto dto)
        {
            var result = await _taskService.ChangeStatusAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }
    }
}
