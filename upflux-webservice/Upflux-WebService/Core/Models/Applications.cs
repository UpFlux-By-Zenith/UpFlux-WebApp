using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Upflux_WebService.Core.Models
{
    public class ApplicationVersion
    {
        [Key]
        [Column("version_id")]
        public int VersionId { get; set; }

        [Required]
        [ForeignKey("app_id")]
        [Column("app_id")]
        public int AppId { get; set; } // Foreign key to Application

        [Required]
        [Column("version_name")]
        public string VersionName { get; set; }

        [Required]
        [Column("updated_by")]
        public string UpdatedBy { get; set; }

        [Required]
        [Column("date")]
        public DateTime Date { get; set; }

        [JsonIgnore] // Prevent serialization of the Application property
        public Application Application { get; set; } // Navigation property to Application
    }

    public class Application
    {
        [Key]
        [Column("app_id")]
        public int AppId { get; set; }

        [Required]
        [Column("machine_id")]
        public string MachineId { get; set; }

        [Required]
        [Column("app_name")]
        public string AppName { get; set; }

        [Required]
        [Column("added_by")]
        public string AddedBy { get; set; }

        [Required]
        [Column("current_version")]
        public string CurrentVersion { get; set; }

        // Navigation property for related versions
        public ICollection<ApplicationVersion> Versions { get; set; }
    }
}
