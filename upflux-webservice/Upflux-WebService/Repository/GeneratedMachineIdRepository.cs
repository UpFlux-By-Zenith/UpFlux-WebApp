using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{
	public class GeneratedMachineIdRepository : Repository<GeneratedMachineId>, IGeneratedMachineIdRepository
	{
		private readonly ApplicationDbContext _context;

		public GeneratedMachineIdRepository (ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<GeneratedMachineId?> GetByMachineId(string machineId)
		{
			return await _context.Generated_Machine_Ids
				.FirstOrDefaultAsync(l => l.MachineId == machineId);
		}
	}
}
