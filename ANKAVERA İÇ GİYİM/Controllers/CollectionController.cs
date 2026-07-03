using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANKAVERA_İÇ_GİYİM.Models;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

public class CollectionsController : Controller
{
    private readonly ApplicationDbContext _db;

    public CollectionsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? category = null)
    {
        var query = _db.Products
            .Where(p => p.IsActive)
            .AsQueryable();

        // ✅ DÜZELTME: OrdinalIgnoreCase yerine EF Core uyumlu karşılaştırma
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.CategoryName == category);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewData["SelectedCategory"] = category ?? "Tümü";
        ViewData["AllCategories"] = await _db.Products
            .Where(p => p.IsActive)
            .Select(p => p.CategoryName)
            .Distinct()
            .ToListAsync();

        return View(products);
    }
}