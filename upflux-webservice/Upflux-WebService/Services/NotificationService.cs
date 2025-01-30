using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    // Local mapping for group machine IDs and URIs
    private static readonly Dictionary<string, List<string>> GroupUriMapping = new();

    public NotificationService(IHubContext<NotificationHub> hubContext, ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task CreateGroupWithTokenAsync(string token)
    {
        _logger.LogInformation("Creating group with token.");

        try
        {
            var claims = ParseToken(token);

            if (!claims.TryGetValue(ClaimTypes.Email, out var email) ||
                !claims.TryGetValue("MachineIds", out var machineIdsClaim))
                throw new ArgumentException("Invalid token: Missing required claims.");

            var groupId = token;
            var machineIds = machineIdsClaim.Split(',').ToList();

            // Create the group if it doesn't exist
            if (!GroupUriMapping.ContainsKey(groupId))
            {
                GroupUriMapping[groupId] = new List<string>();

                foreach (var machineId in machineIds)
                {
                    await AddUriToGroupAsync(groupId, machineId);
                    await AddUriToGroupAsync(groupId, $"{machineId}/alert");
                }

                _logger.LogInformation("Group created successfully with ID: {GroupId}", groupId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating group with token.");
            throw;
        }
    }

    public async Task AddUriToGroupAsync(string groupId, string uri)
    {
        _logger.LogInformation("Adding URI '{Uri}' to group '{GroupId}'", uri, groupId);

        try
        {
            if (!GroupUriMapping.ContainsKey(groupId))
                throw new ArgumentException($"Group '{groupId}' does not exist.");

            if (!GroupUriMapping[groupId].Contains(uri))
            {
                GroupUriMapping[groupId].Add(uri);
                _logger.LogInformation("Successfully added URI '{Uri}' to group '{GroupId}'", uri, groupId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding URI '{Uri}' to group '{GroupId}'", uri, groupId);
            throw;
        }
    }

    public async Task RemoveUriFromGroupAsync(string groupId, string uri)
    {
        _logger.LogInformation("Removing URI '{Uri}' from group '{GroupId}'", uri, groupId);

        if (GroupUriMapping.TryGetValue(groupId, out var uris))
        {
            uris.Remove(uri);
            _logger.LogInformation("Successfully removed URI '{Uri}' from group '{GroupId}'", uri, groupId);
        }
        else
        {
            _logger.LogWarning("Attempt to remove URI from non-existent group '{GroupId}'", groupId);
        }
    }

    public async Task SendMessageToUriAsync(string uri, string message)
    {
        _logger.LogInformation("Sending message to URI '{Uri}'", uri);

        try
        {
            // Loop through all groups
            foreach (var groupId in GroupUriMapping.Keys)
                // Check if the group contains the uri
                if (GroupUriMapping[groupId].Contains(uri))
                {
                    // Send message to the group if the uri is found
                    await _hubContext.Clients.Group(groupId).SendAsync("ReceiveMessage", uri, message);
                    _logger.LogInformation("Message sent to URI '{Uri}' in group '{GroupId}'", uri, groupId);
                }

            _logger.LogWarning("[SignalR] Message not sent. URI '{Uri}' not found in any group.", uri);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[SignalR] Error occurred while sending message to URI '{Uri}'", uri);
            throw;
        }
    }

    private Dictionary<string, string> ParseToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        return jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value);
    }
}