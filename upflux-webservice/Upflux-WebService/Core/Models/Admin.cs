using Upflux_WebService.Core.Models.Enums;

namespace Upflux_WebService.Core.Models
{
    public class Admin : UserBase
    {
        public string HashedPassword { get; set; }
        public Admin(Guid id, string name, string email, string hashedPassword)
            : base(id, name, email, Role.Admin)
        {
            HashedPassword = hashedPassword;
        }
    }

}
