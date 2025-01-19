
using Upflux_WebService.Core.Models;
using Upflux_WebService.Core.Models.Enums;
using Upflux_WebService.Services.Enums;

namespace Upflux_WebService.Services.Interfaces
{
    /// <summary>
    /// Interface for querying entities from the database.
    /// </summary>
    public interface IEntityQueryService
    {
      Task<DbErrorEnum> CreateAdminAccount(string name,string email, string password);

      Task<DbErrorEnum> CheckAdminLogin(string email, string password);
      Task<(string,DbErrorEnum)> CreateUser(string email, string name, UserRole role = UserRole.Engineer);

      Task<DbErrorEnum> AddCredentials(string userId, List<string> machineIds, DateTime accessGranted, DateTime expiry);
      Task<DbErrorEnum> CreateEngineerCredentials(string email, string name, List<string> machineIds, DateTime accessGranted, DateTime expiry);
      Task<List<Machine>> GetListOfMachines();
    }
}
