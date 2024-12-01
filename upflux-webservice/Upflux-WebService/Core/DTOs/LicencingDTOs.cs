using System.ComponentModel.DataAnnotations;

namespace Upflux_WebService.Core.DTOs
{
	/// <summary>
	///  Represents the request device registration
	/// </summary>
	public class RegisterMachineRequest
	{
		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Machine ID must be a positive number.")]
		public int MachineId { get; set; }
	}
}
