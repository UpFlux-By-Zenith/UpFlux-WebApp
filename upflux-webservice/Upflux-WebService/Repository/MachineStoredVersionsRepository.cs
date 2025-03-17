using System.Linq.Expressions;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Data;

namespace Upflux_WebService.Repository;

public class MachineStoredVersionsRepository : Repository<MachineStoredVersions>, IMachineStoredVersionsRepository
{
    private readonly ApplicationDbContext _context;

    public MachineStoredVersionsRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public List<MachineStoredVersions> GetMachineStoredVersions()
    {
        return _context.MachineStoredVersions.ToList();
    }

    public List<MachineStoredVersions> GetMachineStoredVersions(string machineId)
    {
        return _context.MachineStoredVersions.ToList().Where(v => v.MachineId == machineId).ToList();
    }
}