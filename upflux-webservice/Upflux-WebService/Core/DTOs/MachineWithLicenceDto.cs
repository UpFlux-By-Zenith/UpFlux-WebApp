namespace Upflux_WebService.Core.DTOs;

public class MachineWithLicenceDto
{
    public string MachineId { get; set; }
    public string MachineName { get; set; }
    public DateTime DateAddedOn { get; set; }
    public string IpAddress { get; set; }

    // Licence Details (Optional)
    public string? LicenceKey { get; set; }
    public string? ValidityStatus { get; set; }
    public DateTime? ExpirationDate { get; set; }
}