using SmartApi.Models;

namespace SmartApi.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (!db.Products.Any())
        {
            db.Products.AddRange(
                new Product { Name = "Wireless Mouse", Price = 800m, Description = "Compact mouse with Bluetooth" },
                new Product { Name = "Keyboard", Price = 1200m, Description = "Mechanical keyboard" },
                new Product { Name = "USB Cable", Price = 200m, Description = "Type-C charging cable" },
                new Product { Name = "Laptop Stand", Price = 1500m, Description = "Adjustable aluminum stand" }
            );
            db.SaveChanges();
        }
    }
}
