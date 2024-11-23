using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Admin login and token generation
        /// </summary>
        [HttpPost("admin/login")]
        public IActionResult AdminLogin([FromBody] AdminLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { Error = "Email and Password are required." });

            try
            {
                var token = _authService.AdminLogin(request.Email, request.Password);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Engineer login to retrieve machine access
        /// </summary>
        [HttpPost("engineer/login")]
        public IActionResult EngineerLogin([FromBody] EngineerLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.EngineerToken))
                return BadRequest(new { Error = "Email and token are required." });

            try
            {
                // Parse and validate the engineer token
                var tokenData = _authService.ParseToken(request.EngineerToken);

                // Ensure the token's email matches the provided email
                if (!tokenData.TryGetValue(ClaimTypes.Email, out var tokenEmail) || tokenEmail != request.Email)
                    return Unauthorized(new { Error = "Invalid token for the provided email." });

                // Retrieve machine IDs from the token
                if (!tokenData.TryGetValue("MachineIds", out var machineIds))
                    return Unauthorized(new { Error = "Invalid token: no machine IDs found." });

                // Generate a new authorization token for the engineer
                var authToken = _authService.GenerateEngineerToken(request.Email, machineIds.Split(',').ToList());

                return Ok(new { Token = authToken });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Error = "Invalid token: " + ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        /// <summary>
        /// Admin creates a token for an engineer
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        [HttpPost("admin/create-engineer-token")]
        public IActionResult CreateEngineerToken([FromBody] EngineerTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.EngineerEmail) || request.MachineIds == null || !request.MachineIds.Any())
                return BadRequest(new { Error = "EngineerEmail and MachineIds are required." });

            try
            {
                var adminEmail = GetClaimValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(adminEmail))
                    return Unauthorized(new { Error = "Invalid admin token." });

                var engineerToken = _authService.GenerateEngineerToken(request.EngineerEmail, request.MachineIds);
                return Ok(new { EngineerToken = engineerToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Engineer retrieves accessible machines
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Engineer")]
        [HttpGet("engineer/access-machines")]
        public IActionResult GetAccessibleMachines()
        {
            try
            {
                var engineerEmail = GetClaimValue(ClaimTypes.Email);
                var machineIds = GetClaimValue("MachineIds");

                if (string.IsNullOrEmpty(engineerEmail) || string.IsNullOrEmpty(machineIds))
                    return Unauthorized(new { Error = "Invalid engineer token." });

                var machines = machineIds.Split(',').ToList();
                return Ok(new { EngineerEmail = engineerEmail, AccessibleMachines = machines });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Parse and verify a token
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("parse-token")]
        public IActionResult ParseToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { Error = "Token is required." });

            try
            {
                var tokenData = _authService.ParseToken(token);
                return Ok(tokenData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Shared endpoint for Admin and Engineer roles
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
        [HttpGet("shared-endpoint")]
        public IActionResult SharedEndpoint()
        {
            var email = GetClaimValue(ClaimTypes.Email);
            var role = GetClaimValue(ClaimTypes.Role);

            return Ok(new { Email = email, Role = role });
        }




        // Helper method to get claim value
        private string GetClaimValue(string claimType)
        {
            return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
    }

    // Request DTOs
    public class AdminLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class EngineerTokenRequest
    {
        public string EngineerEmail { get; set; }
        public string EnginnerName { get; set; }
        public List<string> MachineIds { get; set; }
    }

    public class EngineerLoginRequest
    {
        public string Email { get; set; }
        public string EngineerToken { get; set; }
    }

}
