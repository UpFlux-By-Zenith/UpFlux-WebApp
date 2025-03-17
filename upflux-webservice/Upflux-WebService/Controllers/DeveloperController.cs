using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Data;
using Upflux_WebService.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Upflux_WebService.Controllers;

/// <summary>
/// API for debugging for developers
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Ensures only Admins can access these endpoints
public class DeveloperController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEntityQueryService _entityQuery;

    public DeveloperController(ApplicationDbContext dbContext, IEntityQueryService entityQuery)
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
    /// <response code="500">Internal Server Error - If an exception occurs.</response>
    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAdmins()
    {
        try
        {
            var admins = await _context.Admin_Details.ToListAsync();

            if (admins.Count == 0)
                return NoContent(); // 204 if there are no admins

            return Ok(admins); // 200 OK with list of admins
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while fetching admins.", Error = ex.Message });
        }
    }

    /// <summary>
    /// Gets details of a specific admin by ID.
    /// </summary>
    /// <param name="id">The ID of the admin.</param>
    /// <returns>The admin details.</returns>
    /// <response code="200">Returns the admin details.</response>
    /// <response code="404">Admin not found.</response>
    /// <response code="401">Unauthorized - Access restricted to Admin role.</response>
    /// <response code="400">Bad Request - Invalid ID format.</response>
    /// <response code="500">Internal Server Error - If an exception occurs.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAdminById(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return BadRequest(new { Message = "Admin ID cannot be empty." });

        try
        {
            var admin = await _context.Admin_Details
                .FirstOrDefaultAsync(a => a.AdminId == id);

            if (admin == null)
                return NotFound(new { Message = "Admin not found." });

            return Ok(admin);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Message = "An error occurred while fetching the admin.", Error = ex.Message });
        }
    }
}