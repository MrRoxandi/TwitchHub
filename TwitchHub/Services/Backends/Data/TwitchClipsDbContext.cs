using Microsoft.EntityFrameworkCore;

namespace TwitchHub.Services.Backends.Data;

public sealed class TwitchClipsDbContext(DbContextOptions<TwitchClipsDbContext> options) 
    : DbContext(options)
{
    public const string ConnectionString = "TwitchClipsDB";
    public DbSet<Entities.TwitchClipEntity> TwitchClips { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _ = modelBuilder.Entity<Entities.TwitchClipEntity>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.HasIndex(e => e.ClipId).IsUnique();
            _ = entity.Property(e => e.ClipId).IsRequired();
            _ = entity.Property(e => e.UserId).IsRequired();
            _ = entity.Property(e => e.ChannelId).IsRequired();
            _ = entity.Property(e => e.Title).IsRequired();
            _ = entity.Property(e => e.CreatedAt).IsRequired();
        });
    }
}
