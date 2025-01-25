namespace Upflux_WebService.Services.Interfaces
{
	public interface IControlChannelService
	{
		Task SendLicenceResponseAsync(
			string deviceUuid,
			bool approved,
			string licenceContent,
			DateTime expirationDate);
	}
}
