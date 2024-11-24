using Upflux_WebService.Core.Models.Enums;

public abstract class UserBase
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public Role UserRole { get; set; }

    /// <summary>
    /// Constructor 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="email"></param>
    /// <param name="userRole"></param>
    public UserBase(Guid id, string name, string email, Role userRole)
    {
        Id = id;
        Name = name;
        Email = email;
        UserRole = userRole;
    }
}
