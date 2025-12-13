using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dominus.Domain.Entities;
namespace Dominus.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        //public DbSet<Cart> Carts { get; set; }
        //public DbSet<CartItem> CartItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        //public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<User> Users { get; set; }
        //public DbSet<Category> Categories { get; set; }
        //public DbSet<Order> Orders { get; set; }
        //public DbSet<OrderItem> OrderItems { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique email
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasPrecision(18, 2);
            });

            // Product → Category
            //modelBuilder.Entity<Product>()
            //    .HasOne(p => p.Category)
            //    .WithMany(c => c.Products)
            //    .HasForeignKey(p => p.CategoryId)
            //    .OnDelete(DeleteBehavior.Restrict);

            // User role as string
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();

            //// Cart → CartItem
            //modelBuilder.Entity<Cart>()
            //    .HasMany(c => c.Items)
            //    .WithOne(i => i.Cart)
            //    .HasForeignKey(i => i.CartId);

            //modelBuilder.Entity<CartItem>()
            //    .HasOne(ci => ci.Product)
            //    .WithMany()
            //    .HasForeignKey(ci => ci.ProductId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Order>()
            //    .HasOne(o => o.User)
            //    .WithMany(u => u.Orders)
            //    .HasForeignKey(o => o.UserId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //// Order → OrderItem
            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(oi => oi.Order)
            //    .WithMany(o => o.Items)
            //    .HasForeignKey(oi => oi.OrderId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //// OrderItem → Product
            //modelBuilder.Entity<OrderItem>()
            //    .HasOne(oi => oi.Product)
            //    .WithMany()
            //    .HasForeignKey(oi => oi.ProductId)
            //    .OnDelete(DeleteBehavior.Restrict);
            //// Wishlist → User
            //modelBuilder.Entity<Wishlist>()
            //    .HasOne(w => w.User)
            //    .WithMany()
            //    .HasForeignKey(w => w.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //// Wishlist → Product
            //modelBuilder.Entity<Wishlist>()
            //    .HasOne(w => w.Product)
            //    .WithMany()
            //    .HasForeignKey(w => w.ProductId)
            //    .OnDelete(DeleteBehavior.Restrict);


            //modelBuilder.Entity<Order>()
            //    .Property(o => o.PaymentStatus)
            //    .HasConversion<string>();

            //modelBuilder.Entity<Order>()
            //    .Property(o => o.PaymentMethod)
            //    .HasConversion<string>();

            //modelBuilder.Entity<Order>()
            //    .Property(o => o.OrderStatus)
            //    .HasConversion<string>();

        }
    }
}
