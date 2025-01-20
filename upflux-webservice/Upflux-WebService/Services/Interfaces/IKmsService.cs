namespace Upflux_WebService.Services.Interfaces
{
	public interface IKmsService
	{
		Task<string> CreateKeyAsync();

		Task<byte[]> SignDataAsync(string keyId, byte[] hash);

		Task<bool> VerifySignatureAsync(string keyId, byte[] hash, byte[] signature);
	}
}
