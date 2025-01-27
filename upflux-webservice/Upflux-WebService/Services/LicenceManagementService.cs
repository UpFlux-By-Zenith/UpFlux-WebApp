using Upflux_WebService.Core.Models;
using Upflux_WebService.Repository.Interfaces;
using Upflux_WebService.Services.Interfaces;
using Upflux_WebService.GrpcServices.Interfaces;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using LicenceCommunication;
using Upflux_WebService.Core.DTOs;
using UpFlux_WebService;

///
///  ************************TO BE REMOVED*********************************
///

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
		private readonly IKmsService _kmsService;
		private readonly IXmlService _xmlService;
		private readonly ILogger<LicenceManagementService> _logger;
		private readonly IControlChannelService _controlChannelService;
		private readonly string _gatewayId;
		#endregion

		#region public methods

		/// <summary>
		/// Constructor
		/// </summary>
		public LicenceManagementService(
			ILicenceRepository licenceRepository,
			IMachineRepository machineRepository,
			IKmsService kmsService,
			IXmlService xmlService,
			ILogger<LicenceManagementService> logger,
			IControlChannelService controlChannelService,
			IConfiguration configuration)
		{
			_licenceRepository = licenceRepository;
			_machineRepository = machineRepository;
			_kmsService = kmsService;
			_xmlService = xmlService;
			_logger = logger;
			_controlChannelService = controlChannelService;
			_gatewayId = configuration["GatewayId"]!;
		}

		/// <summary>
		/// Retrieves a license by the machine ID.
		/// </summary>
		/// <param name="machineId">The ID of the machine for which the license is requested.</param>
		/// <returns>A task representing the asynchronous operation, containing the license response DTO.</returns>
		public async Task<LicenceResponse?> GetLicenceByMachineId(string machineId)
		{
			_logger.LogInformation("Retrieving license for Machine ID: {MachineId}", machineId);

			try
			{
				var licence = await _licenceRepository.GetLicenceByMachineId(machineId);
				if (licence == null)
				{
					_logger.LogWarning("No license found for Machine ID: {MachineId}", machineId);
					return null;
				}

				return new LicenceResponse
				{
					MachineId = licence.MachineId,
					ExpirationDate = licence.ExpirationDate
				};
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving licence for Machine ID: {MachineId}", machineId);
				throw;
			}
		}

		/// <summary>
		/// Retrieves all licenses.
		/// </summary>
		/// <returns>A task representing the asynchronous operation, containing a list of license response DTOs.</returns>
		public async Task<IEnumerable<LicenceResponse>> GetAllLicences()
		{
			_logger.LogInformation("Retrieving all licences.");

			try
			{
				var licences = await _licenceRepository.GetAllAsync();

				return licences.Select(l => new LicenceResponse
				{
					MachineId = l.MachineId,
					ExpirationDate = l.ExpirationDate
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while retrieving all licences.");
				throw;
			}
		}

		/// <summary>
		/// Generates a new license or renews an existing one for the specified machine ID.
		/// If a license already exists for the given machine ID, it is renewed with updated metadata.
		/// If no license exists, a new license is created, stored, and pushed for communication.
		/// </summary>
		/// <param name="machineId">The machine ID to associate with the license.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task CreateLicence(string machineId)
		{
			_logger.LogInformation("Starting licence creation for Machine ID: {MachineId}", machineId);

			try
			{
				// Check for existing license and renew if available
				var existingLicence = await ValidateMachineAndGetLicence(machineId);

				if (existingLicence != null)
				{
					_logger.LogInformation("Existing license found for Machine ID: {MachineId}. Renewing license.", machineId);

					await RenewExistingLicence(existingLicence);
					var updatedLicenceFile = await CreateSignedFile(existingLicence);

					await _controlChannelService.SendLicenceResponseAsync(_gatewayId, existingLicence.MachineId, true, updatedLicenceFile, existingLicence.ExpirationDate);

					_logger.LogInformation("No existing license found for Machine ID: {MachineId}. Creating new license.", machineId);
					return;
				}

				// Create new license
				var keyId = await _kmsService.CreateKeyAsync();

				var now = DateTime.UtcNow;
				var truncatedExpirationDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);

				var licence = new Licence
				{
					LicenceKey = keyId,
					MachineId = machineId,
					ValidityStatus = "Valid",
					ExpirationDate = truncatedExpirationDate.AddYears(1)
				};

				await _licenceRepository.AddAsync(licence);
				await _licenceRepository.SaveChangesAsync();

				var licenceFile = await CreateSignedFile(licence);

				await _controlChannelService.SendLicenceResponseAsync(_gatewayId, licence.MachineId, true, licenceFile, licence.ExpirationDate);

				_logger.LogInformation("Successfully created new license for Machine ID: {MachineId}", machineId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while creating license for Machine ID: {MachineId}", machineId);
				throw;
			}
		}

		/// <summary>
		/// Validates a license by checking its XML content, signature, and metadata consistency.
		/// </summary>
		/// <param name="licenceXml">The XML string representing the license to validate.</param>
		/// <returns>
		/// A <see cref="LicenceValidationResponse"/> indicating whether the license is valid, along with an error message if invalid.
		/// </returns>
		public async Task<LicenceValidationResponse> ValidateLicence(string licenceXml)
		{
			_logger.LogInformation("Starting license validation.");

			try
			{
				// Parse and normalize XML
				var receivedXml = _xmlService.ParseXml(licenceXml);

				if (!TryExtractLicenceValues(receivedXml, out var expirationDate, out var machineId, out var signature))
				{
					_logger.LogWarning("License validation failed: Invalid XML content.");
					return new LicenceValidationResponse() { IsValid = false, Message = "Invalid XML Content." };
				}

				_logger.LogInformation("Validating license for Machine ID: {MachineId}", machineId);

				receivedXml.Root?.Element("Signature")?.Remove();
				var normalizedXml = _xmlService.NormalizeXml(receivedXml);

				// Compute the hash of the normalized XML
				using var sha256 = SHA256.Create();
				var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedXml));

				var existingLicenceMetadata = await _licenceRepository.GetLicenceByMachineId(machineId);
				if (existingLicenceMetadata == null)
				{
					_logger.LogWarning("License validation failed: No matching license found for Machine ID: {MachineId}", machineId);
					return new LicenceValidationResponse() { IsValid = false, Message = "No Matching Licence" };
				}

				// Verify the signature
				if (!await _kmsService.VerifySignatureAsync(existingLicenceMetadata.LicenceKey, hash, Convert.FromBase64String(signature)))
				{
					_logger.LogWarning("License validation failed: Invalid signature for Machine ID: {MachineId}", machineId);
					return new LicenceValidationResponse() { IsValid = false, Message = "Invalid Signature." };
				}

				// Validate metadata consistency
				if (!ValidateMetadata(expirationDate, machineId, existingLicenceMetadata))
				{
					_logger.LogWarning("License validation failed: Invalid Licence Metadata for Machine ID: {MachineId}", machineId);
					return new LicenceValidationResponse() { IsValid = false, Message = "Invalid Metadata" };
				}

				_logger.LogInformation("License validation successful for Machine ID: {MachineId}", machineId);
				return new LicenceValidationResponse() { IsValid = true, Message = "Licence is valid." };
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred while validating license.");
				return new LicenceValidationResponse() { IsValid = false, Message = "Internal Server Error." };
			}
		}

		#endregion

		#region private methods

		/// <summary>
		/// Extracts license-related values (ExpirationDate, MachineID, Signature) from an XML document.
		/// </summary>
		/// <param name="doc">The XML document containing license data.</param>
		/// <param name="expirationDate">The extracted expiration date as a string, if found.</param>
		/// <param name="machineId">The extracted machine ID as a string, if found.</param>
		/// <param name="signature">The extracted signature as a string, if found.</param>
		/// <returns>
		/// True if all three values (ExpirationDate, MachineID, Signature) are successfully extracted and are non-empty;
		/// otherwise, false.
		/// </returns>
		/// <remarks>
		/// - The method assumes the XML structure contains the elements "ExpirationDate", "MachineID", and "Signature".
		/// - If any of these elements are missing or empty, the method returns false.
		/// </remarks>
		private bool TryExtractLicenceValues(XDocument doc, out string expirationDate, out string machineId, out string signature)
		{
			expirationDate = doc.Root?.Element("ExpirationDate")?.Value;
			machineId = doc.Root?.Element("MachineID")?.Value;
			signature = doc.Root?.Element("Signature")?.Value;

			return !string.IsNullOrEmpty(expirationDate) &&
				   !string.IsNullOrEmpty(machineId) &&
				   !string.IsNullOrEmpty(signature);
		}

		/// <summary>
		/// Parses a date-time string into a <see cref="DateTime"/> object using a specific format.
		/// </summary>
		/// <param name="dateTime">The date-time string to parse, expected in the format "yyyy-MM-ddTHH:mm:ss[...]" (ISO 8601).</param>
		/// <returns>
		/// A <see cref="DateTime"/> object representing the parsed date and time in UTC format.
		/// </returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="dateTime"/> is null.</exception>
		/// <exception cref="FormatException">
		/// Thrown if the <paramref name="dateTime"/> does not match the expected format
		/// or if the string is shorter than 19 characters.
		/// </exception>
		/// <remarks>
		/// - The method extracts the first 19 characters of the provided string and appends a "Z" to ensure UTC formatting.
		/// - The format "yyyy-MM-ddTHH:mm:ssZ" is strictly enforced for parsing.
		/// - This is particularly useful for handling ISO 8601 formatted date-time strings.
		/// </remarks>
		private DateTime ParseDateTime(string dateTime)
		{
			if (dateTime == null)
			{
				throw new ArgumentNullException(nameof(dateTime), "The dateTime parameter cannot be null.");
			}

			if (dateTime.Length < 19)
			{
				throw new FormatException("The dateTime string must be at least 19 characters long.");
			}

			DateTime output = DateTime.ParseExact(
				dateTime.Substring(0, 19) + "Z",
				"yyyy-MM-ddTHH:mm:ssZ",
				CultureInfo.InvariantCulture,
				DateTimeStyles.AdjustToUniversal
			);

			return output;
		}

		/// <summary>
		/// Checks if a machine exists in the repository and retrieves its associated license if available.
		/// </summary>
		/// <param name="machineId">The ID of the machine to check.</param>
		/// <returns>
		/// The <see cref="Licence"/> associated with the machine, or <c>null</c> if no license is found.
		/// </returns>
		/// <exception cref="KeyNotFoundException">
		/// Thrown if the machine with the specified ID does not exist in the repository.
		/// </exception>
		private async Task<Licence?> ValidateMachineAndGetLicence(string machineId)
		{
			var machine = await _machineRepository.GetByIdAsync(machineId);
			if (machine is null)
				throw new KeyNotFoundException($"Machine with ID {machineId} was not found.");

			return await _licenceRepository.GetLicenceByMachineId(machineId);
		}

		/// <summary>
		/// Renews an existing license by resetting its expiration date and validity status.
		/// </summary>
		/// <param name="existingLicence">The <see cref="Licence"/> to be renewed.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		private async Task RenewExistingLicence(Licence existingLicence)
		{
			var now = DateTime.UtcNow;
			var truncatedExpirationDate = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second, DateTimeKind.Utc);

			existingLicence.ValidityStatus = "Valid";
			existingLicence.ExpirationDate = truncatedExpirationDate.AddYears(1);

			_licenceRepository.Update(existingLicence);
			await _licenceRepository.SaveChangesAsync();
		}

		/// <summary>
		/// Generates a signed license file by creating a digital signature using the license data.
		/// </summary>
		/// <param name="licence">The <see cref="Licence"/> object containing the data to be signed.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string result
		/// containing the signed license XML.
		/// </returns>
		/// <remarks>
		/// The method performs the following steps:
		/// 1. Constructs an XML document with the license's expiration date and machine ID.
		/// 2. Normalizes the XML structure for consistent hashing.
		/// 3. Computes a SHA-256 hash of the normalized XML content.
		/// 4. Generates a digital signature for the hash using the KMS service.
		/// 5. Adds the digital signature as a "Signature" element to the XML document.
		/// 6. Returns the finalized signed license as a string.
		/// </remarks>
		private async Task<string> CreateSignedFile(Licence licence)
		{
			var doc = new XDocument(
				new XElement("Licence",
					new XElement("ExpirationDate", licence.ExpirationDate.ToString("o")),
					new XElement("MachineID", licence.MachineId)
				)
			);
			var normalizedXml = _xmlService.NormalizeXml(doc);

			using var sha256 = SHA256.Create();
			var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedXml));

			var signature = await _kmsService.SignDataAsync(licence.LicenceKey, hash);
			doc.Root?.Add(new XElement("Signature", Convert.ToBase64String(signature)));

			return doc.ToString(SaveOptions.DisableFormatting);
		}

		/// <summary>
		/// crosscheck licence with the metadata stored in the database
		/// </summary>
		/// <param name="expirationDate">licence expiration date</param>
		/// <param name="machineId">licence id contained in the licence</param>
		/// <param name="metadata">metadata in the database</param>
		/// <returns>the validity of the received licence</returns>
		private bool ValidateMetadata(string expirationDate, string machineId, Licence metadata)
		{
			if (ParseDateTime(expirationDate) != metadata.ExpirationDate)
			{
				return false;
			}

			if (machineId != metadata.MachineId)
			{
				return false;
			}

			if (DateTime.TryParse(expirationDate, out var expDate) && DateTime.UtcNow > expDate)
			{
				return false;
			}

			return true;
		}
		#endregion
	}
}