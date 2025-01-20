﻿using Amazon.KeyManagementService.Model;
using Amazon.KeyManagementService;
using Upflux_WebService.Services.Interfaces;

namespace Upflux_WebService.Services
{
	/// <summary>
	/// Service that deals with Amazon KMS
	/// </summary>
	public class KmsService : IKmsService
	{
		private readonly AmazonKeyManagementServiceClient _kmsClient;

		/// <summary>
		/// Constructor
		/// </summary>
		public KmsService()
		{
			_kmsClient = new AmazonKeyManagementServiceClient(Amazon.RegionEndpoint.EUNorth1); 
		}

		/// <summary>
		/// Creates a new cryptographic key in the Key Management Service (KMS) for signing and verifying operations.
		/// </summary>
		/// <returns>
		/// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a string result
		/// containing the unique identifier of the created key.
		/// </returns>
		/// <remarks>
		/// This method performs the following steps:
		/// 1. Constructs a request to create a new key in KMS with the following attributes:
		/// 2. Sends the request to the KMS client to create the key.
		/// 3. Retrieves the unique key identifier from the response.
		/// </remarks>
		public async Task<string> CreateKeyAsync()
		{
			var request = new CreateKeyRequest
			{
				Description = "Key for signing and verifying",
				KeyUsage = KeyUsageType.SIGN_VERIFY,
				KeySpec = KeySpec.RSA_2048
			};
			var response = await _kmsClient.CreateKeyAsync(request);
			return response.KeyMetadata.KeyId;
		}

		/// <summary>
		/// Signs a hash using the specified cryptographic key in the Key Management Service (KMS).
		/// </summary>
		/// <param name="keyId">The unique identifier of the key to use for signing.</param>
		/// <param name="hash">The hash of the message to be signed, represented as a byte array.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a byte array result
		/// containing the digital signature generated by KMS.
		/// </returns>
		/// <remarks>
		/// This method performs the following steps:
		/// 1. Constructs a request to KMS with the provided key ID and hash.
		/// 2. Sends the request to the KMS client to generate the digital signature.
		/// 3. Extracts and returns the signature as a byte array.
		/// </remarks>
		public async Task<byte[]> SignDataAsync(string keyId, byte[] hash)
		{
			var request = new SignRequest
			{
				KeyId = keyId,
				Message = new MemoryStream(hash),
				MessageType = MessageType.DIGEST,
				SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256
			};
			var response = await _kmsClient.SignAsync(request);
			return response.Signature.ToArray();
		}

		/// <summary>
		/// Verifies a digital signature using the specified cryptographic key in the Key Management Service (KMS).
		/// </summary>
		/// <param name="keyId">The unique identifier of the key to use for verification.</param>
		/// <param name="hash">The hash of the message that was signed, represented as a byte array.</param>
		/// <param name="signature">The digital signature to verify, represented as a byte array.</param>
		/// <returns>
		/// A <see cref="Task{TResult}"/> representing the asynchronous operation, with a boolean result:
		/// <c>true</c> if the signature is valid; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// This method performs the following steps:
		/// 1. Constructs a verification request to KMS with the provided key ID, message hash, and signature.
		/// 2. Sends the verification request to the KMS client.
		/// 3. Returns <c>true</c> if the signature is valid, or <c>false</c> if the signature is invalid or an error occurs.
		/// </remarks>
		public async Task<bool> VerifySignatureAsync(string keyId, byte[] hash, byte[] signature)
		{
			try
			{
				var request = new VerifyRequest
				{
					KeyId = keyId,
					Message = new MemoryStream(hash),
					MessageType = MessageType.DIGEST,
					Signature = new MemoryStream(signature),
					SigningAlgorithm = SigningAlgorithmSpec.RSASSA_PSS_SHA_256
				};
				var response = await _kmsClient.VerifyAsync(request);
				return response.SignatureValid;
			}
			catch
			{
				return false;
			}
		}
	}
}
