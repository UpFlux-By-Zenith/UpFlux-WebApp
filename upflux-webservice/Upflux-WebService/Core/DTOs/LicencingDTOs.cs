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
}
