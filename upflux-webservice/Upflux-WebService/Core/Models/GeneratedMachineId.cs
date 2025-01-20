using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class GeneratedMachineId
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int Id { get; set; }

	[Required]
	[MaxLength(36)]
	[Column("machine_id")]
	public string MachineId { get; set; }

	[Required]
	[Column("created_at")]
	public DateTime CreatedAt { get; set; }
}