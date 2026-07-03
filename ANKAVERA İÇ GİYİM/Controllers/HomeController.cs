using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANKAVERA_İÇ_GİYİM.Models;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var featured = await _db.Products
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .ToListAsync();

        var vm = new HomeViewModel
        {
            FeaturedProducts = featured,
            Categories = new List<Category>
            {
                new() { Id=1, Name="Saten",  IconEmoji="🌸", Description="İpeksi yumuşaklık" },
                new() { Id=2, Name="Dantel", IconEmoji="🌺", Description="Feminen zarafet" },
                new() { Id=3, Name="Bridal", IconEmoji="💍", Description="Özel geceler" },
                new() { Id=4, Name="Setler", IconEmoji="✨", Description="Uyumlu takımlar" },
            }
        };

        return View(vm);
    }
}