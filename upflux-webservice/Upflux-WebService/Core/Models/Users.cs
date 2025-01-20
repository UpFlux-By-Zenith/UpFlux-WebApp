using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Upflux_WebService.Core.Models.Enums;

public class Users
{
    [Key]
    [Required]
    [Column("user_id")]
    public string UserId { get; set; }

    [Required]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [Column("email")]
    public string Email { get; set; }

    [Required]
    [Column("role")]
    public UserRole Role { get; set; }

    [Column("last_login")]
    public DateTime? LastLogin { get; set; }
}