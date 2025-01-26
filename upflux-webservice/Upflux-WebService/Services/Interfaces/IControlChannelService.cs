using UpFlux_WebService.Protos;

namespace Upflux_WebService.Services.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IControlChannelService
	{

		Task SendLicenceResponseAsync(string gatewayId, string deviceUuid, bool approved, string licenceContent, DateTime expirationDate);

		Task SendLogRequestAsync(string gatewayId, string[] deviceUuids);

		Task SendCommandToGatewayAsync(string gatewayId, string commandId, CommandType cmdType, string parameters, params string[] targetDevices);

		Task SendUpdatePackageAsync(string gatewayId, string fileName, byte[] packageData, string[] targetDevices);
	}
}
