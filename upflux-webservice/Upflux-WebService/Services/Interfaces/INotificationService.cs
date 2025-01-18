namespace Upflux_WebService.Services.Interfaces
{
    public interface INotificationService
    {
        Task CreateGroupAsync(string groupId);
        Task AddUriToGroupAsync(string groupId, string uri);
        Task RemoveUriFromGroupAsync(string groupId, string uri);
        Task SendMessageToUriAsync(string groupId, string uri, string message);
    }
}
