using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models;

[Table("Machine_Stored_Versions")]
public class MachineStoredVersion
{
    [Key] [Column("id")] public int id { get; set; }

    [Required] [Column("machine_id")] public string MachineId { get; set; }

    [Column("version_name")] public string VersionName { get; set; }

    [Column("installed_date")] public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}