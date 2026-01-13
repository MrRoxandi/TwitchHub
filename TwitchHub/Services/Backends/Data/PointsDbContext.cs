using Microsoft.EntityFrameworkCore;
using TwitchHub.Services.Backends.Entities;

namespace TwitchHub.Services.Backends.Data;

public sealed class PointsDbContext(DbContextOptions<PointsDbContext> options) : DbContext(options)
{
    public const string ConnectionString = "LuaPointsDB";
    public DbSet<UserPoints> UserPoints { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.Entity<UserPoints>(entity =>
        {
            _ = entity.HasKey(e => e.UserId);
            _ = entity.Property(e => e.Balance).HasDefaultValue(0);
        });
    }
}
