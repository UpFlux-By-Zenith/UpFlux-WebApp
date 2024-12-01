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
	}
}
