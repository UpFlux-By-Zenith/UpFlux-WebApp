using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Upflux_WebService.Core.Models
{
	public class Machine
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Column("machine_id")]
		public int MachineId { get; set; }

		[Required]
		[EnumDataType(typeof(MachineStatus))]
		[Column("machine_status")]
		public MachineStatus MachineStatus { get; set; }

		[Column("memory_usage")]
		public float? MemoryUsage { get; set; }

		[Required]
		[EnumDataType(typeof(ActivityStatus))]
		[Column("activity_status")]
		public ActivityStatus ActivityStatus { get; set; }
	}

	public enum MachineStatus
	{
		Alive,
		Shutdown,
		Unknown
	}

	public enum ActivityStatus
	{
		Busy,
		Idle,
		Offline
	}
}
