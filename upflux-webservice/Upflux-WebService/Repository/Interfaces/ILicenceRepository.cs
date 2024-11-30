using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	public interface ILicenceRepository : IRepository<Licence>
	{
		Task<IEnumerable<Licence>> GetValidLicensesAsync();
	}
}
