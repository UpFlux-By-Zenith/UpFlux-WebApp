using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{

	public class LicenseRepository : Repository<License>, ILicenseRepository
	{
		private readonly ApplicationDbContext _context;

		public LicenseRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}


		/// <summary>
		/// Example use of the Table specific repository
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public Task<IEnumerable<License>> GetValidLicensesAsync()
		{
			throw new NotImplementedException();
		}
	}
}
