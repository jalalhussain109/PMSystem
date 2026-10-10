using Azure.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PMS.Application.DTOs;
using PMS.Application.Interfaces;
using PMS.Domain.Entities;

namespace PMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
        {
            var result = await _userService.CreateAsync(dto);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            var created = result.Data!;
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _userService.GetAllAsync();
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            var users = result.Data ?? new List<UserDto>();
            return Ok(users);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetById(long id)
        {
            var result = await _userService.GetByIdAsync(id);
            if (result.NotFound)
                return NotFound(new { message = "User not found." });
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            var user = result.Data!;
            return Ok(user);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(id, dto);
            if (result.NotFound)
                return NotFound();
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors });

            return Ok(result.Data);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            var result = await _userService.DeleteAsync(id);
            if (result.NotFound) return NotFound();
            if (!result.Succeeded) return BadRequest(new { errors = result.Errors });
            return NoContent();
        }
    }
}
