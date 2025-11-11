using Microsoft.EntityFrameworkCore;
using ShopBTW.Models;

namespace ShopBTW.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Таблицы (DbSet'ы)
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(10, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(i => i.Price)
                .HasPrecision(10, 2);

            modelBuilder.Entity<CartItem>()
                .Property(i => i.UnitPrice)
                .HasPrecision(10, 2);

            // одна активная корзина на клиента (в SQL Server работает)
            modelBuilder.Entity<Cart>()
                .HasIndex(c => new { c.CustomerId, c.IsCheckedOut })
                .IsUnique()
                .HasFilter("[IsCheckedOut] = 0");

            // CartItem → Product без каскадного удаления продуктов
            modelBuilder.Entity<CartItem>()
                .HasOne(i => i.Product)
                .WithMany()
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
