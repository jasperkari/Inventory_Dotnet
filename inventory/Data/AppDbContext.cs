using Microsoft.EntityFrameworkCore;
using inventory.Models;

namespace inventory.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Inventory> Inventory { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<Categories> Categories { get; set; }
    public DbSet<Product_Categories> Product_Categories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Inventory>()
            .Property(i => i.UsedSpace)
            .HasColumnName("used_space");

        modelBuilder.Entity<Inventory>()
            .Property(i => i.TotalSpace)
            .HasColumnName("total_space");

        modelBuilder.Entity<Products>()
            .Property(p => p.InventoryId)
            .HasColumnName("inventory_id");
        
        modelBuilder.Entity<Product_Categories>()
            .Property(p => p.ProductId)
            .HasColumnName("product_id");

        modelBuilder.Entity<Product_Categories>()
            .Property(p => p.CategoryId)
            .HasColumnName("category_id");

        modelBuilder.Entity<Product_Categories>()
            .HasKey(pc => new { pc.ProductId, pc.CategoryId });

        modelBuilder.Entity<Product_Categories>()
            .HasOne<Products>()
            .WithMany()
            .HasForeignKey(pc => pc.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product_Categories>()
            .HasOne<Categories>()
            .WithMany()
            .HasForeignKey(pc => pc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }
}