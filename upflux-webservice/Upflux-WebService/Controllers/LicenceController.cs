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
	public class LicenceController : ControllerBase
	{
		#region private members

		private readonly ILicenceManagementService _licenceManagementService;
		private readonly IGeneratedMachineIdService _generatedMachineIdService;

		#endregion

		#region constructor

		public LicenceController(ILicenceManagementService licenceManagementService, IGeneratedMachineIdService generatedMachineIdService)
		{
			_licenceManagementService = licenceManagementService;
			_generatedMachineIdService = generatedMachineIdService;
		}

		#endregion

		#region endpoints

		/// <summary>
		/// Get all licences.
		/// </summary>
		/// <response code="200">List of all licenses</response>
		/// <response code="500">Internal Server Error in case of unexpected errors</response>
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		[HttpGet("admin/all")]
		public async Task<IActionResult> GetAllLicences()
		{
			try
			{
				var licenses = await _licenceManagementService.GetAllLicences();
				return Ok(licenses);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}

		/// <summary>
		/// Get licence by machine ID.
		/// </summary>
		/// <param name="machineId">The ID of the machine</param>
		/// <response code="200">Licence details for the specified machine</response>
		/// <response code="404">Not Found if no licence exists for the machine ID</response>
		/// <response code="500">Internal Server Error in case of unexpected errors</response>
		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
		[HttpGet("admin/{machineId}")]
		public async Task<IActionResult> GetLicenceByMachineId(string machineId)
		{
			try
			{
				var licence = await _licenceManagementService.GetLicenceByMachineId(machineId);
				if (licence == null)
					return NotFound(new { Error = "Licence not found for the specified machine ID." });

				return Ok(licence);
			}
			catch (Exception ex)
			{
				return BadRequest(new { Error = ex.Message });
			}
		}

		/// <summary>
		/// Admin register a machine and create licence for it.
		/// </summary>
		/// <param name="registerMachineDto">the register request containing machineId</param>
		/// <response code="200">Device is registered and licence created</response>
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

				await _licenceManagementService.CreateLicence(registerMachineDto.MachineId);
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
		public async Task<IActionResult> GenerateMachineId()
		{
			try
			{
				var adminEmail = GetClaimValue(ClaimTypes.Email);
				if (string.IsNullOrEmpty(adminEmail))
					return Unauthorized(new { Error = "Invalid admin token." });

				var machineId = Guid.NewGuid().ToString();
				await _generatedMachineIdService.SaveGeneratedMachineId(machineId);

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