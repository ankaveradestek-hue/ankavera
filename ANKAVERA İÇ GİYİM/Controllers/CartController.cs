using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ANKAVERA_İÇ_GİYİM.Models;
using ANKAVERA_İÇ_GİYİM.Services;
using System.Text.Json;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

public class CartController : Controller
{
    private const string KEY = "ANKAVERA_CART";

    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ApplicationDbContext db,
                          IEmailService emailService,
                          UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _emailService = emailService;
        _userManager = userManager;
    }

    private List<CartItem> GetCart() =>
        JsonSerializer.Deserialize<List<CartItem>>(
            HttpContext.Session.GetString(KEY) ?? "[]") ?? new();

    private void Save(List<CartItem> cart) =>
        HttpContext.Session.SetString(KEY, JsonSerializer.Serialize(cart));

    public IActionResult Index() =>
        View(new CartViewModel { Items = GetCart() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Add(int productId, string name, decimal price,
                             string imageUrl, string category, int quantity = 1)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
            item.Quantity += quantity;
        else
            cart.Add(new CartItem
            {
                ProductId = productId,
                Name = name,
                Price = price,
                ImageUrl = imageUrl,
                Category = category,
                Quantity = quantity
            });
        Save(cart);

        // AJAX isteği
        if (Request.Headers.XRequestedWith == "XMLHttpRequest")
            return Json(new
            {
                success = true,
                message = $"{name} sepete eklendi!",
                count = cart.Sum(i => i.Quantity)
            });

        TempData["SuccessMessage"] = $"{name} sepete eklendi!";
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update(int productId, int quantity)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            if (quantity < 1) cart.Remove(item);
            else item.Quantity = quantity;
        }
        Save(cart);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int productId)
    {
        var cart = GetCart();
        cart.RemoveAll(i => i.ProductId == productId);
        Save(cart);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Clear()
    {
        Save(new List<CartItem>());
        return RedirectToAction("Index");
    }

    // ── Sipariş Formu (GET) ───────────────────────────────────────────────
    [HttpGet]
    public async Task<IActionResult> Checkout()
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["SuccessMessage"] = "Sepetiniz boş.";
            return RedirectToAction("Index");
        }

        var vm = new CheckoutViewModel { Cart = new CartViewModel { Items = cart } };

        // Giriş yapılmışsa bilgileri önceden doldur
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                vm.CustomerName = user.FullName;
                vm.CustomerEmail = user.Email ?? string.Empty;
                vm.PhoneNumber = user.PhoneNumber ?? string.Empty;
            }
        }

        return View(vm);
    }

    // ── Siparişi Tamamla (POST) ───────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            TempData["SuccessMessage"] = "Sepetiniz boş.";
            return RedirectToAction("Index");
        }

        // Doğrulama hatası olursa formu özetle birlikte geri göster
        model.Cart = new CartViewModel { Items = cart };
        if (!ModelState.IsValid)
            return View(model);

        var summary = new CartViewModel { Items = cart };

        // Takip kodu üret: ANK-YYYYMMDD-XXXXXX
        var trackingCode =
            $"ANK-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

        var order = new Order
        {
            AppUserId = _userManager.GetUserId(User), // giriş yoksa null
            OrderDate = DateTime.Now,
            OrderStatus = "Onay Bekliyor",
            TotalPrice = summary.GrandTotal,
            CustomerName = model.CustomerName,
            CustomerEmail = model.CustomerEmail,
            PhoneNumber = model.PhoneNumber,
            ShippingAddress = model.ShippingAddress,
            City = model.City,
            TrackingCode = trackingCode,
            OrderItems = cart.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList()
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        // Sipariş onay + takip kodu mailini gönder (mail hatası siparişi bozmasın)
        try
        {
            await _emailService.SendOrderConfirmationEmailAsync(
                model.CustomerEmail, model.CustomerName, order, cart);
        }
        catch
        {
            // SMTP hatası olsa bile sipariş oluştu; kullanıcıyı bilgilendireceğiz
            TempData["MailError"] = "true";
        }

        // Sepeti temizle
        Save(new List<CartItem>());

        return RedirectToAction("OrderComplete", new { code = trackingCode });
    }

    // ── Teşekkür / Sipariş Tamamlandı Sayfası ─────────────────────────────
    [HttpGet]
    public IActionResult OrderComplete(string code)
    {
        ViewData["TrackingCode"] = code;
        ViewData["MailError"] = TempData["MailError"];
        return View();
    }

    [HttpGet]
    public IActionResult Count() =>
        Json(new { count = GetCart().Sum(i => i.Quantity) });
}