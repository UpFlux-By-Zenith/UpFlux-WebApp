using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	public interface IApplicationVersionRepository : IRepository<ApplicationVersion>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		Task<ApplicationVersion?> GetByMachineId(string machineId);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		Task<List<ApplicationVersion>> GetVersionsByMachineId(string machineId);

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <param name="versionName"></param>
		/// <returns></returns>
		Task<ApplicationVersion?> GetByMachineIdAndVersion(string machineId, string versionName);
	}
}
