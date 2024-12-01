using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Upflux_WebService.Core.DTOs;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{
	/// <summary>
	/// Licencing related controllers
	/// </summary>
	[ApiController]
	[Route("api/[controller]")]
	public class LicenseController : ControllerBase
	{
		#region private members

		private readonly ILicenseManagementService _licenseManagementService;

		#endregion

		#region constructor

		public LicenseController(ILicenseManagementService licenseManagementService)
		{
			_licenseManagementService = licenseManagementService;
		}

		#endregion

		#region endpoints

		/// <summary>
		/// Admin register a machine and create license for it.
		/// </summary>
		/// <param name="registerMachineDto">the register request containing machineId</param>
		/// <response code="200">Device is registered and license created</response>
		/// <response code="400">Bad Request if the machine IDs are missing or invalid</response>
		/// <response code="401">Unauthorized if the admin token is invalid or the admin does not have the correct role</response>
		/// <response code="500">Internal Server Error in case of unexpected errors</response>
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		[HttpPost("admin/register")]
		public async Task<IActionResult> RegisterMachine([FromBody] RegisterMachineRequest registerMachineDto)
		{
			try
			{
				var adminEmail = GetClaimValue(ClaimTypes.Email);
				if (string.IsNullOrEmpty(adminEmail))
					return Unauthorized(new { Error = "Invalid admin token." });

				await _licenseManagementService.CreateLicense(registerMachineDto.MachineId);
				return Ok(new { Message = "Machine Registered Succesfully." });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}

		/// <summary>
		/// Generate UUID that can used as a device's identifier
		/// </summary>
		/// <response code="401">Unauthorized if the admin token is invalid or the admin does not have the correct role</response>
		/// <response code="500">Internal Server Error in case of unexpected errors</response>
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		[HttpGet("admin/generateId")]
		public IActionResult GenerateMachineId()
		{
			try
			{
				var adminEmail = GetClaimValue(ClaimTypes.Email);
				if (string.IsNullOrEmpty(adminEmail))
					return Unauthorized(new { Error = "Invalid admin token." });

				var machineId = Guid.NewGuid().ToString();
				return Ok(new { MachineId = machineId });
			}
			catch (Exception ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}

		#endregion

		#region private methods

		// Helper method to get claim value
		private string? GetClaimValue(string claimType)
		{
			return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
		}
		#endregion
	}
}