using System.Security.Claims;

namespace Upflux_WebService.Core.Interfaces
{
    public interface ITokenService
    {
        /// <summary>
        /// Validates the JWT token and returns the principal (user claims).
        /// </summary>
        /// <param name="token">The JWT token to validate.</param>
        /// <returns>ClaimsPrincipal representing the user's claims, or null if invalid.</returns>
        ClaimsPrincipal ValidateToken(string token);

        /// <summary>
        /// Generates a JWT token based on user details (email, role, etc.).
        /// </summary>
        /// <param name="userEmail">The email of the user.</param>
        /// <param name="role">The role of the user (e.g., "Admin", "Engineer").</param>
        /// <param name="claims">Additional claims to include in the token.</param>
        /// <returns>A string containing the generated JWT token.</returns>
        string GenerateToken(string userEmail, string role, List<Claim> claims);
    }
}
