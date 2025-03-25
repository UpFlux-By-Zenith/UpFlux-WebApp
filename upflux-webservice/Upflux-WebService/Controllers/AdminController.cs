using System.Security.Claims;
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
    [HttpGet("users")] // Defines the HTTP GET endpoint at "/users".
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
    /// <param name="reason">Optional reason for revocation.</param>
    /// <returns>Returns an HTTP response indicating success or failure.</returns>
    [HttpPost("revoke-engineer")]
    public IActionResult RevokeEngineerToken([FromBody] RevokeEngineerRequest request)
    {
        try
        {
            // Get the admin's email from the claims
            var adminEmail = GetClaimValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(adminEmail))
                return Unauthorized("Admin email not found in claims.");

            // Validate required parameters
            if (string.IsNullOrEmpty(request.engineerId))
                return BadRequest("Engineer ID is required.");

            // Check if the admin engineer exists
            var engineer = _context.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (engineer == null)
                return NotFound("Engineer not found.");

            var admin = _context.Admin_Details.FirstOrDefault(a => a.UserId == engineer.UserId);
            var adminUser = _context.Users.FirstOrDefault(u => u.UserId == admin.UserId && u.Email == adminEmail);

            if (admin == null || adminUser == null)
                return NotFound("Admin not found or unauthorized.");

            // Create a new revocation entry
            var revocation = new Revokes
            {
                UserId = request.engineerId,
                RevokedBy = admin.AdminId,
                RevokedAt = DateTime.UtcNow,
                Reason = request.reason
            };

            // Add and save the revocation
            _context.Revokes.Add(revocation);
            _context.SaveChanges();

            Console.WriteLine($"Token revoked: Engineer {request.engineerId} by Admin {admin.AdminId}");

            return Ok(new { message = "Engineer token revoked successfully." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error revoking token: {ex.Message}");
            return StatusCode(500, "An error occurred while revoking the token.");
        }
    }

    /// <summary>
    /// Removes an engineer's token revocation, restoring access.
    /// </summary>
    /// <param name="engineerId">The ID of the engineer whose revocation is being removed.</param>
    /// <param name="adminId">The ID of the admin performing the removal.</param>
    /// <returns>Returns an HTTP response indicating success or failure.</returns>
    [HttpDelete("reinstate-engineer")]
    public IActionResult ReinstateEngineerToken(string engineerId)
    {
        try
        {
            // Get the admin's email from the claims
            var adminEmail = GetClaimValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(adminEmail))
                return Unauthorized("Admin email not found in claims.");

            // Validate required parameters
            if (string.IsNullOrEmpty(engineerId))
                return BadRequest("Engineer ID is required.");

            // Check if the admin engineer exists
            var engineer = _context.Users.FirstOrDefault(u => u.Email == adminEmail);
            if (engineer == null)
                return NotFound("Engineer not found.");

            var admin = _context.Admin_Details.FirstOrDefault(a => a.UserId == engineer.UserId);
            var adminUser = _context.Users.FirstOrDefault(u => u.UserId == admin.UserId && u.Email == adminEmail);

            if (admin == null || adminUser == null)
                return NotFound("Admin not found or unauthorized.");

            // Check if the engineer's token is revoked
            var revokedToken = _context.Revokes.FirstOrDefault(r => r.UserId == engineerId);
            if (revokedToken == null)
                return NotFound("This engineer's token is not revoked.");

            // Remove the revocation entry
            _context.Revokes.Remove(revokedToken);
            _context.SaveChanges();

            // Logging (for debugging purposes)
            Console.WriteLine($"Revocation removed: Engineer {engineerId} by Admin {admin.AdminId}");

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


    #region private methods

    // Helper method to get claim value
    private string? GetClaimValue(string claimType)
    {
        return User.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
    }

    #endregion
}

public class RevokeEngineerRequest
{
    public string engineerId;
    public string? reason = null;
}