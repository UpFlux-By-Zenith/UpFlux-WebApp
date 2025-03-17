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
    /// Retrieves a list of all users from the database.
    /// </summary>
    /// <returns>A list of users if found, otherwise a 404 Not Found response.</returns>
    [HttpGet("/users")] // Defines the HTTP GET endpoint at "/users".
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        // Fetch all users from the database
        var users = _context.Users.ToList();

        // If no users are found, return a 404 Not Found response
        if (users == null || !users.Any()) return NotFound("No users found.");

        // Return the list of users with a 200 OK response
        return Ok(users);
    }

    /// <summary>
    /// Revokes an engineer's token, preventing further access.
    /// </summary>
    /// <param name="engineerId">The ID of the engineer whose token is being revoked.</param>
    /// <param name="adminId">The ID of the admin performing the revocation.</param>
    /// <param name="reason">Optional reason for revocation.</param>
    /// <returns>Returns an HTTP response indicating success or failure.</returns>
    [HttpPost("/revoke-engineer")]
    public IActionResult RevokeEngineerToken(string engineerId, string adminId, string? reason = null)
    {
        try
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(engineerId) || string.IsNullOrEmpty(adminId))
                return BadRequest("Engineer ID and Admin ID are required.");

            // Check if the engineer exists in the Users table
            var engineerExists = _context.Users.Any(u => u.UserId == engineerId);
            if (!engineerExists)
                return NotFound("Engineer not found.");

            // Check if the admin exists in the Admin_Details table
            var adminExists = _context.Admin_Details.Any(a => a.AdminId == adminId);
            if (!adminExists)
                return NotFound("Admin not found.");

            // Create a new revocation entry
            var revocation = new Revokes
            {
                UserId = engineerId,
                RevokedBy = adminId,
                RevokedAt = DateTime.UtcNow,
                Reason = reason
            };

            // Add the revocation entry to the database
            _context.Revokes.Add(revocation);
            _context.SaveChanges();

            // Logging (for debugging purposes)
            Console.WriteLine($"Token revoked: Engineer {engineerId} by Admin {adminId}");

            // Return success response
            return Ok(new { message = "Engineer token revoked successfully." });
        }
        catch (Exception ex)
        {
            // Log the error message
            Console.WriteLine($"Error revoking token: {ex.Message}");

            // Return a 500 Internal Server Error response
            return StatusCode(500, "An error occurred while revoking the token.");
        }
    }

    /// <summary>
    /// Removes an engineer's token revocation, restoring access.
    /// </summary>
    /// <param name="engineerId">The ID of the engineer whose revocation is being removed.</param>
    /// <param name="adminId">The ID of the admin performing the removal.</param>
    /// <returns>Returns an HTTP response indicating success or failure.</returns>
    [HttpDelete("/reinstate-engineer")]
    public IActionResult ReinstateEngineerToken(string engineerId, string adminId)
    {
        try
        {
            // Validate required parameters
            if (string.IsNullOrEmpty(engineerId) || string.IsNullOrEmpty(adminId))
                return BadRequest("Engineer ID and Admin ID are required.");

            // Check if the engineer exists in the Users table
            var engineerExists = _context.Users.Any(u => u.UserId == engineerId);
            if (!engineerExists)
                return NotFound("Engineer not found.");

            // Check if the admin exists in the Admin_Details table
            var adminExists = _context.Admin_Details.Any(a => a.AdminId == adminId);
            if (!adminExists)
                return NotFound("Admin not found.");

            // Check if the engineer's token is revoked
            var revokedToken = _context.Revokes.FirstOrDefault(r => r.UserId == engineerId);
            if (revokedToken == null)
                return NotFound("This engineer's token is not revoked.");

            // Remove the revocation entry
            _context.Revokes.Remove(revokedToken);
            _context.SaveChanges();

            // Logging (for debugging purposes)
            Console.WriteLine($"Revocation removed: Engineer {engineerId} by Admin {adminId}");

            // Return success response
            return Ok(new { message = "Engineer token revocation removed successfully." });
        }
        catch (Exception ex)
        {
            // Log the error message
            Console.WriteLine($"Error reinstating token: {ex.Message}");

            // Return a 500 Internal Server Error response
            return StatusCode(500, "An error occurred while reinstating the token.");
        }
    }


    [HttpGet("machinesWithLicenses")]
    public async Task<IActionResult> GetAllMachinesWithLicenses()
    {
        var machinesWithLicense = await _entityQuery.GetAllMachinesWithLicences();
        return Ok(machinesWithLicense);
    }
}