using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{
	public class MachineRepository : Repository<Machine>, IMachineRepository
	{
		private readonly ApplicationDbContext _context;

		public MachineRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public Machine? GetById(int id)
		{
			return  _context.Machines.FirstOrDefault(m => m.MachineId == id);
		}
	}
}
