using System.ComponentModel.DataAnnotations;

namespace Upflux_WebService.Core.DTOs
{
	/// <summary>
	///  Represents the request device registration
	/// </summary>
	public class RegisterMachineRequest
	{
		[Required]
		public string MachineId { get; set; }
	}

	public class LicenceResponse
	{
		[Required]
		public string MachineId { get; set; }

		[Required]
		public DateTime ExpirationDate { get; set; }
	}

	public class LicencesResponse
	{
		[Required]
		public IEnumerable<LicenceResponse> Licences { get; set; }
	}
}
