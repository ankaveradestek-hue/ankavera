using Microsoft.AspNetCore.Mvc;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

public class LegalController : Controller
{
    // /Legal            -> ilk sekme açık
    // /Legal/Index/kvkk -> KVKK sekmesi açık
    // /Legal?doc=iade   -> İade sekmesi açık
    public IActionResult Index(string? id, string? doc)
    {
        // Footer bağlantıları veya doğrudan link ile açılacak belge anahtarı
        ViewData["ActiveDoc"] = (id ?? doc ?? "mesafeli").Trim().ToLowerInvariant();
        return View();
    }
}
