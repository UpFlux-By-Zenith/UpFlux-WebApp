using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models
{
    public class Credentials
    {
        [Required]
        [Key]
        [Column("credential_id")]
        public int CredentialId { get; set; }

        [ForeignKey("user_id")]
        [Column("user_id")]
        public string UserId { get; set; }

        [ForeignKey("machine_id")]
        [Column("machine_id")]
        public string MachineId { get; set; }

        [ForeignKey("admin_id")]
        [Column("access_granted_by")]
        public string AdminId { get; set; }

        [Column("access_granted_at")]
        public DateTime AccessGrantedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }
    }
}
