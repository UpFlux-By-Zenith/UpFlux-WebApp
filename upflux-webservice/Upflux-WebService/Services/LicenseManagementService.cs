using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service that deals with licensing
	/// </summary>
	public class LicenseManagementService : ILicenseManagementService
	{
		#region private members
		private readonly ILicenseRepository _licenseRepository;
		private readonly IMachineRepository _machineRepository;
		#endregion

		#region public methods
		public LicenseManagementService(ILicenseRepository licenseRepository, IMachineRepository machineRepository)
		{
			_licenseRepository = licenseRepository;
			_machineRepository = machineRepository;
		}

		/// <summary>
		/// Receives machine Id, generate license and store metadata
		/// </summary>
		/// <param name="machineId">the machine id which the licence belong to</param>
		public async Task CreateLicense(string machineId)
		{
			var machine = await _machineRepository.GetByIdAsync(machineId);

			if (machine is null)
				throw new KeyNotFoundException($"Machine with ID {machineId} was not found.");

			var key = await CreateKmsKeyAsync();

			await _licenseRepository.AddAsync(new Core.Models.License
			{
				LicenseKey = key.KeyMetadata.KeyId,
				MachineId = machineId,
				ValidityStatus = "Valid",
				ExpirationDate = DateTime.UtcNow.AddYears(1)
			});

			// Call cloud communication service and send XML file
			// Call machine relate method to change their status

			await _licenseRepository.SaveChangesAsync();
		}
		#endregion

		#region private methods
		/// <summary>
		/// Creates a Asymmetric key using aws user details
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

		/// <summary>
		/// Uses a KMS managed key to sign a hashed item
		/// </summary>
		/// <param name="keyId">the id of the key that is to be used</param>
		/// <param name="hash"></param>
		/// <returns></returns>
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