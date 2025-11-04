using Microsoft.EntityFrameworkCore;
using ShowBTW.Models; // если модели в ShowBTW.Models

namespace ShowBTW.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // примеры DbSet'ов
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Order> Orders => Set<Order>();
    }
}
