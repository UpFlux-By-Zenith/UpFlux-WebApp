using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{
	public class MachineStatusRepository : Repository<MachineStatus>, IMachineStatusRepository
	{
		private readonly ApplicationDbContext _context;

		public MachineStatusRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}
	}
}
