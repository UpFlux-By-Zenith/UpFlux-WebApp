using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Upflux_WebService.Core.Models;

[Table("Revoked_tokens")]
public class Revokes
{
    [Key] [Column("revoke_id")] public int RevokeId { get; set; }

    [Required] [Column("user_id")] public string UserId { get; set; }

    [Required] [Column("revoked_by")] public string RevokedBy { get; set; }

    [Required] [Column("revoked_at")] public DateTime RevokedAt { get; set; } = DateTime.UtcNow;

    [Column("reason")] public string? Reason { get; set; }
}