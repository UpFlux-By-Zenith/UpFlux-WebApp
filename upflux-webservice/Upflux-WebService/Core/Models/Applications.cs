using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Upflux_WebService.Core.Models;

public class ApplicationVersion
{
    [Key] [Column("version_id")] public int VersionId { get; set; }

    [Required]
    [ForeignKey("app_id")]
    [Column("app_id")]
    public int AppId { get; set; } // Foreign key to Application

    [Required] [Column("version_name")] public string VersionName { get; set; }

    [Required] [Column("updated_by")] public string UpdatedBy { get; set; }

    [Required] [Column("date")] public DateTime Date { get; set; }

    [JsonIgnore] // Prevent serialization of the Application property
    public Application Application { get; set; } // Navigation property to Application

    public string DeviceUuid { get; set; }
}

public class ApplicationVersions
{
    [Key] [Column("version_id")] public int VersionId { get; set; }

    [Required]
    [ForeignKey("app_id")]
    [Column("app_id")]
    public int AppId { get; set; } 

    [Required] [Column("version_name")] public List<string> VersionNames { get; set; }

    [Required] [Column("updated_by")] public string UpdatedBy { get; set; }

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