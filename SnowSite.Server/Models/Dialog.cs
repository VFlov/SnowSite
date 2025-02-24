using System.ComponentModel.DataAnnotations;

namespace SnowSite.Server.Models;
public class Dialog
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(200)]
    public string LastMessage { get; set; } = string.Empty;

    [MaxLength(7)]
    public string AvatarColor { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
    public int UserId { get; set; }
}