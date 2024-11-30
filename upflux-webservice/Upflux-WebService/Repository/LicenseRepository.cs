using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository;

public class LicenceRepository : Repository<Licence>, ILicenceRepository
{
	public LicenceRepository(ApplicationDbContext context) : base(context)
	{
	}

	public Task<IEnumerable<Licence>> GetValidLicensesAsync()
	{
		throw new NotImplementedException();
	}


}
