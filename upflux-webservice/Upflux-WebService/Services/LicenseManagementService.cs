using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	public class LicenseManagementService : ILicenseManagementService
	{
		#region private members
		private readonly ILicenseRepository _licenceRepository;
		private readonly IMachineRepository _machineRepository;
		#endregion

		#region public methods
		public LicenseManagementService(ILicenseRepository licenceRepository, IMachineRepository machineRepository)
		{
			_licenceRepository = licenceRepository;
			_machineRepository = machineRepository;
		}

		/// <summary>
		/// Receives machine Id, generate license and store metadata
		/// </summary>
		/// <param name="machineId">the machine id which the licence belong to</param>
		public async Task CreateLicense(int machineId)
		{
			var machine = await _machineRepository.GetByIdAsync(machineId);

			if (machine is null)
				throw new KeyNotFoundException($"Machine with ID {machineId} was not found.");

			var key = await CreateKmsKeyAsync();

			await _licenceRepository.AddAsync(new Core.Models.License
			{
				LicenseKey = key.KeyMetadata.KeyId,
				MachineId = machineId,
				ValidityStatus = "Valid",
				ExpirationDate = DateTime.UtcNow.AddYears(1)
			});

			// Call cloud communication service and send XML file here

			await _licenceRepository.SaveChangesAsync();
		}
		#endregion

		#region private methods
		/// <summary>
		/// Creates a Asymmetric key using 
		/// </summary>
		/// <returns>a key pair that is used for signing and verifying</returns>
		private async Task<CreateKeyResponse> CreateKmsKeyAsync()
		{
			var request = new CreateKeyRequest
			{
				Description = "Asymmetric key for signing and verifying messages",
				KeyUsage = KeyUsageType.SIGN_VERIFY, 
				KeySpec = KeySpec.RSA_2048
			};

			try
			{
				var kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.USEast1); 
				var response = await kmsClient.CreateKeyAsync(request);
				return response;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error creating KMS key: {ex.Message}");
				throw;
			}
		}

		private async Task<byte[]> SignDataAsync(string keyId, byte[] hash)
		{
			try
			{
				var kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.USEast1);

				var signRequest = new SignRequest
				{
					KeyId = keyId,
					Message = new MemoryStream(hash), 
					MessageType = MessageType.DIGEST, 
					SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256
				};

				var signResponse = await kmsClient.SignAsync(signRequest);
				return signResponse.Signature.ToArray();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error signing data: {ex.Message}");
				throw;
			}
		}
		#endregion
	}
}