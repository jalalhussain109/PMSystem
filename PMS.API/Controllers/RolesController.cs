using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Domain.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly PMS.Application.Interfaces.IUserRoleService _service;

        public RolesController(PMS.Application.Interfaces.IUserRoleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            var list = await _service.GetRolesWithPermissionsAsync();
            return Ok(list);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name)) return BadRequest("Name is required");
            var created = await _service.CreateRoleAsync(dto.Name);
            return created ? Ok() : Conflict();
        }

        [HttpPut("{roleName}")]
        public async Task<IActionResult> RenameRole(string roleName, [FromBody] RenameRoleDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Name)) return BadRequest("Name is required");
            var result = await _service.RenameRoleAsync(roleName, dto.Name);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return Conflict(new { errors = result.Errors });
            return Ok();
        }

        [HttpDelete("{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            var result = await _service.DeleteRoleAsync(roleName);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return Conflict(new { errors = result.Errors });
            return NoContent();
        }

        [HttpPost("{roleName}/permissions")]
        public async Task<IActionResult> AddPermission(string roleName, [FromBody] PermissionDto dto)
        {
            var added = await _service.AddPermissionToRoleAsync(roleName, dto.Permission);
            return added ? Ok() : Conflict();
        }

        [HttpDelete("{roleName}/permissions")]
        public async Task<IActionResult> RemovePermission(string roleName, [FromBody] PermissionDto dto)
        {
            var removed = await _service.RemovePermissionFromRoleAsync(roleName, dto.Permission);
            return removed ? Ok() : NotFound();
        }

        [HttpPost("/api/users/{userId:long}/roles")]
        public async Task<IActionResult> AssignRoleToUser(long userId, [FromBody] AssignRoleDto dto)
        {
            var ok = await _service.AssignRoleToUserAsync(userId, dto.RoleName);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("/api/users/{userId:long}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRoleFromUser(long userId, string roleName)
        {
            var ok = await _service.RemoveRoleFromUserAsync(userId, roleName);
            return ok ? Ok() : NotFound();
        }

        [HttpGet("/api/users/{userId:long}/permissions")]
        public async Task<IActionResult> GetUserEffectivePermissions(long userId)
        {
            var (roles, perms) = await _service.GetUserEffectivePermissionsAsync(userId);
            return Ok(new { roles, permissions = perms });
        }
    }

    public class CreateRoleDto { public string Name { get; set; } = string.Empty; }
    public class RenameRoleDto { public string Name { get; set; } = string.Empty; }
    public class PermissionDto { public string Permission { get; set; } = string.Empty; }
    public class AssignRoleDto { public string RoleName { get; set; } = string.Empty; }
}
