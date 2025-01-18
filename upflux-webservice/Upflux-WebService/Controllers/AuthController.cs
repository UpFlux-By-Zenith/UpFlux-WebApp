using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Upflux_WebService.Core.DTOs;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Services.Enums;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Controllers
{
    /// <summary>
    /// Authentication related Controllers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        #region private members

        private readonly IAuthService _authService;
        private readonly IEntityQueryService _entityQueryService;

        #endregion

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="authService"></param>
        public AuthController(IAuthService authService, IEntityQueryService entityQuery)
        {
            _authService = authService;
            _entityQueryService = entityQuery;
        }
        #endregion

        #region Admin APIs

        /// <summary>
        /// Admin login
        /// </summary>
        /// <remarks>
        /// This endpoint allows an admin to log in by providing email and password. Upon successful authentication,
        /// a JWT token is generated and returned. The token is required for further API requests that need admin privileges.
        /// </remarks>
        /// <param name="request">The login request containing email and password</param>
        /// <returns>Returns a token if login is successful, or error messages if validation fails.</returns>
        /// <response code="200">Successful login and token generation</response>
        /// <response code="400">Bad Request if email or password are empty or missing</response>
        /// <response code="401">Unauthorized if the provided credentials are incorrect</response>
        /// <response code="500">Internal Server Error in case of unexpected errors</response>
        [HttpPost("admin/login")]
        public IActionResult AdminLogin([FromBody] AdminCreateLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { Error = "Email and Password are required." });

            try
            {
                var token = _authService.AdminLogin(request.Email, request.Password);
                return Ok(new { Token = token });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Error = ex.Message });
            }
        }

        [HttpPost("admin/create")]
        public IActionResult AdminCreate([FromBody] AdminCreateRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
                return BadRequest(new { Error = "Email and Password are required." });
            try
            {

               DbErrorEnum response = _entityQueryService.CreateAdminAccount(request.Name,request.Email, request.Password).Result;
                if (response != DbErrorEnum.Success)
                {
                    return BadRequest(new { Response = response });
                }
                else
                {
                    return Ok();
                }

            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        /// <summary>
        /// Admin password change
        /// </summary>
        [Authorize(Roles = "Admin")]
        [HttpPost("admin/change-password")]
        public IActionResult ChangeAdminPassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.OldPassword) || string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.ConfirmPassword))
                return BadRequest(new { Error = "All fields are required." });

            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest(new { Error = "New password and confirmation password do not match." });

            try
            {
                // Get admin email from the claims to identify the admin
                var adminEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                if (string.IsNullOrEmpty(adminEmail))
                    return Unauthorized(new { Error = "Admin not authenticated." });

                // Change the password
                var result = _authService.ChangeAdminPassword(adminEmail, request.OldPassword, request.NewPassword);
                if (!result)
                    return Unauthorized(new { Error = "Old password is incorrect." });

                return Ok(new { Message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }


        /// <summary>
        /// Admin creates a token for an engineer
        /// </summary>
        /// <remarks>
        /// This endpoint allows an admin to create a new token for an engineer, granting them access to specified machines and Application.
        /// The admin must be authenticated with the appropriate role to perform this operation.
        /// </remarks>
        /// <param name="request">The request containing the engineer's email and list of machine IDs</param>
        /// <returns>Returns the engineer's token if creation is successful, or error messages if validation fails.</returns>
        /// <response code="200">Token successfully created for the engineer</response>
        /// <response code="400">Bad Request if the engineer's email or machine IDs are missing or invalid</response>
        /// <response code="401">Unauthorized if the admin token is invalid or the admin does not have the correct role</response>
        /// <response code="500">Internal Server Error in case of unexpected errors</response>
        [HttpPost("admin/create-engineer-token")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public IActionResult CreateEngineerToken([FromBody] EngineerTokenRequest request)
        {
            if (string.IsNullOrEmpty(request.EngineerEmail) || request.MachineIds == null || !request.MachineIds.Any())
                return BadRequest(new { Error = "EngineerEmail and MachineIds are required." });

            try
            {
                var adminEmail = GetClaimValue(ClaimTypes.Email);
                if (string.IsNullOrEmpty(adminEmail))
                    return Unauthorized(new { Error = "Invalid admin token." });

                var engineerToken = _authService.GenerateEngineerToken(request.EngineerEmail, request.MachineIds);
                return Ok(new { EngineerToken = engineerToken });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }

        }
        #endregion

        #region Engineer APIs    
        /// <summary>
        /// Engineer login
        /// </summary>
        /// <remarks>
        /// This endpoint allows an engineer to log in by providing their email and an engineer token. 
        /// The engineer token is parsed and validated, and if valid, a new authorization token is generated 
        /// for the engineer with access to the specified machines.
        /// </remarks>
        /// <param name="request">The login request containing the engineer's email and token</param>
        /// <returns>Returns a new authorization token if login and validation are successful, or error messages if validation fails.</returns>
        /// <response code="200">Successful login and token generation for the engineer</response>
        /// <response code="400">Bad Request if email or token are missing or invalid</response>
        /// <response code="401">Unauthorized if the token is invalid, email mismatch, or missing machine IDs in the token</response>
        /// <response code="500">Internal Server Error in case of unexpected errors</response>

        [HttpPost("engineer/login")]
        public IActionResult EngineerLogin([FromBody] EngineerLoginRequest request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.EngineerToken))
                return BadRequest(new { Error = "Email and token are required." });

            try
            {
                // Parse and validate the engineer token
                var tokenData = _authService.ParseToken(request.EngineerToken);

                // Ensure the token's email matches the provided email
                if (!tokenData.TryGetValue(ClaimTypes.Email, out var tokenEmail) || tokenEmail != request.Email)
                    return Unauthorized(new { Error = "Invalid token for the provided email." });

                // Retrieve machine IDs from the token
                if (!tokenData.TryGetValue("MachineIds", out var machineIds))
                    return Unauthorized(new { Error = "Invalid token: no machine IDs found." });

                // Generate a new authorization token for the engineer
                var authToken = _authService.GenerateEngineerToken(request.Email, machineIds.Split(',').ToList());

                return Ok(new { Token = authToken });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Error = "Invalid token: " + ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        #endregion

        #region Example APIs
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

        /// <summary>
        /// Parse and verify a token
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("parse-token")]
        public IActionResult ParseToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest(new { Error = "Token is required." });

            try
            {
                var tokenData = _authService.ParseToken(token);
                return Ok(tokenData);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        /// <summary>
        /// Shared endpoint for Admin and Engineer roles
        /// </summary>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AdminOrEngineer")]
        [HttpGet("shared-endpoint")]
        public IActionResult SharedEndpoint()
        {
            var email = GetClaimValue(ClaimTypes.Email);
            var role = GetClaimValue(ClaimTypes.Role);

            return Ok(new { Email = email, Role = role });
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
