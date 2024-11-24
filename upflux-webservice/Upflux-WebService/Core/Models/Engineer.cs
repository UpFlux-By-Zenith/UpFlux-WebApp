using Upflux_WebService.Core.Models.Enums;

public class Engineer : UserBase
{
    public List<string> MachineIds { get; set; } = new List<string>();

    public Engineer(Guid id, string name, string email, List<string> machineIds)
        : base(id, name, email, Role.Engineer)
    {
        MachineIds = machineIds;
    }
}