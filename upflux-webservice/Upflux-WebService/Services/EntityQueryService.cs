using System.Security.Cryptography;
using System.Text;
using Upflux_WebService.Core.Models.Enums;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;
using Upflux_WebService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Upflux_WebService.Services.Enums;
using System.Data;
using Upflux_WebService.Core.DTOs;

namespace Upflux_WebService.Services;

public class EntityQueryService : IEntityQueryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EntityQueryService> _logger;

    public enum DbGenerateId
    {
        ADMIN,
        ENGINEER,
        MACHINE
    }

    #region public methods

    public EntityQueryService(ApplicationDbContext context, ILogger<EntityQueryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of applications along with their versions.
    /// </summary>
    /// <returns>A list of applications with their versions.</returns>
    public async Task<List<Application>> GetApplicationsWithVersionsAsync()
    {
        _logger.LogInformation("Fetching applications with versions.");

        try
        {
            var applicationsWithVersions = await _context.Applications
                .Include(a => a.Versions)
                .ToListAsync();

            _logger.LogInformation("Successfully fetched {Count} applications with versions.",
                applicationsWithVersions.Count);
            return applicationsWithVersions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching applications with versions.");
            throw;
        }
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


    public async Task<DbErrorEnum> CreateAdminAccount(string name, string email, string password)
    {
        _logger.LogInformation("Creating admin account for email: {Email}", email);

        // Check if the email already exists in the UserBase table
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            _logger.LogWarning("Admin account creation failed: Email {Email} is already in use.", email);
            return DbErrorEnum.UserNotFound; // Return UserNotFound if email is already taken
        }

        var userId = CreateUser(email, name).Result.Item1;

        // Hash the password before storing
        var hashedPassword = HashPassword(password);

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
            _logger.LogError(ex, "Error occurred while creating admin account for email: {Email}", email);
            return DbErrorEnum.GeneralError;
        }

        // If everything is successful, return success
        _logger.LogInformation("Admin account successfully created for email: {Email}", email);
        return DbErrorEnum.Success;
    }

    public async Task<(string, DbErrorEnum)> CreateUser(string email, string name, UserRole role = UserRole.Engineer)
    {
        _logger.LogInformation("Starting user creation for email: {Email}, role: {Role}", email, role);

        // Check if the email already exists in the UserBases table
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

        if (existingUser != null)
        {
            _logger.LogWarning("User creation failed: Email {Email} is already in use.", email);
            return (null, DbErrorEnum.UserNotFound); // Return error if email is already taken
        }

        // Generate a new userId
        var userId = GenerateUserId(DbGenerateId.ENGINEER);

        // Create a new UserBase object
        var newUser = new Users
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
        _logger.LogInformation("User created successfully: UserId={UserId}, Email={Email}, Role={Role}", userId, email,
            role);
        return (userId, DbErrorEnum.Success);
    }

    public async Task<DbErrorEnum> CreateEngineerCredentials(string adminEmail, string engineerEmail, string name,
        List<string> machineIds, DateTime accessGranted, DateTime expiry)
    {
        _logger.LogInformation("Creating engineer credentials for engineer: {EngineerEmail} by admin: {AdminEmail}",
            engineerEmail, adminEmail);

        var userId = CreateUser(engineerEmail, name).Result.Item1;

        var adminId = _context.Admin_Details
            .Join(_context.Users, a => a.UserId, u => u.UserId, (a, u) => new { a.AdminId, u.Email })
            .Where(x => x.Email.ToLower() == adminEmail.ToLower())
            .Select(x => x.AdminId)
            .FirstOrDefault();

        if (adminId == null) return DbErrorEnum.UserNotFound;

        Console.WriteLine($"User with userId '{userId}' found.");

        // Validate that all machine IDs in the input list exist in the database
        var existingMachineIds = await _context.Machines
            .Where(m => machineIds.Contains(m.MachineId))
            .Select(m => m.MachineId)
            .ToListAsync();

        var invalidMachineIds = machineIds.Except(existingMachineIds).ToList();

        if (invalidMachineIds.Any())
        {
            _logger.LogWarning("Engineer credential creation failed: Invalid machine IDs provided: {InvalidMachineIds}",
                string.Join(", ", invalidMachineIds));
            return DbErrorEnum.MachineNotFound; // Return error if any machineIds are invalid
        }

        // Check for duplicate credentials in a single query
        var existingCredentials = await _context.Credentials
            .Where(c => c.UserId == userId && machineIds.Contains(c.MachineId))
            .Select(c => c.MachineId)
            .ToListAsync();

        if (existingCredentials.Any())
        {
            _logger.LogWarning(
                "Engineer credential creation failed: Duplicate credentials for machine IDs: {MachineIds}",
                string.Join(", ", existingCredentials));
            return DbErrorEnum.GeneralError; // Return error if duplicates are found
        }

        // Prepare credentials to add
        var credentialsList = machineIds.Select(machineId => new Credentials
        {
            UserId = userId,
            MachineId = machineId,
            AccessGrantedAt = accessGranted,
            ExpiresAt = expiry,
            AdminId = adminId
        }).ToList();
        await _context.Database.ExecuteSqlRawAsync($"SET @current_user_id = 'e1'");

        // Add credentials to the database
        await _context.Credentials.AddRangeAsync(credentialsList);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Engineer credentials successfully created for engineer: {EngineerEmail}",
            engineerEmail);
        return DbErrorEnum.Success;
    }

    public async Task<DbErrorEnum> CheckEngineerLogin(string email)
    {
        _logger.LogInformation("Checking engineer login for email: {Email}", email);

        try
        {
            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            // Check if the user exists
            if (user == null)
            {
                _logger.LogWarning("Engineer login failed: User not found for email: {Email}", email);
                return DbErrorEnum.UserNotFound;
            }

            // Update last_login timestamp
            user.LastLogin = DateTime.UtcNow;
            _context.Users.Update(user);

            // Save changes to the database
            await _context.SaveChangesAsync();

            _logger.LogInformation("Engineer login successful for email: {Email}", email);
            return DbErrorEnum.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking engineer login for email: {Email}", email);
            return DbErrorEnum.GeneralError;
        }
    }

    public string GenerateUserId(DbGenerateId idType)
    {
        // Determine the prefix based on the ID type, defaulting to "E" for UserBase
        var appendId = idType switch
        {
            DbGenerateId.ADMIN => "A",
            DbGenerateId.MACHINE => "M",
            _ => "E" // Default to "E" for UserBase
        };

        var random = new Random();
        int newId;

        do
        {
            // Generate a random integer within a range (e.g., 100000 to 999999)
            newId = random.Next(100000, 999999);
        } while (IsIdInUse(idType, appendId + newId)); // Check uniqueness in the appropriate DbSet

        Console.WriteLine(appendId + newId);

        return appendId + newId;
    }

    public async Task<List<Users>> GetAllEngineers()
    {
        return await _context.Users
            .Where(u => u.Role == UserRole.Engineer) // Filters only Engineers
            .ToListAsync();
    }

    public async Task<List<MachineWithLicenceDto>> GetAllMachinesWithLicences()
    {
        return await _context.Machines
            .GroupJoin(
                _context.Licences,
                machine => machine.MachineId,
                licence => licence.MachineId,
                (machine, licences) => new { machine, licences }
            )
            .SelectMany(
                m => m.licences.DefaultIfEmpty(), // Ensures machines without licences are included
                (m, licence) => new MachineWithLicenceDto
                {
                    MachineId = m.machine.MachineId,
                    MachineName = m.machine.machineName,
                    DateAddedOn = m.machine.dateAddedOn,
                    IpAddress = m.machine.ipAddress,

                    // Licence details (can be null if no licence exists)
                    LicenceKey = licence != null ? licence.LicenceKey : null,
                    ValidityStatus = licence != null ? licence.ValidityStatus : null,
                    ExpirationDate = licence != null ? licence.ExpirationDate : null
                }
            )
            .ToListAsync();
    }

    public async Task<List<Machine>> GetListOfMachinesWithApplications()
    {
        var machinesWithApplications = await _context.Machines
            .Include(m => m.Applications) // Including related Applications
            .ThenInclude(a => a.Versions) // Including related Application Versions
            .ToListAsync();

        return machinesWithApplications;
    }


    #endregion

    #region private methods

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
        var hashedInput = HashPassword(plainTextPassword);
        return hashedPassword == hashedInput;
    }

    public Task<List<Machine>> GetListOfMachines()
    {
        return _context.Machines.ToListAsync();
    }

    public Task<List<Machine>> GetListOfMachines(List<string> machineIds)
    {
        return _context.Machines
            .Where(m => machineIds.Contains(m.MachineId)) // Assuming MachineId is the property you're filtering by
            .ToListAsync();
    }

    #endregion
}