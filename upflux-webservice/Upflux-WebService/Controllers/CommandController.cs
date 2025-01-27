using Microsoft.AspNetCore.Mvc;
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
		public async Task<IActionResult> SendRollbackCommand(
			[FromQuery] string version = "",
			[FromBody] string[] targetDevices = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(_gatewayId))
					return BadRequest(new { message = "Gateway ID is required." });

				targetDevices ??= [];

				await _controlChannelService.SendCommandToGatewayAsync(
					_gatewayId,
					Guid.NewGuid().ToString(),
					CommandType.Rollback,
					version,
					targetDevices
				);

				return Ok(new { message = "Rollback command sent successfully." });
			}
			catch (Exception ex)
			{
				return StatusCode(500, new { message = "An error occurred while sending the rollback command.", error = ex.Message });
			}
		}
	}
}
