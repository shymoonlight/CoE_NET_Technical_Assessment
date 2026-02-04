using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TA_API.Models.Data;
using TA_API.Models.DTOs;
using TA_API.Services.Auth;

namespace TA_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly IAuthService _authService;

        public LoginController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// POST api/login
        /// Body: { "userName": "...", "password": "..." }
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            if (request is null)
            {
                return BadRequest();
            }

            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            var result = await _authService.AuthenticateAsync(request.UserName, request.Password);

            if (result is null)
            {
                return Unauthorized(new { message = "Invalid credentials." });
            }

            return Ok(result);
        }
    }
}