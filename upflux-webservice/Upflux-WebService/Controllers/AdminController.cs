using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers;

/// <summary>
/// API for managing Admins. Accessible only by Admins.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEntityQueryService _entityQuery;

    public AdminController(ApplicationDbContext dbContext, IEntityQueryService entityQuery)
    {
        _context = dbContext;
        _entityQuery = entityQuery;
    }

    /// <summary>
    /// Gets a list of all admins.
    /// </summary>
    /// <returns>A list of admins.</returns>
    /// <response code="200">Returns the list of admins.</response>
    /// <response code="401">Unauthorized - Access restricted to Admin role.</response>
    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAdmins()
    {
        var admins = _context.Admin_Details.ToList();
        return Ok(admins);
    }

    /// <summary>
    /// Gets details of a specific admin by ID.
    /// </summary>
    /// <param name="id">The ID of the admin.</param>
    /// <returns>The admin details.</returns>
    /// <response code="200">Returns the admin details.</response>
    /// <response code="404">Admin not found.</response>
    /// <response code="401">Unauthorized - Access restricted to Admin role.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAdminById(Guid id)
    {
        var admin = await _context.Admin_Details.FindAsync(id);
        if (admin == null)
            return NotFound(new { Message = "Admin not found." });
        return Ok(admin);
    }

    [HttpGet("engineers")]
    public async Task<IActionResult> GetEngineers()
    {
        var engineers = await _entityQuery.GetAllEngineers();
        if (engineers == null)
            return NoContent();
        return Ok(engineers);
    }

    [HttpGet("machinesWithLicenses")]
    public async Task<IActionResult> GetAllMachinesWithLicenses()
    {
        var machinesWithLicense = await _entityQuery.GetAllMachinesWithLicences();
        return Ok(machinesWithLicense);
    }

    // TODO: API Cleanup (Regarding application table changes)
    //[HttpGet("machines/applications")]
    //public async Task<IActionResult> GetAllMachinesWithApplications()
    //{
    //    var machinesWithApplications = await _entityQuery.GetListOfMachinesWithApplications();
    //    return Ok(machinesWithApplications);
    //}
}