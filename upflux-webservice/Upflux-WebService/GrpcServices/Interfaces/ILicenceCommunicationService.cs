///
/// ------------------- TO BE DELETED --------------------
///

using LicenceCommunication;

namespace Upflux_WebService.GrpcServices.Interfaces
{
	/// <summary>
	/// interface for Licence related gRPC service
	/// </summary>
	public interface ILicenceCommunicationService
	{
		/// <summary>
		/// Send licence update to the clients
		/// </summary>
		/// <param name="licenceFileUpdate">the licence metadata which will be used as load</param>
		Task PushLicenceUpdateAsync(LicenceUpdateEvent licenceFileUpdate);
	}
}
