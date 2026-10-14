using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;
using System.Linq;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectDto dto)
        {
            var result = await _projectService.CreateAsync(dto);
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });

            var created = result.Data!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int? page = 1, [FromQuery] int? pageSize = 20)
        {
            var pagedResult = await _projectService.ListPagedAsync(page ?? 1, pageSize ?? 20);
            if (pagedResult.NotFound) return NotFound();
            if (!pagedResult.Succeeded) return BadRequest(new { errors = pagedResult.Errors });
            return Ok(pagedResult);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _projectService.GetByIdAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProjectDto dto)
        {
            var result = await _projectService.UpdateAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpPut("{id:long}/status")]
        public async Task<IActionResult> ChangeStatus(long id, [FromBody] ProjectStatusDto dto)
        {
            var result = await _projectService.ChangeStatusAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _projectService.DeleteAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return NoContent();
        }

        [HttpPost("{id:long}/members")]
        public async Task<IActionResult> AddMember(long id, [FromBody] AddMemberDto dto)
        {
            var result = await _projectService.AddMemberAsync(id, dto);
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpDelete("{id:long}/members/{userId:long}")]
        public async Task<IActionResult> RemoveMember(long id, long userId)
        {
            var result = await _projectService.RemoveMemberAsync(id, userId);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return NoContent();
        }
    }
}
