using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository
{
	public class UserRepository : Repository<User>, IUserRepository
	{
		private readonly ApplicationDbContext _context;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="context"></param>
		public UserRepository(ApplicationDbContext context) : base(context)
		{
			_context = context;
		}

		public async Task<User?> GetUserByEmail(string email)
		{
			return await _context.Users
				.FirstOrDefaultAsync(l => l.Email == email);
		}
	}
}
