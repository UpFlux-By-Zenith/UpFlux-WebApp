using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Upflux_WebService.Controllers;
using Upflux_WebService.Core.Objects;

namespace Upflux_WebService.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly List<User> _users; // Temporary data source (can be replaced by a database)

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;
            _users = new List<User>(); // Simulating user data; replace with real database calls.
        }

        // Admin login method
        public async Task<string> AdminLoginAsync(AdminLoginRequest loginRequest)
        {
            var user = _users.FirstOrDefault(u => u.Email == loginRequest.Email && u.Role == "Admin");
            if (user == null || user.HashedPassword != loginRequest.Password)
                return null;

            return GenerateJwtToken(user);
        }


        // Admin password reset method
        public async Task<bool> AdminResetPasswordAsync(AdminResetPasswordRequest resetRequest)
        {
            var user = _users.FirstOrDefault(u => u.Email == resetRequest.Email && u.Role == "Admin");
            if (user == null)
                return false;

            // Update the password (make sure it's hashed in production)
            user.HashedPassword = resetRequest.NewPassword;
            return true;
        }


        // Engineer login method
        public async Task<string> EngineerLoginAsync(AdminLoginRequest loginRequest)
        {
            var user = _users.FirstOrDefault(u => u.Email == loginRequest.Email && u.Role == "Engineer");
            if (user == null || user.HashedPassword != loginRequest.Password)
                return null;

            return GenerateJwtToken(user);
        }

        // JWT generation
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
