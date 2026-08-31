using ElasticSearchDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasticSearchDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products => Set<Product>();

        public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();

        public DbSet<ProductDetails> ProductDetails => Set<ProductDetails>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");

                entity.HasKey(p => p.Id);

                entity.Property(p => p.Id)
                    .HasColumnName("Id");

                entity.Property(p => p.Name)
                    .HasColumnName("Name")
                    .IsRequired();

                entity.Property(p => p.Description)
                    .HasColumnName("Description")
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnName("Price")
                    .HasPrecision(18, 2);

                entity.Property(p => p.Category)
                    .HasColumnName("Category")
                    .IsRequired();
<<<<<<< HEAD
=======

>>>>>>> prodetails
            });

            modelBuilder.Entity<ProductVariant>(entity =>
            {
                entity.ToTable("ProductVariants");

                entity.HasKey(v => v.Id);

                entity.Property(v => v.Id)
                    .HasColumnName("Id");

<<<<<<< HEAD
=======
               

>>>>>>> prodetails
                entity.Property(v => v.Name)
                    .HasColumnName("Name")
                    .IsRequired();

                entity.Property(v => v.Value)
                    .HasColumnName("Value")
                    .IsRequired();
            });

            modelBuilder.Entity<ProductDetails>(entity =>
            {
                entity.ToTable("ProductDetails");

                entity.HasKey(d => d.ProductDetailsId);

                entity.Property(d => d.ProductDetailsId)
                    .HasColumnName("ProductDetailsId");

                entity.Property(d => d.SellerProductSku)
                    .HasColumnName("SellerProductSku")
                    .IsRequired();
            });
        }
    }
}
