using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models
{
	public class Licence
	{
		[Key]
		[Required]
		[MaxLength(255)]
		[Column("license_key")]
		public string LicenceKey { get; set; } = null!; // Primary key, non-nullable

		[Required]
		[Column("machine_id")]
		public int MachineId { get; set; } // Foreign key

		[Required]
		[MaxLength(50)]
		[Column("validity_status")]
		public string ValidityStatus { get; set; } = null!; // Non-nullable string with max length

		[Required]
		[Column("expiration_date")]
		public DateTime ExpirationDate { get; set; } // Non-nullable timestamp

		// public Machine Machine { get; set; } = null!; // Navigation property for the foreign key
	}
}
