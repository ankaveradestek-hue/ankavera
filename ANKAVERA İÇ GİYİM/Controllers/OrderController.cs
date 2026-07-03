using ANKAVERA_İÇ_GİYİM.Models;
using ANKAVERA_İÇ_GİYİM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")] // Kilit zaten sende var kanka!
public class OrderController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public OrderController(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // Admin'in sipariş listesini gördüğü yer
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
        return View(orders);
    }

    // Siparişin detayına (hangi ürünler var, adresi ne?) bakacağın yer
    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();
        return View(order);
    }

    // ── Kargoya Ver & Müşteriye Bildir ────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(int id, string cargoCompany,
                                          string cargoTrackingNumber)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        if (string.IsNullOrWhiteSpace(cargoCompany) ||
            string.IsNullOrWhiteSpace(cargoTrackingNumber))
        {
            TempData["OrderError"] = "Kargo firması ve takip numarası zorunludur.";
            return RedirectToAction("Details", new { id });
        }

        order.CargoCompany = cargoCompany.Trim();
        order.CargoTrackingNumber = cargoTrackingNumber.Trim();
        order.OrderStatus = "Kargoda";
        await _context.SaveChangesAsync();

        // Müşteriye "kargon yola çıktı" maili gönder (mail hatası işlemi bozmasın)
        if (!string.IsNullOrWhiteSpace(order.CustomerEmail))
        {
            try
            {
                await _emailService.SendShippingNotificationEmailAsync(
                    order.CustomerEmail, order.CustomerName ?? "Müşterimiz", order);
                TempData["OrderSuccess"] =
                    "Sipariş kargoya verildi ve müşteriye bilgilendirme maili gönderildi.";
            }
            catch
            {
                TempData["OrderError"] =
                    "Sipariş kargoya verildi ancak bilgilendirme maili gönderilemedi.";
            }
        }
        else
        {
            TempData["OrderSuccess"] =
                "Sipariş kargoya verildi. (Müşteri e-postası bulunmadığı için mail gönderilmedi.)";
        }

        return RedirectToAction("Details", new { id });
    }

    // ── Sipariş Durumu Güncelle ───────────────────────────────────────────
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string orderStatus)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(orderStatus))
        {
            order.OrderStatus = orderStatus;
            await _context.SaveChangesAsync();
            TempData["OrderSuccess"] = $"Sipariş durumu '{orderStatus}' olarak güncellendi.";
        }

        return RedirectToAction("Details", new { id });
    }

    [HttpGet]
    [Route("Order/SahteSiparisOlustur")]
    public IActionResult SahteSiparisOlustur()
    {
        var testSiparisi = new Order
        {
            OrderDate = DateTime.Now,
            TotalPrice = 1250.50m,
            OrderStatus = "Onay Bekliyor",
            ShippingAddress = "Sakarya Üniversitesi, Bilişim Sistemleri Teknolojileri Bölümü, Serdivan / Sakarya"
            // Not: Eğer Order modelinde 'UserId' veya başka zorunlu alanlar varsa onları da buraya eklemelisin.
        };

        _context.Orders.Add(testSiparisi);
        _context.SaveChanges();

        return Content("Sahte sipariş başarıyla veritabanına düştü! Şimdi sitemizin /Order linkine gidip tabloyu kontrol edebilirsin.");
    }
}