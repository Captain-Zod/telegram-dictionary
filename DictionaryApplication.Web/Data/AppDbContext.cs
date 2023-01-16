using DictionaryApplication.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace DictionaryApplication.Web.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserRequestLog>()
            .HasOne(x => x.Question)
            .WithMany(x => x.UserRequestLogs)
            .HasForeignKey(x => x.IdQuestion)
            .OnDelete(DeleteBehavior.Cascade);
    }

    public DbSet<Question> Questions { get; set; }
    public DbSet<UserRequestLog> UserRequestLogs { get; set; }
}
