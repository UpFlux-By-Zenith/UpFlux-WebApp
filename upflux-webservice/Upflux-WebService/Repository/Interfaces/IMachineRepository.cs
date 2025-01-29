using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	public interface IMachineRepository : IRepository<Machine>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		string[] GetAllMachineIds();
	}
}
