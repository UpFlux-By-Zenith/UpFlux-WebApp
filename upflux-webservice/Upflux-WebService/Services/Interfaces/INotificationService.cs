namespace Upflux_WebService.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateGroupWithTokenAsync(string token);
        Task AddUriToGroupAsync(string groupId, string uri);
        Task RemoveUriFromGroupAsync(string groupId, string uri);
        Task SendMessageToUriAsync(string groupId, string uri, string message);
    }
}
