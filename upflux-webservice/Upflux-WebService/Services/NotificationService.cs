using Microsoft.AspNetCore.SignalR;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly MockDataGenerator _dataGenerator;
        // Local mapping for group URIs (could be moved to a shared store like Redis for scaling)
        private static readonly Dictionary<string, List<string>> GroupUriMapping = new();
        private readonly Dictionary<string, AggregatedData> _mockDataStorage = new();

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
            _dataGenerator = new MockDataGenerator();
        }

        public async Task CreateGroupAsync(string groupId)
        {
            if (!GroupUriMapping.ContainsKey(groupId))
            {
                GroupUriMapping[groupId] = new List<string>();
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
                // Create a new mock object for this URI
                var mockData = _dataGenerator.GenerateMockData(uri);
                _mockDataStorage[uri] = mockData;

                // Start sending updates for this URI
                _ = Task.Run(async () =>
                {
                    while (_mockDataStorage.ContainsKey(uri))
                    {
                        // Update the mock data
                        _dataGenerator.UpdateMockData(mockData);

                        // Send the updated data to the group
                        await _hubContext.Clients.Group(groupId).SendAsync("ReceiveData",uri, mockData);

                        // Wait for 1 second
                        await Task.Delay(1000);
                    }
                });
            }
        }

        public async Task RemoveUriFromGroupAsync(string groupId, string uri)
        {
            if (GroupUriMapping.TryGetValue(groupId, out var uris))
            {
                // Remove from the mock storage
                _mockDataStorage.Remove(uri);
                uris.Remove(uri);
            }
        }

        public async Task SendMessageToUriAsync(string groupId, string uri, string message)
        {
            if (GroupUriMapping.TryGetValue(groupId, out var uris) && uris.Contains(uri))
            {
                await _hubContext.Clients.Group(groupId).SendAsync("ReceiveMessage", uri, message);
            }
        }
    }
}
