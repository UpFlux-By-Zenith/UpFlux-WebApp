using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Services;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers;

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
    private readonly ApplicationDbContext _context;
    private readonly IAuthService _authService;

    #endregion

    public DataRequestController(IEntityQueryService entityQueryService, IConfiguration configuration,
        IControlChannelService controlChannelService, ApplicationDbContext context, IAuthService authService)
    {
        _context = context;
        _entityQueryService = entityQueryService;
        _gatewayId = configuration["GatewayId"]!;
        _controlChannelService = controlChannelService;
        _authService = authService;
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    [HttpGet("machines/status")]
    public IActionResult GetDeviceStatus()
    {
        try
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            if (_authService.CheckIfUserIsRevoked(email)) return Unauthorized();

            var machineIds = GetClaimValue("MachineIds");
            var machines = machineIds.Split(',').ToList();
            return Ok(_context.Machine_Status.Where(m => machines.Contains(m.MachineId)).ToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching applications: {ex.Message}");
            return StatusCode(500, new { Error = "An internal error occurred." });
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    [HttpGet("machines/storedVersions")]
    public IActionResult GetStoredVersions()
    {
        try
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            if (_authService.CheckIfUserIsRevoked(email)) return Unauthorized();
            _controlChannelService.SendVersionDataRequestAsync("gateway-patrick-1234");
            return Ok(_context.MachineStoredVersions.ToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching applications: {ex.Message}");
            return StatusCode(500, new { Error = "An internal error occurred." });
        }
    }

    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    [HttpGet("applications")]
    public IActionResult GetApplications()
    {
        try
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
            if (_authService.CheckIfUserIsRevoked(email)) return Unauthorized();
            // Fetch applications and join with their corresponding versions
            var applications = _context.Application_Versions.ToList();

            // Check if applications exist
            if (!applications.Any())
                return NotFound(new { Message = "No applications found." });

            return Ok(applications);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching applications: {ex.Message}");
            return StatusCode(500, new { Error = "An internal error occurred." });
        }
    }

    /// <summary>
    /// Engineer retrieves accessible machines
    /// </summary>
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    [HttpGet("engineer/access-machines")]
    public IActionResult GetAccessibleMachines()
    {
        try
        {
            var engineerEmail = GetClaimValue(ClaimTypes.Email);
            var machineIds = GetClaimValue("MachineIds");
            if (_authService.CheckIfUserIsRevoked(engineerEmail)) return Unauthorized();
            var role = GetClaimValue(ClaimTypes.Role);

            // C# Query to fetch all Machines with UserName for Admin
            var allMachines = (
                from m in _context.Machines
                join u in _context.Users on m.lastUpdatedBy equals u.UserId into userGroup
                from user in userGroup.DefaultIfEmpty()
                select new
                {
                    m.MachineId,
                    m.machineName,
                    m.ipAddress,
                    m.currentVersion,
                    m.dateAddedOn,
                    m.appName,
                    LastUpdatedBy = user != null ? user.Name : "Unknown"
                }
            ).ToListAsync();

            if (role == "Admin") return Ok(new { engineerEmail, AccessibleMachines = allMachines });

            if (string.IsNullOrEmpty(engineerEmail) || string.IsNullOrEmpty(machineIds))
                return Unauthorized(new { Error = "Invalid engineer token." });

            var machines = machineIds.Split(',').ToList();

            return Ok(new
            {
                EngineerEmail = engineerEmail,
                AccessibleMachines = _context.Machines
                    .Where(m => machines.Contains(m.MachineId))
                    .Select(m => new
                    {
                        m.MachineId,
                        m.machineName,
                        m.ipAddress,
                        m.currentVersion,
                        m.dateAddedOn,
                        m.appName,
                        LastUpdatedBy = _context.Users
                            .Where(u => u.UserId == m.lastUpdatedBy)
                            .Select(u => u.Name)
                            .FirstOrDefault()
                    })
                    .ToListAsync()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get applications running on machines
    /// </summary>
    /// TODO: Delete.. Marked as Depricated
    /// This endpoint is to get the gateway's version data, this does not get data from the cloud database and return it.
    /// this is needed will change name 
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
    [HttpGet("engineer/machines-application")]
    public async Task<IActionResult> GetRunningApplications()
    {
        try
        {
            var machineIds = GetClaimValue("MachineIds").Split(",");
            var role = GetClaimValue(ClaimTypes.Role);


            // Send version data request via control channel service
            await _controlChannelService.SendVersionDataRequestAsync(_gatewayId);

            // return Ok(new
            // {
            // 	Message = "Version data request sent successfully.",
            // 	//EngineerEmail = engineerEmail,
            // 	Timestamp = DateTime.UtcNow
            // });

            var storedVersionsList = new List<MachineStoredVersion>();

            if (role == "Engineer")
                storedVersionsList = await _context.MachineStoredVersions
                    .Where(msv => machineIds.Contains(msv.MachineId.ToString()))
                    .ToListAsync();
            else if (role == "Admin") storedVersionsList = await _context.MachineStoredVersions.ToListAsync();

            return Ok(storedVersionsList);
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