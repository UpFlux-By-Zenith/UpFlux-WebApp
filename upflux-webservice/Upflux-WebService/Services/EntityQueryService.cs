using System.Security.Cryptography;
using System.Text;
using Upflux_WebService.Core.Models.Enums;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Services.Enums;
using System.Data;

namespace Upflux_WebService.Services
{
    public class EntityQueryService : IEntityQueryService
    {
        private readonly ApplicationDbContext _context;

        private enum DbGenerateId
        {
            ADMIN,
            ENGINEER,
            MACHINE
        }

        #region public methods  
        public EntityQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DbErrorEnum> CheckAdminLogin(string email, string password)
        {
            // Find the user base entry by email (since email is in UserBases table)
            var userBase = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (userBase == null)
                return DbErrorEnum.UserNotFound;

            // Find the admin entity based on the userId
            var admin = await _context.Admin_Details
                .FirstOrDefaultAsync(a => a.UserId == userBase.UserId);

            if (admin == null)
                return DbErrorEnum.AdminNotFound;

            // Verify the password
            if (!VerifyPassword(password, admin.HashedPassword))
                return DbErrorEnum.InvalidPassword;

            // Update LastLogin for the userBase
            userBase.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return DbErrorEnum.Success;
        }


        public async Task<DbErrorEnum> CreateAdminAccount(string name,string email, string password)
        {
            // Check if the email already exists in the UserBase table
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                return DbErrorEnum.UserNotFound; // Return UserNotFound if email is already taken

            string userId = CreateUser(email, name).Result.Item1;

            // Hash the password before storing
            string hashedPassword = HashPassword(password);

            // Map the request data to the Admin entity
            var admin = new Admin
            {
                AdminId = GenerateUserId(DbGenerateId.ADMIN),
                UserId = userId,
                HashedPassword = hashedPassword
            };

            try
            {
                // Add and save changes
                await _context.Database.ExecuteSqlRawAsync($"SET @current_user_id = '1'");
                await _context.Admin_Details.AddAsync(admin); // Use the Admins DbSet
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // If an error occurs during saving to the database, return a general error
                // You could log the exception for further debugging.
                return DbErrorEnum.GeneralError;
            }

            // If everything is successful, return success
            return DbErrorEnum.Success;
        }

        public async Task<(string,DbErrorEnum)> CreateUser(string email, string name, UserRole role = UserRole.Engineer)
        {
            // Check if the email already exists in the UserBases table
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);

            if (existingUser != null)
                return (null,DbErrorEnum.UserNotFound); // Return error if email is already taken

            // Generate a new userId
            string userId = GenerateUserId(DbGenerateId.ENGINEER);

            // Create a new UserBase object
            Users newUser = new Users
            {
                UserId = userId,
                Email = email,
                Name = name,
                Role = role,
                LastLogin = null // Initially, no login
            };

            // Add and save changes to the context
            //await _context.Database.ExecuteSqlRawAsync($"SET @current_user_id = '1'");
            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Return the generated userId on success
            return (userId,DbErrorEnum.Success);
        }

        public async Task<DbErrorEnum> CreateEngineerCredentials(string email, string name, List<string> machineIds, DateTime accessGranted, DateTime expiry)
        {
            string userId = CreateUser(email, name).Result.Item1;
            Console.WriteLine($"User with userId '{userId}' found.");

            // Validate that all machine IDs in the input list exist in the database
            var existingMachineIds = await _context.Machines
                .Where(m => machineIds.Contains(m.MachineId))
                .Select(m => m.MachineId)
                .ToListAsync();

            var invalidMachineIds = machineIds.Except(existingMachineIds).ToList();

            if (invalidMachineIds.Any())
            {
                Console.WriteLine("Invalid machineIds: " + string.Join(", ", invalidMachineIds));
                return DbErrorEnum.MachineNotFound; // Return error if any machineIds are invalid
            }

            // Check for duplicate credentials in a single query
            var existingCredentials = await _context.Credentials
                .Where(c => c.UserId == userId && machineIds.Contains(c.MachineId))
                .Select(c => c.MachineId)
                .ToListAsync();

            if (existingCredentials.Any())
            {
                Console.WriteLine("Duplicate credentials for machineIds: " + string.Join(", ", existingCredentials));
                return DbErrorEnum.GeneralError; // Return error if duplicates are found
            }

            // Prepare credentials to add
            var credentialsList = machineIds.Select(machineId => new Credentials
            {
                UserId = userId,
                MachineId = machineId,
                AccessGrantedAt = accessGranted,
                ExpiresAt = expiry
            }).ToList();
            await _context.Database.ExecuteSqlRawAsync($"SET @current_user_id = 'e1'");

            // Add credentials to the database
            await _context.Credentials.AddRangeAsync(credentialsList);
            await _context.SaveChangesAsync();
            Console.WriteLine("Credentials added successfully.");
            return DbErrorEnum.Success;
        }


        public async Task<DbErrorEnum> AddCredentials(string userId, List<string> machineIds, DateTime accessGranted, DateTime expiry)
        {
            try
            {
                // Check if the user exists in the Users table
                var userExists = await _context.Users.AnyAsync(u => u.UserId == userId);
                if (!userExists)
                {
                    Console.WriteLine($"User with userId '{userId}' not found.");
                    return DbErrorEnum.UserNotFound; // Return error if userId is not found
                }
                Console.WriteLine($"User with userId '{userId}' found.");

                // Check if all machineIds exist in the Machines table
                var existingMachineIds = await _context.Machines
                    .Where(m => machineIds.Contains(m.MachineId))
                    .Select(m => m.MachineId)
                    .ToListAsync();

                Console.WriteLine($"exisitng MachinesIds {existingMachineIds}");

                // Find missing machineIds
                var missingMachineIds = machineIds.Except(existingMachineIds).ToList();
                if (missingMachineIds.Any())
                {
                    Console.WriteLine("Missing machineIds: " + string.Join(", ", missingMachineIds));
                    return DbErrorEnum.MachineNotFound; // Return error if any machineIds are missing
                }

                // Prepare credentials to add
                var credentialsList = machineIds.Select(machineId => new Credentials
                {
                    UserId = userId,
                    MachineId = machineId,
                    AccessGrantedAt = accessGranted,
                    ExpiresAt = expiry
                }).ToList();

                // Check for duplicates before adding
                foreach (var credential in credentialsList)
                {
                    bool exists = await _context.Credentials.AnyAsync(c =>
                        c.UserId == credential.UserId &&
                        c.MachineId == credential.MachineId);

                    if (exists)
                    {
                        Console.WriteLine($"Credential already exists for userId '{credential.UserId}' and machineId '{credential.MachineId}'.");
                        return DbErrorEnum.GeneralError; // Return error if duplicate credential exists
                    }
                }

                // Add credentials to the database
                await _context.Credentials.AddRangeAsync(credentialsList);
                await _context.SaveChangesAsync();
                Console.WriteLine("Credentials added successfully.");
                return DbErrorEnum.Success;
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error in AddCredentials: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return DbErrorEnum.GeneralError; // Return a general error code for unexpected errors
            }
        }

        #endregion

        #region private methods
        private string GenerateUserId(DbGenerateId idType)
        {
            // Determine the prefix based on the ID type, defaulting to "E" for UserBase
            string appendId = idType switch
            {
                DbGenerateId.ADMIN => "A",
                DbGenerateId.MACHINE => "M",
                _ => "E" // Default to "E" for UserBase
            };

            Random random = new Random();
            int newId;

            do
            {
                // Generate a random integer within a range (e.g., 100000 to 999999)
                newId = random.Next(100000, 999999);
            }
            while (IsIdInUse(idType, appendId + newId)); // Check uniqueness in the appropriate DbSet

            Console.WriteLine(appendId + newId);

            return appendId + newId;
        }

        private bool IsIdInUse(DbGenerateId idType, string generatedId)
        {
            // Dynamically check the appropriate DbSet for uniqueness
            return idType switch
            {
                DbGenerateId.ADMIN => _context.Admin_Details.Any(u => u.AdminId == generatedId),
                DbGenerateId.MACHINE => _context.Machines.Any(m => m.MachineId == generatedId),
                _ => _context.Users.Any(u => u.UserId == generatedId) // Default to UserBase
            };
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private bool VerifyPassword(string plainTextPassword, string hashedPassword)
        {
            string hashedInput = HashPassword(plainTextPassword);
            return hashedPassword == hashedInput;
        }

        public Task<List<Machine>> GetListOfMachines()
        {
            return _context.Machines.ToListAsync();
        }
        #endregion
    }
}
