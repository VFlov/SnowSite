using SnowSite.Server.Models;
using Microsoft.EntityFrameworkCore;
namespace SnowSite.Server.Data;
public class AppDbContext : DbContext
{
    public DbSet<Message> Messages { get; set; }
    public DbSet<Dialog> Dialogs { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}