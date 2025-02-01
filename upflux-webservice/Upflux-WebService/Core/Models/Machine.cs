using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models;

public class Machine
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("machine_id")]
    public string MachineId { get; set; }

    [Required] [Column("machine_name")] public string machineName { get; set; }

    [Required] [Column("date_added")] public DateTime dateAddedOn { get; set; }

    [Required] [Column("ip_address")] public string ipAddress { get; set; }

    // Navigation property for related Applications
    public ICollection<Application> Applications { get; set; }
}