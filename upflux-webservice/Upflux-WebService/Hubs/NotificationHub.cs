using Microsoft.AspNetCore.SignalR;

public class NotificationHub : Hub
{
    // Mapping for group URIs: GroupId -> List of URIs
    private static readonly Dictionary<string, List<string>> GroupUriMapping = new();

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        // Optional: Handle cleanup if necessary, e.g., remove the connection from all groups.
        return base.OnDisconnectedAsync(exception);
    }

    public async Task CreateGroup(string groupId)
    {
        if (!GroupUriMapping.ContainsKey(groupId))
        {
            GroupUriMapping[groupId] = new List<string>();
            Console.WriteLine($"Group '{groupId}' created.");
        }
        else
        {
            Console.WriteLine($"Group '{groupId}' already exists.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task SubscribeUri(string groupId, string uri)
    {
        if (!GroupUriMapping.TryGetValue(groupId, out var uris))
        {
            throw new HubException($"Group '{groupId}' does not exist.");
        }

        if (!uris.Contains(uri))
        {
            uris.Add(uri);
            Console.WriteLine($"URI '{uri}' added to group '{groupId}'.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    public async Task UnsubscribeUri(string groupId, string uri)
    {
        if (GroupUriMapping.TryGetValue(groupId, out var uris) && uris.Contains(uri))
        {
            uris.Remove(uri);
            Console.WriteLine($"URI '{uri}' removed from group '{groupId}'.");
        }

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }
}
