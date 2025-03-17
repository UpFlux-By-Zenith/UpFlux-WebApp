using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository;

public interface IMachineStoredVersionsRepository : IRepository<MachineStoredVersions>
{
    public List<MachineStoredVersions> GetMachineStoredVersions();

    public List<MachineStoredVersions> GetMachineStoredVersions(string machineId);
}