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
		private readonly ILogger<GeneratedMachineIdService> _logger;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="generatedMachineIdRepository"></param>
		public GeneratedMachineIdService(IGeneratedMachineIdRepository generatedMachineIdRepository, ILogger<GeneratedMachineIdService> logger)
		{
			_generatedMachineIdRepository = generatedMachineIdRepository;
			_logger = logger;
		}

		/// <summary>
		/// Save machineId in the database along side its creation time
		/// </summary>
		/// <param name="machineId">the generated machine Id that is to be saved</param>
		/// <returns></returns>
		public async Task SaveGeneratedMachineId(string machineId) {
			_logger.LogInformation("Starting to save generated machine ID: {MachineId}", machineId);

			try
			{
				var generatedMachineId = new GeneratedMachineId()
				{
					MachineId = machineId,
					CreatedAt = DateTime.UtcNow
				};

				await _generatedMachineIdRepository.AddAsync(generatedMachineId);
				await _generatedMachineIdRepository.SaveChangesAsync();

				_logger.LogInformation("Successfully saved generated machine ID: {MachineId}", machineId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while saving generated machine ID: {MachineId}", machineId);
				throw; 
			}
		}
	}
}
