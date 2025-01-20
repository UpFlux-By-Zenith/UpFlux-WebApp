using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Upflux_WebService.Core.Models
{
    public class Admin 
    {
        [Key]
        [Required]
        [Column("admin_id")]
        public string AdminId { get; set; }

        [ForeignKey(nameof(UserId))]
        [Required]
        [Column("user_id")]
        public string UserId { get; set; }

        [Required]
        [Column("password_hash")]
        public string HashedPassword { get; set; }

    }
}
