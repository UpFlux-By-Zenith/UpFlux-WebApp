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

		public DbSet<Application> Applications{ get; set; }

		public DbSet<GeneratedMachineId> Generated_Machine_Ids { get; set; }


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
            modelBuilder.Entity<ApplicationVersion>()
       .ToTable("Application_Versions"); // Map the entity to the correct table name

            modelBuilder.Entity<Application>()
        .HasMany(a => a.Versions) // Application has many versions (one-to-many)
        .WithOne(av => av.Application) // Each ApplicationVersion has one application
        .HasForeignKey(av => av.AppId); // The foreign key is on the ApplicationVersion side




            base.OnModelCreating(modelBuilder);
		}
	}
}
