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
	}
}
