using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ANKAVERA_İÇ_GİYİM.Models;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    private static readonly List<string> Categories = new()
    {
        "Saten", "Dantel", "Bridal", "Setler", "Korse", "Gecelik"
    };

    public AdminController(
        UserManager<ApplicationUser> um,
        ApplicationDbContext db,
        IWebHostEnvironment env)
    {
        _um = um;
        _db = db;
        _env = env;
    }

    // ════════════════════════════════════════════════════════
    // DASHBOARD
    // ════════════════════════════════════════════════════════
    public async Task<IActionResult> Index()
    {
        var totalUsers = await _um.Users.CountAsync();
        var admins = (await _um.GetUsersInRoleAsync("Admin")).Count;
        var members = (await _um.GetUsersInRoleAsync("User")).Count;
        var totalProducts = await _db.Products.CountAsync();
        var activeProducts = await _db.Products.CountAsync(p => p.IsActive);

        ViewData["TotalUsers"] = totalUsers;
        ViewData["Admins"] = admins;
        ViewData["Members"] = members;
        ViewData["TotalProducts"] = totalProducts;
        ViewData["ActiveProducts"] = activeProducts;

        var recentUsers = await _um.Users
            .OrderByDescending(u => u.RegisteredAt)
            .Take(5).ToListAsync();

        return View(recentUsers);
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN LİSTESİ
    // ════════════════════════════════════════════════════════
    public async Task<IActionResult> Products(string? search, string? category)
    {
        var query = _db.Products.AsQueryable();

        // ✅ Contains büyük/küçük harf SQL'de zaten case-insensitive çalışır
        if (!string.IsNullOrEmpty(search))
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.CategoryName.Contains(search));

        // ✅ Direkt == kullan
        if (!string.IsNullOrEmpty(category))
            query = query.Where(p => p.CategoryName == category);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        ViewData["Search"] = search;
        ViewData["Category"] = category;
        ViewData["Categories"] = Categories;

        return View(products);
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN EKLE — GET
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult AddProduct()
    {
        ViewData["Categories"] = Categories;
        return View(new ProductViewModel { IsActive = true });
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN EKLE — POST
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(ProductViewModel vm)
    {
        ViewData["Categories"] = Categories;

        if (!ModelState.IsValid)
            return View(vm);

        string imageUrl = vm.ImageUrl; // Önce URL'yi al

        // Eğer dosya yüklendiyse URL'ye göre öncelik ver
        if (vm.ImageFile != null && vm.ImageFile.Length > 0)
        {
            var uploaded = await SaveImageAsync(vm.ImageFile);
            if (uploaded == null)
            {
                ModelState.AddModelError("ImageFile",
                    "Resim yüklenirken hata oluştu. Lütfen tekrar deneyin.");
                return View(vm);
            }
            imageUrl = uploaded;
        }

        // İkisi de boşsa uyar
        if (string.IsNullOrEmpty(imageUrl))
        {
            ModelState.AddModelError("ImageUrl",
                "Lütfen bir görsel yükleyin veya URL girin.");
            return View(vm);
        }

        var product = new Product
        {
            Name = vm.Name.Trim(),
            Description = vm.Description.Trim(),
            Price = vm.Price,
            CategoryName = vm.CategoryName,
            ImageUrl = imageUrl,
            IsNew = vm.IsNew,
            IsBestseller = vm.IsBestseller,
            IsActive = vm.IsActive,
            Badge = vm.Badge.Trim(),
            StockCount = vm.StockCount,
            CreatedAt = DateTime.Now
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"✓ \"{product.Name}\" ürünü başarıyla eklendi!";
        return RedirectToAction(nameof(Products));
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN DÜZENLE — GET
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> EditProduct(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p == null) return NotFound();

        ViewData["Categories"] = Categories;

        var vm = new ProductViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryName = p.CategoryName,
            IsNew = p.IsNew,
            IsBestseller = p.IsBestseller,
            IsActive = p.IsActive,
            Badge = p.Badge,
            StockCount = p.StockCount,
            ExistingImageUrl = p.ImageUrl,
            ImageUrl = p.ImageUrl
        };

        return View(vm);
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN DÜZENLE — POST
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProduct(int id, ProductViewModel vm)
    {
        ViewData["Categories"] = Categories;

        if (!ModelState.IsValid)
            return View(vm);

        var product = await _db.Products.FindAsync(id);
        if (product == null) return NotFound();

        string imageUrl = vm.ExistingImageUrl; // Mevcut resmi koru

        // Yeni dosya yüklendiyse güncelle
        if (vm.ImageFile != null && vm.ImageFile.Length > 0)
        {
            var uploaded = await SaveImageAsync(vm.ImageFile);
            if (uploaded != null) imageUrl = uploaded;
        }
        else if (!string.IsNullOrEmpty(vm.ImageUrl) &&
                 vm.ImageUrl != vm.ExistingImageUrl)
        {
            // Yeni URL girilmişse güncelle
            imageUrl = vm.ImageUrl;
        }

        product.Name = vm.Name.Trim();
        product.Description = vm.Description.Trim();
        product.Price = vm.Price;
        product.CategoryName = vm.CategoryName;
        product.ImageUrl = imageUrl;
        product.IsNew = vm.IsNew;
        product.IsBestseller = vm.IsBestseller;
        product.IsActive = vm.IsActive;
        product.Badge = vm.Badge.Trim();
        product.StockCount = vm.StockCount;

        await _db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"✓ \"{product.Name}\" güncellendi!";
        return RedirectToAction(nameof(Products));
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN SİL
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p != null)
        {
            _db.Products.Remove(p);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"✓ \"{p.Name}\" silindi.";
        }
        return RedirectToAction(nameof(Products));
    }

    // ════════════════════════════════════════════════════════
    // ÜRÜN AKTİF/PASİF TOGGLE
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleProduct(int id)
    {
        var p = await _db.Products.FindAsync(id);
        if (p != null)
        {
            p.IsActive = !p.IsActive;
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = p.IsActive
                ? $"✓ \"{p.Name}\" aktif edildi."
                : $"✓ \"{p.Name}\" pasif yapıldı.";
        }
        return RedirectToAction(nameof(Products));
    }

    // ════════════════════════════════════════════════════════
    // KULLANICI YÖNETİMİ
    // ════════════════════════════════════════════════════════
    public async Task<IActionResult> Users()
    {
        var users = await _um.Users
            .OrderByDescending(u => u.RegisteredAt)
            .ToListAsync();

        var list = new List<(ApplicationUser User, IList<string> Roles)>();
        foreach (var u in users)
            list.Add((u, await _um.GetRolesAsync(u)));

        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var me = await _um.GetUserAsync(User);
        if (me?.Id == id)
        {
            TempData["ErrorMessage"] = "Kendi hesabınızı silemezsiniz.";
            return RedirectToAction(nameof(Users));
        }
        var u = await _um.FindByIdAsync(id);
        if (u != null)
        {
            await _um.DeleteAsync(u);
            TempData["SuccessMessage"] = "Kullanıcı silindi.";
        }
        return RedirectToAction(nameof(Users));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleRole(string id)
    {
        var u = await _um.FindByIdAsync(id);
        if (u == null) return RedirectToAction(nameof(Users));

        if (await _um.IsInRoleAsync(u, "Admin"))
        {
            await _um.RemoveFromRoleAsync(u, "Admin");
            await _um.AddToRoleAsync(u, "User");
            TempData["SuccessMessage"] = $"{u.FullName} → Normal Üye yapıldı.";
        }
        else
        {
            await _um.RemoveFromRoleAsync(u, "User");
            await _um.AddToRoleAsync(u, "Admin");
            TempData["SuccessMessage"] = $"{u.FullName} → Admin yapıldı.";
        }
        return RedirectToAction(nameof(Users));
    }

    // ════════════════════════════════════════════════════════
    // YARDIMCI: Resim Kaydet
    // ════════════════════════════════════════════════════════
    private async Task<string?> SaveImageAsync(IFormFile file)
    {
        try
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext)) return null;

            var folder = Path.Combine(_env.WebRootPath, "images", "products");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/images/products/{fileName}";
        }
        catch { return null; }
    }
}