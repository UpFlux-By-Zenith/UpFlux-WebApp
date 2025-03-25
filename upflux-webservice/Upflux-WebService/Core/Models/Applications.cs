using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Upflux_WebService.Core.Models;

/// <summary>
/// Presents the available versions on cloud
/// </summary>
///
[Table("Application_Versions")]
public class ApplicationVersion
{
    [Required] [Column("uploaded_by")] public string UploadedBy { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Key]
    [Column("version_name")]
    public string VersionName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("date")]
    public DateTime Date { get; set; }
}

/// <summary>
/// 
/// </summary>
public class Application
{
    /// <summary>
    /// 
    /// </summary>
    [Key]
    [Column("app_id")]
    public int AppId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("machine_id")]
    public string MachineId { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("app_name")]
    public string AppName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("added_by")]
    public string AddedBy { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [Column("current_version")]
    public string CurrentVersion { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}