namespace Upflux_WebService.Core.Objects
{
    public class User
    {
        // Unique identifier for the user
        public Guid Id { get; set; }

        // Name of the user
        public string Name { get; set; }

        // Role of the user (Admin, Engineer, Developer)
        public string Role { get; set; }

        // Email of the user
        public string Email { get; set; }

        // Hashed password (null if not admin)
        public string? HashedPassword { get; set; }

        // List of machine IDs the user has access to
        public List<string> MachineIds { get; set; } = new List<string>();

        // Constructor
        public User()
        {
            // Default constructor for initialization
        }

        // Parameterized constructor
        public User(Guid id, string name, string role, string email, string? hashedPassword, List<string> machineIds)
        {
            Id = id;
            Name = name;
            Role = role;
            Email = email;
            HashedPassword = hashedPassword;
            MachineIds = machineIds;
        }
    }
}
