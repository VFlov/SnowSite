using System.ComponentModel.DataAnnotations;

namespace SnowSite.Server.Models;
public class Dialog
{
    public int Id { get; set; }

    [Required]
    public int User1Id { get; set; }  
    public User User1 { get; set; }

    [Required]
    public int User2Id { get; set; }  
    public User User2 { get; set; }

    [MaxLength(200)]
    public string LastMessage { get; set; } = string.Empty;

    public int User1UnreadCount { get; set; }
    public int User2UnreadCount { get; set; }
}