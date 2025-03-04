using SnowSite.Server.Models;
using Microsoft.EntityFrameworkCore;
namespace SnowSite.Server.Data;
public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Dialog> Dialogs { get; set; }
    public DbSet<Message> Messages { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dialog>()
            .HasOne<User>()
            .WithMany()
            .HasForeignKey(d => d.User2Id);
    }
}