using Microsoft.EntityFrameworkCore;
using SortingProduct.Domain.Entities;

namespace SortingProduct.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<ProductBatch> ProductBatches => Set<ProductBatch>();
    public DbSet<ProductGroup> ProductGroups => Set<ProductGroup>();
    public DbSet<ProductGroupItem> ProductGroupItems => Set<ProductGroupItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductBatch>(b =>
        {
            b.ToTable("product_batches");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(300).IsRequired();
            b.Property(x => x.Unit).HasColumnName("unit").HasMaxLength(50).IsRequired();
            b.Property(x => x.UnitPriceEur).HasColumnName("unit_price_eur").HasColumnType("numeric(18,2)");
            b.Property(x => x.InitialQuantity).HasColumnName("initial_quantity");
            b.Property(x => x.RemainingQuantity).HasColumnName("remaining_quantity");
            b.Property(x => x.Status).HasColumnName("status");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() at time zone 'utc'");

            b.HasIndex(x => x.Status).HasDatabaseName("ix_product_batches_status");
        });

        modelBuilder.Entity<ProductGroup>(b =>
        {
            b.ToTable("product_groups");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
            b.Property(x => x.TotalPriceEur).HasColumnName("total_price_eur").HasColumnType("numeric(18,2)");
            b.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now() at time zone 'utc'");

            b.HasMany(x => x.Items)
                .WithOne(x => x.ProductGroup!)
                .HasForeignKey(x => x.ProductGroupId);

            b.HasIndex(x => x.CreatedAt).HasDatabaseName("ix_product_groups_created_at");
        });

        modelBuilder.Entity<ProductGroupItem>(b =>
        {
            b.ToTable("product_group_items");
            b.HasKey(x => x.Id);

            b.Property(x => x.Id).HasColumnName("id");
            b.Property(x => x.ProductGroupId).HasColumnName("product_group_id");
            b.Property(x => x.ProductBatchId).HasColumnName("product_batch_id");
            b.Property(x => x.Quantity).HasColumnName("quantity");
            b.Property(x => x.LineTotalEur).HasColumnName("line_total_eur").HasColumnType("numeric(18,2)");

            b.HasOne(x => x.ProductBatch)
                .WithMany()
                .HasForeignKey(x => x.ProductBatchId);

            b.HasIndex(x => new { x.ProductGroupId, x.ProductBatchId }).HasDatabaseName("ix_group_items_group_batch");
        });

        base.OnModelCreating(modelBuilder);
    }
}
