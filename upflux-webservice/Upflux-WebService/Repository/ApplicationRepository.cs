using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{
	public class ApplicationRepository : Repository<Application>, IApplicationRepository
	{
		private readonly ApplicationDbContext _context;

		public ApplicationRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<Application?> GetByMachineId(string machineId)
		{
			return await _context.Applications
				.FirstOrDefaultAsync(l => l.MachineId == machineId);
		}
	}
}
