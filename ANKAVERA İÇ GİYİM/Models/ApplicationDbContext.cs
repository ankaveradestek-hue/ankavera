using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ANKAVERA_İÇ_GİYİM.Models;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    // ── Tablolar ──
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Başlangıç ürünleri (Seed)
        builder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Pembe Saten Takım", Price = 849, CategoryName = "Saten", IsNew = true, Badge = "Yeni", ImageUrl = "https://images.unsplash.com/photo-1617922001439-4a2e6562f328?w=600&q=80", Description = "Pudra pembe, ipeksi saten kumaş, dantelli kenar.", IsActive = true, CreatedAt = DateTime.Now },
            new Product { Id = 2, Name = "Ekru Dantel Bralet", Price = 650, CategoryName = "Dantel", IsBestseller = true, Badge = "Çok Satan", ImageUrl = "https://images.unsplash.com/photo-1604671368394-2240d0b1bb6c?w=600&q=80", Description = "Fransız danteli, sofistike tasarım.", IsActive = true, CreatedAt = DateTime.Now },
            new Product { Id = 3, Name = "Gül Kurusu Gecelik", Price = 1190, CategoryName = "Saten", Badge = "", ImageUrl = "https://images.unsplash.com/photo-1631947430066-48c30d57b943?w=600&q=80", Description = "Uzun kesim, V yaka, ince askılı.", IsActive = true, CreatedAt = DateTime.Now },
            new Product { Id = 4, Name = "Şampanya Bridal Set", Price = 2250, CategoryName = "Bridal", IsNew = true, Badge = "Özel", ImageUrl = "https://images.unsplash.com/photo-1616627547584-bf28cee262db?w=600&q=80", Description = "Düğün gecesi 3'lü lüks set.", IsActive = true, CreatedAt = DateTime.Now },
            new Product { Id = 5, Name = "Derin Pembe Korse", Price = 975, CategoryName = "Dantel", IsBestseller = true, Badge = "Çok Satan", ImageUrl = "https://images.unsplash.com/photo-1618354691373-d851c5c3a990?w=600&q=80", Description = "Klasik korse, straplez kullanım.", IsActive = true, CreatedAt = DateTime.Now },
            new Product { Id = 6, Name = "Lila Saten Sabahlık", Price = 1450, CategoryName = "Saten", IsNew = true, Badge = "Yeni", ImageUrl = "https://images.unsplash.com/photo-1609081219090-a6d81d3085bf?w=600&q=80", Description = "Diz altı, kemer detaylı saten.", IsActive = true, CreatedAt = DateTime.Now }
        );
    }
}