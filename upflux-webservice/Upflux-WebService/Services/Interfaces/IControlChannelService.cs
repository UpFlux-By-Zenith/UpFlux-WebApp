using UpFlux_WebService.Protos;

namespace Upflux_WebService.Services.Interfaces
{
	public interface IControlChannelService
	{
		Task SendLicenceResponseAsync(string deviceUuid, bool approved, string licenceContent, DateTime expirationDate);

		Task SendLogRequestAsync(string gatewayId, string[] deviceUuids);

		Task SendCommandToGatewayAsync(string gatewayId,
													string commandId,
													CommandType cmdType,
													string parameters,
													params string[] targetDevices);
	}
}
