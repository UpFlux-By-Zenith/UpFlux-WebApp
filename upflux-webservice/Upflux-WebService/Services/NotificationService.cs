using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly MockDataGenerator _dataGenerator;

        // Local mapping for group machine IDs and URIs
        private static readonly Dictionary<string, List<string>> GroupUriMapping = new();
        private readonly Dictionary<string, AggregatedData> _mockDataStorage = new();

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
            _dataGenerator = new MockDataGenerator();
        }

        public async Task CreateGroupWithTokenAsync(string token)
        {
            var claims = ParseToken(token);

            if (!claims.TryGetValue(ClaimTypes.Email, out var email) || !claims.TryGetValue("MachineIds", out var machineIdsClaim))
            {
                throw new ArgumentException("Invalid token: Missing required claims.");
            }

            string groupId = token;
            var machineIds = machineIdsClaim.Split(',').ToList();

            // Create the group if it doesn't exist
            if (!GroupUriMapping.ContainsKey(groupId))
            {
                GroupUriMapping[groupId] = new List<string>();

                foreach (var machineId in machineIds)
                {
                    await AddUriToGroupAsync(groupId, machineId);
                }
            }
        }

        public async Task AddUriToGroupAsync(string groupId, string uri)
        {
            if (!GroupUriMapping.ContainsKey(groupId))
            {
                throw new ArgumentException($"Group '{groupId}' does not exist.");
            }

            if (!GroupUriMapping[groupId].Contains(uri))
            {
                GroupUriMapping[groupId].Add(uri);
                Console.WriteLine($"Added URI: {uri}");
            }
        }

        public async Task RemoveUriFromGroupAsync(string groupId, string uri)
        {
            if (GroupUriMapping.TryGetValue(groupId, out var uris))
            {
                _mockDataStorage.Remove(uri);
                uris.Remove(uri);
            }
        }
        public async Task SendMessageToUriAsync(string uri, string message)
        {
            // Loop through all groups
            foreach (var groupId in GroupUriMapping.Keys)
            {
                // Check if the group contains the uri
                if (GroupUriMapping[groupId].Contains(uri))
                {
                    // Send message to the group if the uri is found
                    await _hubContext.Clients.Group(groupId).SendAsync("ReceiveMessage", uri, message);
                }
            }
        }


        private Dictionary<string, string> ParseToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
        }
    }
}
