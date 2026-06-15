using AppFactura.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace AppFactura.Data;

public sealed class InvoiceDbContext(DbContextOptions<InvoiceDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceEvidence> InvoiceEvidences => Set<InvoiceEvidence>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property(x => x.Title).IsRequired().HasMaxLength(160);
            entity.Property(x => x.Memorandum).HasMaxLength(2000);
            entity.HasIndex(x => x.IssuedAt);
            entity.HasIndex(x => x.IsFavorite);
            entity.HasMany(x => x.Evidences)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InvoiceEvidence>(entity =>
        {
            entity.Property(x => x.LocalPath).IsRequired().HasMaxLength(500);
            entity.Property(x => x.FileName).IsRequired().HasMaxLength(255);
            entity.HasIndex(x => new { x.InvoiceId, x.SortOrder });
        });
    }
}
