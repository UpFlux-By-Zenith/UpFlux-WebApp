using Amazon.KeyManagementService;
using Amazon.KeyManagementService.Model;
using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;
using Upflux_WebService.GrpcServices.Interfaces;
using GrpcServer;
using System.Xml.Linq;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

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

		//public async Task<bool> ValidateLicence(string licenceXml)
		//{
		//	using var sha256 = System.Security.Cryptography.SHA256.Create();
		//	XDocument receivedXml = XDocument.Parse(licenceXml);

		//	// Extract values from the received XML
		//	string? expirationDate = receivedXml.Root?.Element("ExpirationDate")?.Value;
		//	string? machineId = receivedXml.Root?.Element("MachineID")?.Value;
		//	string? signature = receivedXml.Root?.Element("Signature")?.Value;

		//	if (string.IsNullOrEmpty(expirationDate) || string.IsNullOrEmpty(machineId) || string.IsNullOrEmpty(signature))
		//		return false;

		//	// Retrieve the existing license metadata from the repository
		//	var existingLicenceMetadata = await _licenceRepository.GetLicenceByMachineId(machineId);
		//	if (existingLicenceMetadata is null)
		//		return false;

		//	var existingLicenceXml = new XElement("Licence",
		//		new XElement("ExpirationDate", existingLicenceMetadata.ExpirationDate),
		//		new XElement("MachineID", existingLicenceMetadata.MachineId)
		//	);

		//	var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(existingLicenceXml.ToString()));
		//	var expectedSignature = await SignDataAsync(existingLicenceMetadata.LicenceKey, hash);

		//	if (DateTime.TryParse(expirationDate, out var expDate) && DateTime.UtcNow > expDate)
		//		return false;

		//	// Compare the received signature with the expected one
		//	return signature == Convert.ToBase64String(expectedSignature);
		//}

		public async Task<bool> ValidateLicence(string licenceXml)
		{
			using var sha256 = SHA256.Create();

			// Parse the received XML
			var receivedXml = XDocument.Parse(licenceXml);

			// Extract values from the received XML
			string? expirationDate = receivedXml.Root?.Element("ExpirationDate")?.Value;
			string? machineId = receivedXml.Root?.Element("MachineID")?.Value;
			string? signature = receivedXml.Root?.Element("Signature")?.Value;

			if (string.IsNullOrEmpty(expirationDate) || string.IsNullOrEmpty(machineId) || string.IsNullOrEmpty(signature))
			{
				Console.WriteLine("Invalid XML format.");
				return false;
			}

			// Remove the Signature element for consistent hashing
			receivedXml.Root?.Element("Signature")?.Remove();

			// Normalize the XML for hashing
			var normalizedXml = NormalizeXml(receivedXml);

			// Compute the hash of the normalized XML
			var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedXml));
			var signatureBytes = Convert.FromBase64String(signature);

			// Retrieve the license metadata from the repository
			var existingLicenceMetadata = await _licenceRepository.GetLicenceByMachineId(machineId);
			if (existingLicenceMetadata is null)
			{
				Console.WriteLine("No matching license found.");
				return false;
			}

			// Verify the signature using AWS KMS
			if (!await VerifyWithKmsAsync(existingLicenceMetadata.LicenceKey, hash, signatureBytes))
			{
				Console.WriteLine("Signature validation failed. XML may have been tampered with.");
				return false;
			}

			// Compare the verified XML data with the cloud's metadata
			if (expirationDate != existingLicenceMetadata.ExpirationDate.ToString())
			{
				Console.WriteLine("Expiration date mismatch.");
				return false;
			}

			if (machineId != existingLicenceMetadata.MachineId)
			{
				Console.WriteLine("Machine ID mismatch.");
				return false;
			}

			// Check the expiration date
			if (DateTime.TryParse(expirationDate, out var expDate) && DateTime.UtcNow > expDate)
			{
				Console.WriteLine("License has expired.");
				return false;
			}

			Console.WriteLine("License is valid.");
			return true;
		}

		private async Task<bool> VerifyWithKmsAsync(string keyId, byte[] hash, byte[] signature)
		{
			using var kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.EUNorth1);

			try
			{
				var request = new VerifyRequest
				{
					KeyId = keyId, // Ensure this matches the KeyId from CreateKeyAsync
					Message = new MemoryStream(hash),
					MessageType = MessageType.DIGEST, // Hash is passed as a pre-computed digest
					Signature = new MemoryStream(signature),
					SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256 // Must match the signing algorithm used
				};

				var response = await kmsClient.VerifyAsync(request);
				return response.SignatureValid; // True if the signature is valid
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error during verification: {ex.Message}");
				return false; // Return false if verification fails
			}
		}

		private bool VerifySignature(byte[] hash, byte[] signature, string publicKey)
		{
			using var rsa = RSA.Create();
			rsa.ImportFromPem(publicKey);

			return rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
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
		/// <param name="hash">the hash that is to be signed</param>
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
			// Create the XML using XDocument
			var doc = new XDocument(
				new XElement("Licence",
					new XElement("ExpirationDate", licence.ExpirationDate),
					new XElement("MachineID", licence.MachineId) // Consistent capitalization
				)
			);

			// Normalize the XML to a string for hashing
			var normalizedXml = NormalizeXml(doc);

			// Compute the hash of the normalized XML
			using var sha256 = SHA256.Create();
			var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedXml));

			// Sign the hash using AWS KMS
			var signature = await SignDataAsync(licence.LicenceKey, hash);

			// Add the signature to the XML
			doc.Root?.Add(new XElement("Signature", Convert.ToBase64String(signature)));

			// Return the signed XML as a string
			return doc.ToString(SaveOptions.DisableFormatting); // Compact formatting for consistency
		}

		private string NormalizeXml(XDocument doc)
		{
			using var stringWriter = new StringWriter();
			using var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings
			{
				OmitXmlDeclaration = true, // Remove XML declaration
				Indent = false // Compact format without extra whitespace
			});
			doc.Save(xmlWriter);
			return stringWriter.ToString();
		}

		#endregion
	}
}