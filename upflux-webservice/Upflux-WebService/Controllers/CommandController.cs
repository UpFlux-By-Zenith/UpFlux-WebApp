using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Upflux_WebService.Services.Interfaces;
using UpFlux_WebService.Protos;

//// Access control not implemented
namespace Upflux_WebService.Controllers
{
	/// <summary>
	/// 
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class CommandController : ControllerBase
	{
		private readonly IControlChannelService _controlChannelService;
		private readonly string _gatewayId;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="controlChannelService"></param>
		/// <param name="configuration"></param>
		public CommandController(IControlChannelService controlChannelService, IConfiguration configuration)
		{
			_controlChannelService = controlChannelService;
			_gatewayId = configuration["GatewayId"]!;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="version"></param>
		/// <param name="targetDevices"></param>
		/// <returns></returns>
		[HttpPost("rollback")]
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
		public async Task<IActionResult> SendRollbackCommand(
			[FromQuery] string version = "",
			[FromBody] string[] targetDevices = null)
		{
			try
			{
				var engineerEmail = GetClaimValue(ClaimTypes.Email);
				var machineIds = GetClaimValue("MachineIds");

				//Ensure claims exist
				if (string.IsNullOrWhiteSpace(engineerEmail) || string.IsNullOrWhiteSpace(machineIds))
					return BadRequest(new { Error = "Invalid claims: Engineer email or machine IDs are missing." });


				if (string.IsNullOrWhiteSpace(_gatewayId))
					return BadRequest(new { message = "Gateway ID is required." });

				targetDevices ??= [];

				await _controlChannelService.SendCommandToGatewayAsync(
					_gatewayId,
					Guid.NewGuid().ToString(),
					CommandType.Rollback,
					version,
					engineerEmail,
					targetDevices
				);

				return Ok(new { message = "Rollback command sent successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while sending the rollback command.", error = ex.Message });
			}
		}

		#region helper methods
		private string? GetClaimValue(string claimType)
		{
			return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
		}
		#endregion
	}
}
