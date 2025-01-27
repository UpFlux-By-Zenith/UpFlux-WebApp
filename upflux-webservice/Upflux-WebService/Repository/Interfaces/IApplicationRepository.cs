using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	/// <summary>
	/// 
	/// </summary>
	public interface IApplicationRepository : IRepository<Application>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		Task<Application?> GetByMachineId(string machineId);
	}
}
