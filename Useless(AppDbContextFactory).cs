using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ShopBTW.Data
{
    // Используется ТОЛЬКО инструментами EF при миграциях
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=ShopBTW;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True")
                .Options;

            return new AppDbContext(options);
        }
    }
}
