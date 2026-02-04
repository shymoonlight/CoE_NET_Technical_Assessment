using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using TA_API.Models.DTOs;
using TA_API.Services.Interfaces;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // GET: api/user
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> Get()
        {
            var userRole = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;
            var userId = 0;
            int.TryParse(User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value, out userId);

            var dtos = await _userService.GetUsersAsync(userId, userRole);
            return Ok(dtos);
        }

        // GET: api/user/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> Get(int id)
        {
            var userIdStr = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdStr, out var parsed) ? parsed : null;
            var userRole = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;

            try
            {
                var dto = await _userService.GetByIdAsync(id, userId, userRole);
                if (dto is null) return NotFound();
                return Ok(dto);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "You don't have permissions." });
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> Post([FromBody] UserDto userDto)
        {
            var userRole = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var created = await _userService.CreateUserAsync(userDto, userRole);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "You don't have permissions." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] UserDto userDto)
        {
            var userIdStr = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.NameIdentifier)?.Value;
            int? userId = int.TryParse(userIdStr, out var parsed) ? parsed : null;
            var userRole = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;

            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var updated = await _userService.UpdateUserAsync(id, userDto, userId, userRole);
                if (!updated) return NotFound();
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "You don't have permissions." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userRole = User.Claims.FirstOrDefault(u => u.Type == ClaimTypes.Role)?.Value;

            try
            {
                var deleted = await _userService.DeleteUserAsync(id, userRole);
                if (!deleted) return NotFound();
                return Ok();
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode((int)HttpStatusCode.Forbidden, new { error = "You don't have permissions." });
            }
        }
    }
}