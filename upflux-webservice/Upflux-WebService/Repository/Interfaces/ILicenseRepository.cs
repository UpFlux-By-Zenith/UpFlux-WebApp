using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Repository.Interfaces
{
	public interface ILicenseRepository : IRepository<License>
	{
		Task<IEnumerable<License>> GetValidLicensesAsync();
	}
}
