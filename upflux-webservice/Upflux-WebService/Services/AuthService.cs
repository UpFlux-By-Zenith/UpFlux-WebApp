using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Services.Enums;
using Upflux_WebService.Services.Interfaces;


namespace Upflux_WebService.Services
{
    /// <summary>
    /// Service that deals with Authenication related methods
    /// </summary>
    public class AuthService : IAuthService
    {
        #region private members
        private readonly IConfiguration _configuration;
        private readonly IEntityQueryService _entityQueryService;
        #endregion

        #region public methods
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public AuthService(IConfiguration configuration,IEntityQueryService entityQueryService)
        {
            _configuration = configuration;
            _entityQueryService = entityQueryService;

        }

        /// <summary>
        /// Authenticates an admin user and generates a token.
        /// </summary>
        /// <param name="email">The email address of the admin.</param>
        /// <param name="password">The password of the admin (should be hashed).</param>
        /// <returns>A JWT token for the admin if authentication is successful.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the provided credentials are invalid.</exception>
        /// <remarks>
        /// This method verifies the admin's credentials (email and password). If the credentials are valid, a JWT token is generated and returned.
        /// The token will allow the admin to access authorized resources within the application.
        /// </remarks>
        public string AdminLogin(string email, string password)
        {
            var loginResult = _entityQueryService.CheckAdminLogin(email, password).Result; // Ensures async call is awaited properly

            // Return error message based on login result
            switch (loginResult)
            {
                case DbErrorEnum.Success:
                    // Generate a token for the admin
                    return GenerateToken(email, new List<string>(), "Admin");

                case DbErrorEnum.UserNotFound:
                    throw new Exception("User not found.");

                case DbErrorEnum.AdminNotFound:
                    throw new Exception("Admin not found.");

                case DbErrorEnum.InvalidPassword:
                    throw new Exception("Invalid password.");

                default:
                    throw new Exception("An error occurred during login.");
            }
        }


        /// <summary>
        /// Changes the password for an admin user.
        /// </summary>
        /// <param name="email">The email address of the admin.</param>
        /// <param name="oldPassword">The current (old) password of the admin (hashed).</param>
        /// <param name="newPassword">The new password to be set for the admin (hashed).</param>
        /// <returns>True if the password was successfully changed, otherwise false.</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the admin is not found.</exception>
        /// <remarks>
        /// This method checks if the admin exists and if the old password matches the stored password.
        /// If both conditions are met, the password is updated with the new password (proper hashing should be applied to the new password). 
        /// If the old password is incorrect or the admin is not found, the operation will fail.
        /// </remarks>

        public bool ChangeAdminPassword(string email, string oldPassword, string newPassword)
        {
            return false;
            //var admin = _admins.FirstOrDefault(a => a.Email == email);
            //if (admin == null)
            //    throw new UnauthorizedAccessException("Admin not found.");

            //if (admin.HashedPassword != oldPassword)  // Use hashed password comparison
            //    return false;

            //// Update the password (ensure new password is hashed)
            //admin.HashedPassword = newPassword;  // Replace this with proper password hashing
            //return true;
        }

        /// <summary>
        /// Generates a token for an engineer, either by creating a new engineer or updating an existing one with new machine access.
        /// </summary>
        /// <param name="engineerEmail">The email address of the engineer.</param>
        /// <param name="machineIds">A list of machine IDs the engineer has access to.</param>
        /// <param name="engineerName">The name of the engineer (optional, defaults to "Engineer").</param>
        /// <returns>A JWT token for the engineer granting access to the specified machines.</returns>
        /// <remarks>
        /// This method checks if an engineer with the specified email already exists. If the engineer does not exist, a new entry is added to the list. 
        /// If the engineer already exists, their machine IDs are updated. After ensuring the engineer’s details are up-to-date, a token is generated and returned.
        /// The generated token allows the engineer to access the specified machines.
        /// </remarks>
        public string GenerateEngineerToken(string engineerEmail,List<string> machineIds, string engineerName = "Engineer")
        {
            string userId = _entityQueryService.CreateUser(engineerEmail,engineerName,Core.Models.Enums.UserRole.Engineer).Result.Item1;
            
            if (userId != null) { 
            string token = GenerateToken(engineerEmail, machineIds, "Engineer");
            
                // Generate a token for the engineer
            _entityQueryService.AddCredentials(userId, machineIds, DateTime.UtcNow , DateTime.UtcNow.AddMinutes(30));
            return token;

            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Parses a JWT token and extracts its claims into a dictionary.
        /// </summary>
        /// <param name="token">The JWT token to be parsed.</param>
        /// <returns>A dictionary containing the claims, where the key is the claim type and the value is the claim value.</returns>
        /// <remarks>
        /// This method takes a JWT token, reads and parses it, and returns a dictionary with the claim types as keys and the corresponding values.
        /// This can be used to retrieve specific information from the token, such as user email, roles, or any custom claims stored in the token.
        /// </remarks>

        public Dictionary<string, string> ParseToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

        #endregion

        #region private methods
        private string GenerateToken(string email, List<string> machineIds, string role)
        {
            // Retrieve settings from appsettings.json
            var issuer = _configuration["JwtSettings:Issuer"];
            var audience = _configuration["JwtSettings:Audience"];
            var secretKey = _configuration["JwtSettings:SecretKey"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT settings are not properly configured.");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim("MachineIds", string.Join(",", machineIds)),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion
    }
}
