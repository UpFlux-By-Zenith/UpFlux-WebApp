using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{
	public class ApplicationVersionRepository : Repository<ApplicationVersion>, IApplicationVersionRepository
	{
		private readonly ApplicationDbContext _context;

		public ApplicationVersionRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		public async Task<ApplicationVersion?> GetByMachineId(string machineId)
		{
			return await _context.Application_Versions
				.FirstOrDefaultAsync(l => l.MachineId == machineId);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <param name="versionName"></param>
		/// <returns></returns>
		public async Task<ApplicationVersion?> GetByMachineIdAndVersion(string machineId, string versionName)
		{
			return await _context.Application_Versions
				.FirstOrDefaultAsync(l => l.MachineId == machineId && l.VersionName == versionName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="machineId"></param>
		/// <returns></returns>
		public async Task<List<ApplicationVersion>> GetVersionsByMachineId(string machineId)
		{
			var result = await _context.Application_Versions
				.Where(l => l.MachineId == machineId)
				.ToListAsync();

			return result;
		}

	}
}
