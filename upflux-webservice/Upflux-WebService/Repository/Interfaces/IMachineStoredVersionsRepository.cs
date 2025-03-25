using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;

namespace Upflux_WebService.Repository;

public interface IMachineStoredVersionsRepository : IRepository<MachineStoredVersion>
{
    public List<MachineStoredVersion> GetMachineStoredVersions();

    public List<MachineStoredVersion> GetMachineStoredVersions(string machineId);
}