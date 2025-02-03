using Upflux_WebService.Core.DTOs;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Core.Models.Enums;
using Upflux_WebService.Services.Enums;
using static Upflux_WebService.Services.EntityQueryService;

namespace Upflux_WebService.Services.Interfaces;

/// <summary>
/// Interface for querying entities from the database.
/// </summary>
public interface IEntityQueryService
{
    Task<DbErrorEnum> CreateAdminAccount(string name, string email, string password);

    Task<DbErrorEnum> CheckAdminLogin(string email, string password);

    Task<(string, DbErrorEnum)> CreateUser(string email, string name, UserRole role = UserRole.Engineer);

    Task<DbErrorEnum> CreateEngineerCredentials(string adminEmail, string enginneerEmail, string name,
        List<string> machineIds, DateTime accessGranted, DateTime expiry);

    Task<List<Machine>> GetListOfMachines();

    Task<List<Machine>> GetListOfMachines(List<string> machineIds);

    Task<DbErrorEnum> CheckEngineerLogin(string email);

    /// <summary>
    /// Retrieves all applications with their versions.
    /// </summary>
    /// <returns>A list of applications along with their versions.</returns>
    Task<List<Application>> GetApplicationsWithVersionsAsync();

    string GenerateUserId(DbGenerateId idType);

    /// <summary>
    /// Returns list of all engineers
    /// </summary>
    /// <returns></returns>
    Task<List<Users>> GetAllEngineers();

    /// <summary>
    /// Returns Machine with respective license
    /// </summary>
    /// <returns></returns>
    Task<List<MachineWithLicenceDto>> GetAllMachinesWithLicences();

    /// <summary>
    /// Returns Machines with its respective applications withs versions
    /// </summary>
    /// <returns></returns>
    Task<List<Machine>> GetListOfMachinesWithApplications();

    Task<List<Machine>> GetListOfMachinesWithApplications(List<string> machineIds);
}