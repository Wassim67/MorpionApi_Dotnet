using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MorpionApi_DotNet.Entities;

namespace MorpionApi_DotNet.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<GameEntity> Games => Set<GameEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<GameEntity>(game =>
        {
            game.HasKey(g => g.Id);

            game.Property(g => g.Board)
                .IsRequired()
                .HasMaxLength(9);

            game.Property(g => g.UserId)
                .IsRequired()
                .HasMaxLength(255);

            game.Property(g => g.StatusMessage)
                .IsRequired()
                .HasMaxLength(255);

            game.HasIndex(g => new { g.UserId, g.UpdatedAt });

            game.HasOne(g => g.User)
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
