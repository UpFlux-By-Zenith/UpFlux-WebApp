using LicenceCommunication;
using Upflux_WebService.Core.DTOs;
using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Services.Interfaces
{
	/// <summary>
	/// Interface for licence related services.
	/// </summary>
	public interface ILicenceManagementService
	{
		/// <summary>
		/// Create Licence, store its metadata in the database, and send it to the gateway.
		/// </summary>
		/// <param name="machineId">the machineId which the licence belong to</param>
		/// <returns></returns>
		Task CreateLicence(string machineId);

		/// <summary>
		/// Validates Licence by comparing gateways's licence with the metadata stored in the database
		/// uses KMS to validate digital signature
		/// </summary>
		/// <param name="licenceXml">is the licence file that is received from the gateway</param>
		/// <returns></returns>
		Task<LicenceValidationResponse> ValidateLicence(string licenceXml);

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		Task<IEnumerable<LicenceResponse>> GetAllLicences();

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		Task<LicenceResponse?> GetLicenceByMachineId(string machineId);
	}
}
