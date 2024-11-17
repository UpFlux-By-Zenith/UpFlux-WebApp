using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Upflux_WebService.Core.Interfaces;

namespace Upflux_WebService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineerController : ControllerBase
    {
        private readonly ITokenService _tokenService; // Assume TokenService handles token validation

        public EngineerController(ITokenService tokenService)
        {
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] EngineerLoginDTO request)
        {
            // Validate the token passed by the engineer
            var token = request.JwtToken;
            var principal = _tokenService.ValidateToken(token);

            if (principal == null)
                return Unauthorized("Invalid token");

            // Extract claims and use for further authorization
            var email = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = principal.FindFirst(ClaimTypes.Role)?.Value;
            var machineIds = principal.FindFirst("MachineIds")?.Value?.Split(',').Select(int.Parse).ToList();

            // Check if the role is Engineer
            if (role != "Engineer")
                return Unauthorized("Unauthorized role");

            // Proceed with the request (you may want to retrieve engineer data or proceed with their actions)
            return Ok(new { Email = email, Machines = machineIds });
        }
    }

    public class EngineerLoginDTO
    {
        public string Email { get; set; }
        public string JwtToken { get; set; }
    }
}
