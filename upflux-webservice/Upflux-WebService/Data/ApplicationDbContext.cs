using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Data
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<Licence> Licences { get; set; }

		public DbSet<Machine> Machines { get; set; }

		public DbSet<GeneratedMachineId> Generated_Machine_Ids { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
		}
	}
}
