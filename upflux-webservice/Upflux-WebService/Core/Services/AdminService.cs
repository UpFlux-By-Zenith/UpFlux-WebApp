using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Upflux_WebService.Core.Configuration;

namespace Upflux_WebService.Core.Services
{
    public class AdminService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserService _userService; // Assume this service handles fetching users

        public AdminService(JwtSettings jwtSettings, UserService userService)
        {
            _jwtSettings = jwtSettings;
            _userService = userService;
        }

        public string GenerateEngineerToken(string engineerEmail, List<int> machineIds)
        {
            var engineer = _userService.GetEngineerByEmail(engineerEmail); // Assume this fetches the engineer by email
            if (engineer == null)
                throw new ArgumentException("Engineer not found");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, engineer.Email),
                new Claim(ClaimTypes.Role, "Engineer"), // Role for the engineer
                new Claim("MachineIds", string.Join(",", machineIds)), // Assign machine access
                new Claim(JwtRegisteredClaimNames.Sub, engineer.Email), // Subject claim
                new Claim(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer), // Issuer claim
                new Claim(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience), // Audience claim
                new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes).ToUnixTimeSeconds().ToString()) // Expiration
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _jwtSettings.Issuer,
                _jwtSettings.Audience, // Audience
                claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.ExpirationMinutes), // Expiration
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
