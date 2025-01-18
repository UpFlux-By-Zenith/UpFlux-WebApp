using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Core.Models.Enums;

namespace Upflux_WebService.Data
{
	public class ApplicationDbContext : DbContext
	{
		public DbSet<Licence> Licences { get; set; }
		public DbSet<Machine> Machines { get; set; }

		public DbSet<Admin> Admin_Details { get; set; }

		public DbSet<Users> Users { get; set; }

		public DbSet<Credentials> Credentials { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
            // Add this enum conversion for UserRole
            modelBuilder.Entity<Users>()
                .Property(u => u.Role)
                .HasConversion(
                    v => v.ToString(),       // Enum to string for the database
                    v => (UserRole)Enum.Parse(typeof(UserRole), v) // String to enum for C#
                );
            base.OnModelCreating(modelBuilder);
		}
	}
}
