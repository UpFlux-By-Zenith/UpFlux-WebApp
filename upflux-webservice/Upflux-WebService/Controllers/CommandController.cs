using Microsoft.AspNetCore.Mvc;
using Upflux_WebService.Services.Interfaces;
using UpFlux_WebService.Protos;

namespace Upflux_WebService.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CommandController : ControllerBase
	{
		private readonly IControlChannelService _controlChannelService;

		public CommandController(IControlChannelService controlChannelService)
		{
			_controlChannelService = controlChannelService;
		}

		/// <summary>
		/// Sends a rollback command to a specified gateway and target devices.
		/// </summary>
		/// <param name="gatewayId">The ID of the gateway to send the command to.</param>
		/// <param name="commandId">The unique ID of the command.</param>
		/// <param name="parameters">Optional parameters for the rollback command.</param>
		/// <param name="targetDevices">A list of device UUIDs to target.</param>
		/// <returns>An HTTP response indicating the result of the operation.</returns>
		[HttpPost("rollback")]
		public async Task<IActionResult> SendRollbackCommand(
			[FromQuery] string gatewayId,
			[FromQuery] string version = "",
			[FromBody] string[] targetDevices = null)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(gatewayId))
					return BadRequest(new { message = "Gateway ID is required." });

				targetDevices ??= Array.Empty<string>();

				// Call the ControlChannelService to send the rollback command
				await _controlChannelService.SendCommandToGatewayAsync(
					gatewayId,
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
