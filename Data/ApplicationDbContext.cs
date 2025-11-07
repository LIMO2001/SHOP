using Microsoft.EntityFrameworkCore;
using LaptopStore.Models;

namespace LaptopStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        
        // Your existing DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<Career> Careers { get; set; }
        
        public DbSet<MpesaPayment> MpesaPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
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
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId);
            });

            // Category configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Description).HasDefaultValue("");
                entity.Property(c => c.ImageUrl).HasDefaultValue("/images/default-category.jpg");
            });

            // Order configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId);
            });

            // OrderItem configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
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

            // BlogPost configuration
            modelBuilder.Entity<BlogPost>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(255);
                entity.Property(e => e.Author).IsRequired().HasMaxLength(100);
                entity.Property(e => e.DatePosted).IsRequired();
                entity.Property(e => e.IsPublished).IsRequired();
                
                
                entity.HasIndex(e => e.Slug).IsUnique();
            });

            // Career configuration
            modelBuilder.Entity<Career>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Location).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).IsRequired();
                entity.Property(e => e.Requirements).IsRequired();
                entity.Property(e => e.EmploymentType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SalaryRangeMin).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SalaryRangeMax).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DatePosted).IsRequired();
                entity.Property(e => e.ApplicationDeadline).IsRequired();
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ApplicationDeadline);
            });

            // MpesaPayment configuration (WITH NULLABLE STRINGS)
            modelBuilder.Entity<MpesaPayment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CheckoutRequestID).IsRequired().HasMaxLength(50);
                entity.Property(e => e.MerchantRequestID).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
                
                
                entity.Property(e => e.AccountReference).HasMaxLength(100).IsRequired(false);
                entity.Property(e => e.TransactionDescription).HasMaxLength(255).IsRequired(false);
                entity.Property(e => e.ResponseCode).HasMaxLength(10).IsRequired(false);
                entity.Property(e => e.ResponseDescription).HasMaxLength(255).IsRequired(false);
                entity.Property(e => e.CustomerMessage).HasMaxLength(500).IsRequired(false);
                entity.Property(e => e.ResultCode).HasMaxLength(10).IsRequired(false);
                entity.Property(e => e.ResultDescription).HasMaxLength(255).IsRequired(false);
                entity.Property(e => e.MpesaReceiptNumber).HasMaxLength(50).IsRequired(false);
                
                entity.Property(e => e.PaymentStatus).IsRequired().HasMaxLength(20);
                
                entity.HasIndex(e => e.CheckoutRequestID);
                entity.HasIndex(e => e.MerchantRequestID);
                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.PaymentStatus);
                
                 //entity.HasOne(e => e.Order)
                   //    .WithMany()
                    //  .HasForeignKey(e => e.OrderId)
                    //   .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}