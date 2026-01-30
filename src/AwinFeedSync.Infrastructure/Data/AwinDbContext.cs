using AwinFeedSync.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AwinFeedSync.Infrastructure.Data;

public class AwinDbContext : DbContext
{
    public AwinDbContext(DbContextOptions<AwinDbContext> options) : base(options) { }
    
    public DbSet<Advertiser> Advertisers => Set<Advertiser>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<SyncRun> SyncRuns => Set<SyncRun>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Advertiser>(entity =>
        {
            entity.ToTable("advertisers");
            entity.HasKey(e => e.AdvertiserId);
            entity.Property(e => e.AdvertiserId).HasColumnName("advertiser_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.DefaultCommissionText).HasColumnName("default_commission_text");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });
        
        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AdvertiserId).HasColumnName("advertiser_id");
            entity.Property(e => e.ProductKey).HasColumnName("product_key").IsRequired();
            entity.Property(e => e.FeedProductId).HasColumnName("feed_product_id");
            entity.Property(e => e.Sku).HasColumnName("sku");
            entity.Property(e => e.ProductName).HasColumnName("product_name");
            entity.Property(e => e.ProductUrl).HasColumnName("product_url");
            entity.Property(e => e.ImageUrl).HasColumnName("image_url");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("numeric");
            entity.Property(e => e.Currency).HasColumnName("currency");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Subcategory).HasColumnName("subcategory");
            entity.Property(e => e.CommissionText).HasColumnName("commission_text");
            entity.Property(e => e.CommissionRate).HasColumnName("commission_rate").HasColumnType("numeric");
            entity.Property(e => e.TrackingUrl).HasColumnName("tracking_url");
            entity.Property(e => e.TrackingUrlSource).HasColumnName("tracking_url_source");
            entity.Property(e => e.Extra).HasColumnName("extra").HasColumnType("jsonb");
            entity.Property(e => e.ContentHash).HasColumnName("content_hash").IsRequired();
            entity.Property(e => e.LastSeenAt).HasColumnName("last_seen_at");
            entity.Property(e => e.LastChangedAt).HasColumnName("last_changed_at");
            entity.Property(e => e.LastUpdatedAt).HasColumnName("last_updated_at");
            entity.Property(e => e.InactiveAt).HasColumnName("inactive_at");
            entity.Property(e => e.AiSummary).HasColumnName("ai_summary");
            entity.Property(e => e.AiSummaryStatus).HasColumnName("ai_summary_status");
            entity.Property(e => e.AiSummaryUpdatedAt).HasColumnName("ai_summary_updated_at");
            
            entity.HasIndex(e => new { e.AdvertiserId, e.ProductKey }).IsUnique();
            entity.HasIndex(e => e.LastChangedAt);
            entity.HasIndex(e => e.LastSeenAt);
            entity.HasIndex(e => e.AdvertiserId);
            
            entity.HasOne(e => e.Advertiser)
                .WithMany()
                .HasForeignKey(e => e.AdvertiserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<SyncRun>(entity =>
        {
            entity.ToTable("sync_runs");
            entity.HasKey(e => e.RunId);
            entity.Property(e => e.RunId).HasColumnName("run_id");
            entity.Property(e => e.StartedAt).HasColumnName("started_at");
            entity.Property(e => e.FinishedAt).HasColumnName("finished_at");
            entity.Property(e => e.Status).HasColumnName("status").IsRequired();
            entity.Property(e => e.ErrorText).HasColumnName("error_text");
            entity.Property(e => e.AdvertisersProcessed).HasColumnName("advertisers_processed");
            entity.Property(e => e.ProductsSeen).HasColumnName("products_seen");
            entity.Property(e => e.ProductsChanged).HasColumnName("products_changed");
        });
    }
}
