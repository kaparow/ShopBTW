using ShopBTW.Models;

namespace ShopBTW.Data;

public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        // список товаров, которые хотим гарантированно иметь
        var products = new List<Product>
        {
            // ===== РОБОТЫ =====
            new() { Name = "Rover Scout X1 (Robot)", PartType = RobotPartType.Robot, Price = 1299.00m, Stock = 3 },
            new() { Name = "Warehouse Bot W-200 (Robot)", PartType = RobotPartType.Robot, Price = 2499.00m, Stock = 2 },
            new() { Name = "Security Drone D-9 (Robot)", PartType = RobotPartType.Robot, Price = 1999.00m, Stock = 1 },
            new() { Name = "Line Follower L-3 (Robot)", PartType = RobotPartType.Robot, Price = 799.00m, Stock = 5 },
            new() { Name = "Service Robot S-10 (Robot)", PartType = RobotPartType.Robot, Price = 3299.00m, Stock = 1 },

            // ===== ДВИГАТЕЛИ =====
            new() { Name = "Robot Motor M-20", PartType = RobotPartType.Motor, Price = 89.50m, Stock = 25 },
            new() { Name = "Robot Motor M-50", PartType = RobotPartType.Motor, Price = 139.00m, Stock = 15 },
            new() { Name = "Servo Motor S-15", PartType = RobotPartType.Motor, Price = 49.90m, Stock = 40 },

            // ===== КОНТРОЛЛЕРЫ =====
            new() { Name = "Controller C-1 (Basic)", PartType = RobotPartType.Controller, Price = 59.00m, Stock = 30 },
            new() { Name = "Controller C-2 (Pro)", PartType = RobotPartType.Controller, Price = 129.00m, Stock = 18 },

            // ===== АККУМУЛЯТОРЫ =====
            new() { Name = "Battery Pack B-3000", PartType = RobotPartType.Battery, Price = 79.00m, Stock = 22 },
            new() { Name = "Battery Pack B-6000", PartType = RobotPartType.Battery, Price = 129.00m, Stock = 12 },

            // ===== КОЛЁСА / ГУСЕНИЦЫ =====
            new() { Name = "Wheel Set W-4 (Rubber)", PartType = RobotPartType.Wheel, Price = 24.90m, Stock = 50 },
            new() { Name = "Wheel Set W-6 (All-terrain)", PartType = RobotPartType.Wheel, Price = 39.90m, Stock = 35 },
            new() { Name = "Tracks T-200 (Pair)", PartType = RobotPartType.Tracks, Price = 49.00m, Stock = 20 },
            new() { Name = "Tracks T-400 (Pair)", PartType = RobotPartType.Tracks, Price = 79.00m, Stock = 14 },
        };

        // добавляем только те товары, которых ещё нет (по имени)
        foreach (var p in products)
        {
            var exists = db.Products.Any(x => x.Name == p.Name);
            if (!exists)
                db.Products.Add(p);
        }

        db.SaveChanges();
    }
}
