using Microsoft.EntityFrameworkCore;
using LaptopStore.Models;

namespace LaptopStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Role).HasDefaultValue("Customer");
            });

            // Product configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId);
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.HasOne(oi => oi.Order)
                      .WithMany(o => o.OrderItems)
                      .HasForeignKey(oi => oi.OrderId);
                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId);
            });

            // CartItem configuration
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasKey(ci => ci.Id);
                entity.HasOne(ci => ci.User)
                      .WithMany(u => u.CartItems)
                      .HasForeignKey(ci => ci.UserId);
                entity.HasOne(ci => ci.Product)
                      .WithMany()
                      .HasForeignKey(ci => ci.ProductId);
            });
        }
    }
}