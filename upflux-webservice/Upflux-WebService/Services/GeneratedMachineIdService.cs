using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service related to logging generated machine ids
	/// </summary>
	public class GeneratedMachineIdService : IGeneratedMachineIdService
	{
		private readonly IGeneratedMachineIdRepository _generatedMachineIdRepository;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="generatedMachineIdRepository"></param>
		public GeneratedMachineIdService(IGeneratedMachineIdRepository generatedMachineIdRepository)
		{
			_generatedMachineIdRepository = generatedMachineIdRepository;
		}

		/// <summary>
		/// Save machineId in the database along side its creation time
		/// </summary>
		/// <param name="machineId">the generated machine Id that is to be saved</param>
		/// <returns></returns>
		public async Task SaveGeneratedMachineId(string machineId) {

			var generatedMachineId = new GeneratedMachineId()
			{
				MachineId = machineId,
				CreatedAt = DateTime.UtcNow
			};

			await _generatedMachineIdRepository.AddAsync(generatedMachineId);
			await _generatedMachineIdRepository.SaveChangesAsync();
		}
	}
}
