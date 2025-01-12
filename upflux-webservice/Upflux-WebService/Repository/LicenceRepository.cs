using Microsoft.EntityFrameworkCore;
using System.Reflection.PortableExecutable;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{

	public class LicenceRepository : Repository<Licence>, ILicenceRepository
	{
		private readonly ApplicationDbContext _context;

		public LicenceRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		/// <summary>
		/// Example use of the Table specific repository
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public async Task<Licence?> GetLicenceByMachineId( string machineId)
		{
			return await _context.Licences
				.FirstOrDefaultAsync(l => l.MachineId == machineId);
		}
	}
}
