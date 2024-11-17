using Microsoft.AspNetCore.Mvc;
using Upflux_WebService.Core.Objects;
using Upflux_WebService.Core.Services;
using System.Threading.Tasks;

namespace Upflux_WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // 1. Admin Login
        [HttpPost("admin-login")]
        public async Task<IActionResult> AdminLogin([FromBody] AdminLoginRequest loginRequest)
        {
            var result = await _authService.AdminLoginAsync(loginRequest);
            if (result == null)
                return Unauthorized("Invalid credentials.");
            return Ok(result);
        }

        // 2. Admin Password Reset
        [HttpPost("admin-reset-password")]
        public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordRequest resetRequest)
        {
            var result = await _authService.AdminResetPasswordAsync(resetRequest);
            if (!result)
                return BadRequest("Password reset failed.");
            return Ok("Password reset successfully.");
        }

        // 3. Engineer Login
        [HttpPost("engineer-login")]
        public async Task<IActionResult> EngineerLogin([FromBody] AdminLoginRequest loginRequest)
        {
            var result = await _authService.EngineerLoginAsync(loginRequest);
            if (result == null)
                return Unauthorized("Invalid credentials.");
            return Ok(result);
        }
    }

    // DTO for Login
    public class AdminLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    // DTO for Password Reset
    public class AdminResetPasswordRequest
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }
}
