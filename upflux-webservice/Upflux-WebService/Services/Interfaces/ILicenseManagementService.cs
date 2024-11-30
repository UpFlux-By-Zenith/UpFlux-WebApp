namespace Upflux_WebService.Services.Interfaces
{
	/// <summary>
	/// Interface for license related services.
	/// </summary>
	public interface ILicenseManagementService
	{
		/// <summary>
		/// Create License, store its metadata in the database, and send it to the gateway.
		/// </summary>
		/// <param name="machineId">the machineId which the license belong to</param>
		/// <returns></returns>
		Task CreateLicense(int machineId);
	}
}
