using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs.ProjectMembers;
using PMS.Application.Interfaces;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProjectMembersController : ControllerBase
    {
        private readonly IProjectMemberService _service;

        public ProjectMembersController(IProjectMemberService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProjectMemberDto dto)
        {
            var result = await _service.CreateAsync(dto);
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            var created = result.Data!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var result = await _service.ListAsync();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpGet("project/{projectId:long}")]
        public async Task<IActionResult> ListByProject(long projectId)
        {
            var result = await _service.ListByProjectAsync(projectId);
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateProjectMemberDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return Ok(result.Data);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _service.DeleteAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return NoContent();
        }
    }
}
