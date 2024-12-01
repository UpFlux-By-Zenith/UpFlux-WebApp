using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models
{
	public class Licence
	{
		[Key]
		[Required]
		[MaxLength(255)]
		[Column("licence_key")]
		public string LicenceKey { get; set; } = null!; 

		[Required]
		[Column("machine_id")]
		public string MachineId { get; set; }

		[Required]
		[MaxLength(50)]
		[Column("validity_status")]
		public string ValidityStatus { get; set; } = null!; 

		[Required]
		[Column("expiration_date")]
		public DateTime ExpirationDate { get; set; } 
	}
}
