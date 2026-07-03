using Microsoft.AspNetCore.Mvc;
using ANKAVERA_İÇ_GİYİM.Models;
using System.Text.Json;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

public class FavoriteController : Controller
{
    private const string KEY = "ANKAVERA_FAV";

    private List<FavoriteItem> GetFavs() =>
        JsonSerializer.Deserialize<List<FavoriteItem>>(
            HttpContext.Session.GetString(KEY) ?? "[]") ?? new();

    private void Save(List<FavoriteItem> favs) =>
        HttpContext.Session.SetString(KEY, JsonSerializer.Serialize(favs));

    public IActionResult Index() => View(GetFavs());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Toggle(int productId, string name,
                                decimal price, string imageUrl, string category)
    {
        var favs = GetFavs();
        var existing = favs.FirstOrDefault(f => f.ProductId == productId);
        bool added;

        if (existing != null) { favs.Remove(existing); added = false; }
        else
        {
            favs.Add(new FavoriteItem
            {
                ProductId = productId,
                Name = name,
                Price = price,
                ImageUrl = imageUrl,
                Category = category
            });
            added = true;
        }
        Save(favs);

        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return Json(new
            {
                added,
                count = favs.Count,
                message = added ? $"{name} favorilere eklendi!"
                                : "Favorilerden çıkarıldı."
            });

        TempData["SuccessMessage"] = added
            ? $"{name} favorilere eklendi!" : "Favorilerden çıkarıldı.";
        return Redirect(Request.Headers.Referer.ToString() is { Length: > 0 } r ? r : "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var favs = GetFavs();
        favs.RemoveAll(f => f.ProductId == productId);
        Save(favs);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Count() => Json(new { count = GetFavs().Count });
}