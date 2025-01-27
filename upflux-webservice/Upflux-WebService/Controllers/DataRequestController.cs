using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{
    /// <summary>
    /// Engineer related endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
   
    public class DataRequestController : ControllerBase
    {

        #region private members 

        private readonly IEntityQueryService _entityQueryService;
        private readonly string _gatewayId;
        private readonly IControlChannelService _controlChannelService;

		#endregion

		public DataRequestController(IEntityQueryService entityQueryService, IConfiguration configuration, IControlChannelService controlChannelService)
        {
            _entityQueryService = entityQueryService;
			_gatewayId = configuration["GatewayId"]!;
            _controlChannelService = controlChannelService;
		}

		[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
        [HttpGet("applications")]
        public IActionResult GetApplications()
        {
            try
            {
                List<Application> applications = _entityQueryService.GetApplicationsWithVersionsAsync().Result;
                return Ok(new { applications });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
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
                return Ok(new { EngineerEmail = engineerEmail, AccessibleMachines = _entityQueryService.GetListOfMachines(machines) });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Get applications running on machines
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Engineer")]
        [HttpGet("engineer/machines-application")]
		public async Task<IActionResult> GetRunningApplications()
		{
			try
			{
                var engineerEmail = GetClaimValue(ClaimTypes.Email);
                var machineIds = GetClaimValue("MachineIds");

                //Ensure claims exist

                if (string.IsNullOrWhiteSpace(engineerEmail) || string.IsNullOrWhiteSpace(machineIds))
                {
                    return BadRequest(new { Error = "Invalid claims: Engineer email or machine IDs are missing." });
                }

                // Send version data request via control channel service
                await _controlChannelService.SendVersionDataRequestAsync(_gatewayId);

				// Return a meaningful response to the client
				return Ok(new
				{
					Message = "Version data request sent successfully.",
                    EngineerEmail = engineerEmail,
                    Timestamp = DateTime.UtcNow
				});
			}
			catch (Exception ex)
			{
				return StatusCode(500, new
				{
					Error = "An unexpected error occurred while processing the request.",
					Details = ex.Message
				});
			}
		}

		#region private methods

		// Helper method to get claim value
		private string? GetClaimValue(string claimType)
        {
            return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        #endregion
    }
}
