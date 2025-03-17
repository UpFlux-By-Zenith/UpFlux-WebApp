using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models;

[Table("Machine_Stored_Versions")]
public class MachineStoredVersions
{
    [Key] [Column("id")] public int id { get; set; }

    [Required] [Column("user_id")] public string UserId { get; set; }

    [Required] [Column("machine_id")] public string MachineId { get; set; }

    [Required] [Column("version_name")] public string VersionName { get; set; }

    [Required] [Column("installed_date")] public DateTime InstalledAt { get; set; } = DateTime.UtcNow;
}