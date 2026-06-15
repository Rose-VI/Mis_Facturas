using AppFactura.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

#nullable disable

namespace AppFactura.Data.Migrations;

[DbContext(typeof(InvoiceDbContext))]
partial class InvoiceDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "10.0.0");

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property<DateTime>("CreatedAt").HasColumnType("TEXT");
            entity.Property<bool>("IsFavorite").HasColumnType("INTEGER");
            entity.Property<DateTime>("IssuedAt").HasColumnType("TEXT");
            entity.Property<string>("Memorandum").HasMaxLength(2000).HasColumnType("TEXT");
            entity.Property<string>("Title").IsRequired().HasMaxLength(160).HasColumnType("TEXT");
            entity.Property<DateTime>("UpdatedAt").HasColumnType("TEXT");
            entity.HasKey("Id");
            entity.HasIndex("IsFavorite");
            entity.HasIndex("IssuedAt");
            entity.ToTable("Invoices");
        });

        modelBuilder.Entity<InvoiceEvidence>(entity =>
        {
            entity.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("INTEGER");
            entity.Property<string>("FileName").IsRequired().HasMaxLength(255).HasColumnType("TEXT");
            entity.Property<int>("InvoiceId").HasColumnType("INTEGER");
            entity.Property<string>("LocalPath").IsRequired().HasMaxLength(500).HasColumnType("TEXT");
            entity.Property<int>("SortOrder").HasColumnType("INTEGER");
            entity.HasKey("Id");
            entity.HasIndex("InvoiceId", "SortOrder");
            entity.ToTable("InvoiceEvidences");
            entity.HasOne<Invoice>("Invoice")
                .WithMany("Evidences")
                .HasForeignKey("InvoiceId")
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();
        });

        modelBuilder.Entity<Invoice>(entity => entity.Navigation("Evidences"));
    }
}
