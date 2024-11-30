using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	public class LicenseManagementService : ILicenseManagementService
	{
		private readonly ILicenceRepository _licenceRepository;

		private readonly IMachineRepository _machineRepository;

		public LicenseManagementService(ILicenceRepository licenceRepository, IMachineRepository machineRepository)
		{
			_licenceRepository = licenceRepository;
			_machineRepository = machineRepository;
		}

		/// <summary>
		/// Receives machine Id, generate license and store metadata
		/// </summary>
		/// <param name="machineId"></param>
		public async Task CreateLicence(int machineId)
		{
			var machine = await _machineRepository.GetByIdAsync(machineId);
			if (machine is null)
				throw new KeyNotFoundException($"Machine with ID {machineId} was not found.");

			await _licenceRepository.AddAsync(new Licence
			{
				LicenceKey = "JKL456" + machineId,
				MachineId = machineId,
				ValidityStatus = "Valid",
				ExpirationDate = new DateTime(2026, 12, 31, 23, 59, 59)
			});

			await _licenceRepository.SaveChangesAsync();
		}

		static async Task<CreateKeyResponse> CreateKmsKeyAsync()
		{
			var request = new CreateKeyRequest
			{
				Description = "Asymmetric key for signing and verifying messages",
				KeyUsage = KeyUsageType.SIGN_VERIFY, // Specify SIGN_VERIFY for asymmetric keys
				CustomerMasterKeySpec = CustomerMasterKeySpec.RSA_2048 // Choose RSA_2048 for signing
			};

			try
			{
				var kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.USEast1); // Change region if needed

				// Create the KMS key
				var response = await kmsClient.CreateKeyAsync(request);
				return response;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating KMS key: {ex.Message}");
				throw;
			}
		}
	}
}