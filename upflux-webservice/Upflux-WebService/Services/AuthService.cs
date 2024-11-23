using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
    public class AuthService : IAuthService
    {
        private readonly List<Admin> _admins;
        private readonly List<Engineer> _engineers;

        private readonly IConfiguration _configuration;

        public AuthService(IConfiguration configuration)
        {
            _configuration = configuration;

            // Sample data for demonstration
            _admins = new List<Admin>
            {
                new Admin(Guid.NewGuid(), "Admin User", "admin@upflux.com", "hashedpassword123")
            };

            _engineers = new List<Engineer>
            {
                new Engineer(Guid.NewGuid(), "Engineer User", "engineer@upflux.com", new List<string> { "M1", "M2" })
            };
        }

        public string AdminLogin(string email, string password)
        {
            var admin = _admins.FirstOrDefault(a => a.Email == email && a.HashedPassword == password);
            if (admin == null)
                throw new UnauthorizedAccessException("Invalid admin credentials");

            // Generate a token for the admin
            return GenerateToken(admin.Email, new List<string>(), "Admin");
        }

        public List<string> EngineerLogin(string email)
        {
            var engineer = _engineers.FirstOrDefault(e => e.Email == email);
            if (engineer == null)
                throw new UnauthorizedAccessException("Invalid engineer credentials");

            return engineer.MachineIds;
        }

        public string GenerateEngineerToken(string engineerEmail,List<string> machineIds, string? enginnerName = "Engineer")
        {
            // Ensure the engineer does not already exist
            var existingEngineer = _engineers.FirstOrDefault(e => e.Email == engineerEmail);
            if (existingEngineer == null)
            {
                // Add the new engineer to the list
                _engineers.Add(new Engineer(Guid.NewGuid(), enginnerName, engineerEmail, machineIds));
            }
            else
            {
                // Update the machine IDs if the engineer already exists
                existingEngineer.MachineIds = machineIds;
            }

            // Generate a token for the engineer
            return GenerateToken(engineerEmail, machineIds, "Engineer");
        }


        public Dictionary<string, string> ParseToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }

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
    }
}
