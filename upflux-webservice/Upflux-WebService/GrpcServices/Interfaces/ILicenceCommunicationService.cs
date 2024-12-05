using GrpcServer;

namespace Upflux_WebService.GrpcServices.Interfaces
{
	public interface ILicenceCommunicationService
	{
		Task PushLicenceUpdateAsync(LicenceFileUpdate licenceFileUpdate);
	}
}
