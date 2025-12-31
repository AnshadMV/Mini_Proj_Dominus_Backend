
using Dominus.Domain.Entities;
using Dominus.Domain.Entities.Dominus.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dominus.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        //session with the database 
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<User> Users { get; set; }

        public DbSet<Product> Products { get; set; }
        public DbSet<Color> Colors => Set<Color>();
        public DbSet<ProductColors> ProductColors => Set<ProductColors>();
        //public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<Category> Categories => Set<Category>();



        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();


        public DbSet<Wishlist> Wishlists => Set<Wishlist>();
        public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<ProductImage> ProductImages => Set<ProductImage>();
        public DbSet<ShippingAddress> ShippingAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasConversion<string>();



            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasPrecision(18, 2);
            });



            modelBuilder.Entity<ProductColors>()
                .HasOne(pc => pc.Product)
                .WithMany(p => p.AvailableColors)
                .HasForeignKey(pc => pc.ProductId);

            modelBuilder.Entity<ProductColors>()
                .HasOne(pc => pc.Color)
                .WithMany(c => c.ProductColors)
                .HasForeignKey(pc => pc.ColorId);

            // Product <-> ProductImage
            //modelBuilder.Entity<ProductImage>()
            //    .HasOne(pi => pi.Product)
            //    .WithMany(p => p.Images)
            //    .HasForeignKey(pi => pi.ProductId);

            // RefreshToken <-> User
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId);


            // Product <-> Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
    .HasIndex(p => new { p.IsActive, p.IsDeleted });

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.CategoryId);

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Price);


            modelBuilder.Entity<Cart>()
    .HasMany(c => c.Items)
    .WithOne(i => i.Cart)
    .HasForeignKey(i => i.CartId);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany()
                .HasForeignKey(ci => ci.ProductId);



            modelBuilder.Entity<Wishlist>()
    .HasMany(w => w.Items)
    .WithOne(i => i.Wishlist)
    .HasForeignKey(i => i.WishlistId);

            modelBuilder.Entity<WishlistItem>()
                .HasOne(wi => wi.Product)
                .WithMany()
                .HasForeignKey(wi => wi.ProductId);

            modelBuilder.Entity<Order>()
     .HasMany(o => o.Items)
     .WithOne(i => i.Order)
     .HasForeignKey(i => i.OrderId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId);

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalAmount)
                      .HasPrecision(18, 2);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.Property(oi => oi.Price)
                      .HasPrecision(18, 2);
            });
            modelBuilder.Entity<CartItem>()
    .HasQueryFilter(ci => !ci.IsDeleted);

            modelBuilder.Entity<Cart>()
                .HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Wishlist>()
    .HasQueryFilter(w => !w.IsDeleted);

            modelBuilder.Entity<WishlistItem>()
                .HasQueryFilter(wi => !wi.IsDeleted);
            //        modelBuilder.Entity<Order>()
            //.Property(o => o.Status)
            //.HasConversion<string>()   
            //.HasMaxLength(30);          

            modelBuilder.Entity<OrderItem>()
    .HasOne(oi => oi.Color)
    .WithMany()
    .HasForeignKey(oi => oi.ColorId)
    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductImage>()
    .HasOne(pi => pi.Product)
    .WithMany(p => p.Images)
    .HasForeignKey(pi => pi.ProductId)
    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShippingAddress>()
       .HasOne(sa => sa.User)
       .WithMany(u => u.ShippingAddresses)
       .HasForeignKey(sa => sa.UserId)
       .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ShippingAddress>()
                .HasQueryFilter(sa => !sa.IsDeleted);

            modelBuilder.Entity<ShippingAddress>()
    .HasIndex(sa => new { sa.UserId, sa.IsActive });









        }

        public override async Task<int> SaveChangesAsync(
    CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<BaseEntity>();

            foreach (var entry in entries)
            {
                // 🟢 CREATE
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedOn = DateTime.UtcNow;
                    entry.Entity.CreatedBy ??= "system";
                }

                // 🔴 SOFT DELETE (IsDeleted changed)
                else if (entry.State == EntityState.Modified &&
                         entry.OriginalValues.GetValue<bool>(nameof(BaseEntity.IsDeleted)) == false &&
                         entry.CurrentValues.GetValue<bool>(nameof(BaseEntity.IsDeleted)) == true)
                {
                    entry.Entity.DeletedOn = DateTime.UtcNow;
                    entry.Entity.DeletedBy ??= "system";

                    // ❗ DO NOT update ModifiedOn for deletes
                    entry.Entity.ModifiedOn = null;
                }

                // 🟡 NORMAL UPDATE
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedOn = DateTime.UtcNow;
                    entry.Entity.ModifiedBy ??= "system";
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }


    }
}