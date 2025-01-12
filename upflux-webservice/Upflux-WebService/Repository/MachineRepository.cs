﻿using Upflux_WebService.Core.Models;
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

		/// <summary>
		/// example use of the table specific repository
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public Task<IEnumerable<Machine>> GetUnregisteredMachines(int id)
		{
			throw new NotImplementedException();
		}
	}
}
