namespace Upflux_WebService.Repository.Interfaces
{
	public interface IGeneratedMachineIdRepository : IRepository<GeneratedMachineId>
	{
		Task<GeneratedMachineId?> GetByMachineId(string machineId);
	}
}
