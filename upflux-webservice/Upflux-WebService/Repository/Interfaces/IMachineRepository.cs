using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	public interface IMachineRepository : IRepository<Machine>
	{
		Machine? GetById(int id);
	}
}
