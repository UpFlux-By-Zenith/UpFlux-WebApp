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
        #endregion

        public DataRequestController(IEntityQueryService entityQueryService)
        {
            _entityQueryService = entityQueryService;
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
                return Ok(new { EngineerEmail = engineerEmail, AccessibleMachines = machines });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
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
