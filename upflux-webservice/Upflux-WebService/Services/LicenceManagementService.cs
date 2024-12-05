using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;
using Upflux_WebService.GrpcServices.Interfaces;
using GrpcServer;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service that deals with licensing
	/// </summary>
	public class LicenceManagementService : ILicenceManagementService
	{
		#region private members
		private readonly ILicenceRepository _licenceRepository;
		private readonly IMachineRepository _machineRepository;
		private readonly ILicenceCommunicationService _licenceCommunicationService;
		#endregion

		#region public methods
		public LicenceManagementService(ILicenceRepository licenceRepository, IMachineRepository machineRepository, ILicenceCommunicationService licenceCommunicationService)
		{
			_licenceRepository = licenceRepository;
			_machineRepository = machineRepository;
			_licenceCommunicationService = licenceCommunicationService;
		}

		/// <summary>
		/// Receives machineId, generate licence and store metadata. If a licence exists for the machineId it overrides
		/// </summary>
		/// <param name="machineId">the machine id which the licence belong to</param>
		public async Task CreateLicence(string machineId)
		{
			// need to check for existing license as well
			var existingLicence = await ValidateMachineAndGetLicence(machineId);

			// check for existing licence
			if (existingLicence != null)
			{
				// Update the existing licence
				await RenewExistingLicence(existingLicence);

				// Fetch the updated licence for signing
				var updatedLicenceFile = await CreateSignedFile(existingLicence);

				// Notify clients
				await _licenceCommunicationService.PushLicenceUpdateAsync(new LicenceFileUpdate
				{
					LicenceFileXml = updatedLicenceFile
				});

				return;
			}

			//create new licence
			var key = await CreateKmsKeyAsync();

			var licence = new Licence
			{
				LicenceKey = key.KeyMetadata.KeyId,
				MachineId = machineId,
				ValidityStatus = "Valid",
				ExpirationDate = DateTime.UtcNow.AddYears(1)
			};

			await _licenceRepository.AddAsync(licence);
			await _licenceRepository.SaveChangesAsync();

			var licenceFile = await CreateSignedFile(licence);

			await _licenceCommunicationService.PushLicenceUpdateAsync(new LicenceFileUpdate
			{
				LicenceFileXml = licenceFile
			});
		}
		#endregion

		#region private methods

		/// <summary>
		/// Check if machine exists, and get its licence
		/// </summary>
		/// <param name="machineId">the id of the machine being checked</param>
		/// <returns>Licence metadata</returns>
		/// <exception cref="KeyNotFoundException"></exception>
		private async Task<Licence?> ValidateMachineAndGetLicence(string machineId)
		{
			var machine = await _machineRepository.GetByIdAsync(machineId);
			if (machine is null)
				throw new KeyNotFoundException($"Machine with ID {machineId} was not found.");

			return await _licenceRepository.GetLicenceByMachineId(machineId);
		}

		/// <summary>
		/// Renew licence by resetting expiry date
		/// </summary>
		/// <param name="existingLicence">the licence being renewed</param>
		/// <returns></returns>
		private async Task RenewExistingLicence(Licence existingLicence)
		{
			existingLicence.ValidityStatus = "Valid";
			existingLicence.ExpirationDate = DateTime.UtcNow.AddYears(1);

			_licenceRepository.Update(existingLicence);
			await _licenceRepository.SaveChangesAsync();
		}

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

			var kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.EUNorth1);
			var response = await kmsClient.CreateKeyAsync(request);
			return response;
		}

		/// <summary>
		/// Uses a KMS managed key to sign a hashed item
		/// </summary>
		/// <param name="keyId">the id of the key that is to be used</param>
		/// <param name="hash"></param>
		/// <returns></returns>
		private async Task<byte[]> SignDataAsync(string keyId, byte[] hash)
		{
			var kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.EUNorth1);

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

		/// <summary>
		/// Create signature using icence data and insert it into the response
		/// </summary>
		/// <param name="licence">licence being signed</param>
		/// <returns>signed licence</returns>
		private async Task<string> CreateSignedFile(Licence licence)
		{
			var xmlContent = $@"
				<Licence>
					<ExpirationDate>{licence.ExpirationDate}</ExpirationDate>
					<MachineId>{licence.MachineId}</MachineId>
				</Licence>";

			using var sha256 = System.Security.Cryptography.SHA256.Create();
			var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(xmlContent));

			// Sign the hash
			var signature = await SignDataAsync(licence.LicenceKey, hash);

			// Add the signature to the XML
			var signedXmlContent = $@"
				<Licence>
					<ExpirationDate>{licence.ExpirationDate}</ExpirationDate>
					<DeviceID>{licence.MachineId}</DeviceID>
					<Signature>{Convert.ToBase64String(signature)}</Signature>
				</Licence>";

			return signedXmlContent;
		}
		#endregion
	}
}