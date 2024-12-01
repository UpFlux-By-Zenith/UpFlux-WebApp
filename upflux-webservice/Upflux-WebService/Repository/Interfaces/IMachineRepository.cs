using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	public interface IMachineRepository : IRepository<Machine>
	{
		Task<IEnumerable<Machine>> GetUnregisteredMachines(int id);
	}
}
