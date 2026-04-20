using DemoApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DemoApp.Infrastructure.Data;

/// <summary>
/// EF Core database context for DemoApp persistence.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Creates a database context with configured options.
    /// </summary>
    /// <param name="options">Configured EF Core options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();

    /// <summary>
    /// Configures model mappings and constraints.
    /// </summary>
    /// <param name="modelBuilder">Model builder for EF entity configuration.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ContactSubmission>(entity =>
        {
            entity.ToTable("ContactSubmissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.Phone)
                .HasMaxLength(30);

            entity.Property(e => e.Message)
                .IsRequired()
                .HasMaxLength(2000);

            entity.Property(e => e.SubmissionToken)
                .IsRequired()
                .HasMaxLength(64);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.SubmissionToken)
                .IsUnique();
        });
    }
}
