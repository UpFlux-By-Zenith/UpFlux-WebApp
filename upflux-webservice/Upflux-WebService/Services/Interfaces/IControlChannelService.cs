using UpFlux_WebService.Protos;

namespace Upflux_WebService.Services.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IControlChannelService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayId"></param>
		/// <param name="deviceUuid"></param>
		/// <param name="approved"></param>
		/// <param name="licenceContent"></param>
		/// <param name="expirationDate"></param>
		/// <returns></returns>
		Task SendLicenceResponseAsync(string gatewayId, string deviceUuid, bool approved, string licenceContent, DateTime expirationDate);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayId"></param>
		/// <param name="deviceUuids"></param>
		/// <returns></returns>
		Task SendLogRequestAsync(string gatewayId, string[] deviceUuids);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayId"></param>
		/// <param name="commandId"></param>
		/// <param name="cmdType"></param>
		/// <param name="parameters"></param>
		/// <param name="targetDevices"></param>
		/// <returns></returns>
		Task SendCommandToGatewayAsync(string gatewayId, string commandId, CommandType cmdType, string parameters, string userEmail, params string[] targetDevices );

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayId"></param>
		/// <param name="fileName"></param>
		/// <param name="packageData"></param>
		/// <param name="targetDevices"></param>
		/// <param name="appName"></param>
		/// <returns></returns>
		Task SendUpdatePackageAsync(string gatewayId, string fileName, byte[] packageData, string[] targetDevices, string appName, string version, string userEmail);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayId"></param>
		/// <returns></returns>
		Task SendVersionDataRequestAsync(string gatewayId);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="gatewayId"></param>
		/// <param name="scheduleId"></param>
		/// <param name="deviceUuids"></param>
		/// <param name="fileName"></param>
		/// <param name="packageData"></param>
		/// <param name="startTimeUtc"></param>
		/// <param name="userEmail"></param>
		/// <returns></returns>
		Task SendScheduledUpdateAsync(
			string gatewayId,
			string scheduleId,
			string[] deviceUuids,
			string fileName,
			byte[] packageData,
			DateTime startTimeUtc,
			string userEmail
		);
	}
}
