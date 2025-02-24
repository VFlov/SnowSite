using System.ComponentModel.DataAnnotations;

namespace SnowSite.Server.Models;
public class Message
{
    public int Id { get; set; }
    public int DialogId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Sender { get; set; } = string.Empty;

    [Required]
    [MaxLength(1000)]
    public string Text { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public string? AttachmentUrl { get; set; }
}