namespace SnowSite.Server.Models;
public class Dialog
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string LastMessage { get; set; } = string.Empty;
    public string AvatarColor { get; set; } = string.Empty;
    public int UnreadCount { get; set; }
}