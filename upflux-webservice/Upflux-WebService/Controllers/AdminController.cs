using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{
    /// <summary>
    /// API for managing Admins. Accessible only by Admins.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext dbContext)
        {
            _context = dbContext;
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

        ///// <summary>
        ///// Adds a new admin.
        ///// </summary>
        ///// <param name="admin">The admin details.</param>
        ///// <returns>A confirmation message.</returns>
        ///// <response code="200">Admin added successfully.</response>
        ///// <response code="400">Invalid request data.</response>
        ///// <response code="401">Unauthorized - Access restricted to Admin role.</response>
        //[HttpPost("add")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //public async Task<IActionResult> AddAdmin([FromBody] Admin admin)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(new { Message = "Invalid request data.", Errors = ModelState.Values });
        //    }

        //    await _entityQueryService.GetAllAsync();
        //    await _entityQueryService.SaveChangesAsync();
        //    return Ok(new { Message = "Admin added successfully." });
        //}
    }
}
