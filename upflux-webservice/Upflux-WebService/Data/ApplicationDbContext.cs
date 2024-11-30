using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using Upflux_WebService.Core.Models;

namespace Upflux_WebService.Data
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<Licence> Licenses { get; set; }
		public DbSet<Machine> Machines { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Machine>(entity =>
			{
				// store enum as string
				entity.Property(e => e.MachineStatus)
					  .HasConversion<string>(); 

				entity.Property(e => e.ActivityStatus)
					  .HasConversion<string>(); 
			});
		}
	}
}
