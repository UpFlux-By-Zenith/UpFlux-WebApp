using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Upflux_WebService.Core.Models;

public class MachineStatus
{
	[Key]
	[Column("machine_id")]
	public string MachineId { get; set; }

	[Column("is_online")]
	public bool IsOnline { get; set; }

	[Column("last_seen")]
	public DateTime LastSeen { get; set; }
}
