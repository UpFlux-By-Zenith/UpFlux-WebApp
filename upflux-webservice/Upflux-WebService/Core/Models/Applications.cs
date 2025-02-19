using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Upflux_WebService.Core.Models;

/// <summary>
/// Presents the available versions inside a machine
/// </summary>
public class ApplicationVersion
{
    /// <summary>
    /// 
    /// </summary>
    [Key] [Column("version_id")] public int VersionId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [ForeignKey("machine_id")]
    [Column("machine_id")]
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("version_name")]
    public string VersionName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("date")]
    public DateTime Date { get; set; }
}

// TODO: Remove when application version is handled by control channel service
public class ApplicationVersions
{
    [Key] [Column("version_id")] public int VersionId { get; set; }

    [Required]
    [ForeignKey("machine_id")]
    [Column("machine_id")]
    public int AppId { get; set; } 

    [Required] [Column("version_name")]
    public List<string> VersionNames { get; set; }

    [Required] [Column("updated_by")] 
    public string UpdatedBy { get; set; }

    [JsonIgnore] 
    public Application Application { get; set; } 

    public string DeviceUuid { get; set; }
}

/// <summary>
/// 
/// </summary>
public class Application
{
    /// <summary>
    /// 
    /// </summary>
    [Key][Column("app_id")] public int AppId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required][Column("machine_id")] public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required][Column("app_name")] public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required][Column("added_by")] public string AddedBy { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required][Column("current_version")] public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
	[Column("updated_at")] public DateTime UpdatedAt { get; set; }
}