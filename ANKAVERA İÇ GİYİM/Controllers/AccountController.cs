using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using ANKAVERA_İÇ_GİYİM.Models;
using ANKAVERA_İÇ_GİYİM.Services;

namespace ANKAVERA_İÇ_GİYİM.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _um;
    private readonly SignInManager<ApplicationUser> _sm;
    private readonly IEmailService _email;

    public AccountController(
        UserManager<ApplicationUser> um,
        SignInManager<ApplicationUser> sm,
        IEmailService email)
    {
        _um = um;
        _sm = sm;
        _email = email;
    }

    // ════════════════════════════════════════════════════════
    // REGISTER GET
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Register()
    {
        if (_sm.IsSignedIn(User))
            return RedirectToAction("Index", "Home");
        return View(new RegisterViewModel());
    }

    // ════════════════════════════════════════════════════════
    // REGISTER POST
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel m)
    {
        if (!ModelState.IsValid) return View(m);

        if (await _um.FindByEmailAsync(m.Email) != null)
        {
            ModelState.AddModelError("Email", "Bu e-posta zaten kayıtlı.");
            return View(m);
        }

        var user = new ApplicationUser
        {
            UserName = m.Email,
            Email = m.Email,
            FullName = m.FullName,
            EmailConfirmed = false,   // ← Onay bekleniyor
            RegisteredAt = DateTime.Now,
            AcceptsMarketing = m.AcceptMarketing   // ← Ticari ileti açık rızası
        };

        var result = await _um.CreateAsync(user, m.Password);

        if (result.Succeeded)
        {
            await _um.AddToRoleAsync(user, "User");

            // ── Onay Token Oluştur ──
            var token = await _um.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

            // ── Onay Linki ──
            var confirmLink = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new { userId = user.Id, token = encodedToken },
                protocol: Request.Scheme)!;

            // ── Mail Gönder ──
            try
            {
                await _email.SendConfirmationEmailAsync(
                    user.Email!, user.FullName, confirmLink);
            }
            catch
            {
                // Mail gönderilemese bile kayıt tamamlansın
                // Loglama eklenebilir
            }

            return RedirectToAction(nameof(RegisterConfirmation),
                new { email = user.Email });
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty,
                TurkishError(e.Code, e.Description));

        return View(m);
    }

    // ════════════════════════════════════════════════════════
    // KAYIT ONAY BEKLEMESİ SAYFASI
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult RegisterConfirmation(string email)
    {
        ViewData["Email"] = email;
        return View();
    }

    // ════════════════════════════════════════════════════════
    // E-POSTA ONAY (Link Tıklandığında)
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return RedirectToAction("Index", "Home");

        var user = await _um.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Index", "Home");

        // Token'ı decode et
        var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(token));

        var result = await _um.ConfirmEmailAsync(user, decodedToken);

        if (result.Succeeded)
        {
            // Hoş geldiniz maili gönder
            try
            {
                await _email.SendWelcomeEmailAsync(user.Email!, user.FullName);
            }
            catch { }

            ViewData["Success"] = true;
            ViewData["FullName"] = user.FullName;
        }
        else
        {
            ViewData["Success"] = false;
        }

        return View();
    }

    // ════════════════════════════════════════════════════════
    // ONAY MAİLİ YENİDEN GÖNDER
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResendConfirmation(string email)
    {
        var user = await _um.FindByEmailAsync(email);

        if (user != null && !user.EmailConfirmed)
        {
            var token = await _um.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

            var confirmLink = Url.Action(
                "ConfirmEmail", "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme)!;

            try
            {
                await _email.SendConfirmationEmailAsync(
                    user.Email!, user.FullName, confirmLink);
                TempData["ResendSuccess"] = true;
            }
            catch
            {
                TempData["ResendError"] = true;
            }
        }

        return RedirectToAction(nameof(RegisterConfirmation),
            new { email });
    }

    // ════════════════════════════════════════════════════════
    // LOGIN GET
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_sm.IsSignedIn(User))
            return RedirectToAction("Index", "Home");
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // ════════════════════════════════════════════════════════
    // LOGIN POST
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel m,
                                           string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(m);

        // Kullanıcı var mı?
        var user = await _um.FindByEmailAsync(m.Email);

        if (user != null && !user.EmailConfirmed)
        {
            // E-posta onaylanmamış
            ModelState.AddModelError(string.Empty,
                "E-posta adresiniz henüz onaylanmamış. " +
                "Lütfen gelen kutunuzu kontrol edin.");

            ViewData["UnconfirmedEmail"] = m.Email;
            return View(m);
        }

        var result = await _sm.PasswordSignInAsync(
            m.Email, m.Password, m.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            if (user != null && await _um.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "Admin");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            TempData["SuccessMessage"] = "Hoş geldiniz!";
            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty,
                "Hesabınız kilitlendi. 5 dakika sonra tekrar deneyin.");
            return View(m);
        }

        ModelState.AddModelError(string.Empty,
            "E-posta veya şifre hatalı.");
        return View(m);
    }

    // ════════════════════════════════════════════════════════
    // LOGOUT
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _sm.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // ════════════════════════════════════════════════════════
    // ŞİFRE SIFIRLAMA — GET
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult ForgotPassword() => View();

    // ════════════════════════════════════════════════════════
    // ŞİFRE SIFIRLAMA — POST
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError(string.Empty, "E-posta giriniz.");
            return View();
        }

        var user = await _um.FindByEmailAsync(email);

        // Güvenlik: kullanıcı yoksa bile aynı mesajı göster
        if (user != null && user.EmailConfirmed)
        {
            var token = await _um.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

            var resetLink = Url.Action(
                "ResetPassword", "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme)!;

            var html = $@"
<body style='font-family:Georgia,serif;background:#fff5f7;padding:40px 20px;'>
  <div style='max-width:500px;margin:0 auto;background:white;
              border-radius:20px;padding:40px;
              box-shadow:0 10px 40px rgba(225,29,72,0.12);'>
    <h1 style='font-family:Georgia,serif;font-weight:300;color:#881337;
                font-size:1.8rem;margin-bottom:16px;'>
      Şifre Sıfırlama
    </h1>
    <p style='color:#7c3f60;font-size:0.9rem;font-weight:300;
               line-height:1.7;margin-bottom:24px;'>
      Merhaba {user.FullName}, şifrenizi sıfırlamak için
      aşağıdaki butona tıklayın. Link 1 saat geçerlidir.
    </p>
    <a href='{resetLink}'
       style='display:inline-block;padding:13px 35px;
               background:linear-gradient(135deg,#f472b6,#e11d48);
               color:white;text-decoration:none;border-radius:50px;
               font-size:0.82rem;font-weight:600;letter-spacing:0.1em;'>
      Şifremi Sıfırla
    </a>
    <p style='margin-top:20px;font-size:0.72rem;color:#d4a0b4;'>
      Bu işlemi siz yapmadıysanız dikkate almayınız.
    </p>
  </div>
</body>";

            try
            {
                await _email.SendEmailAsync(
                    user.Email!, user.FullName,
                    "🔑 Ankavera — Şifre Sıfırlama", html);
            }
            catch { }
        }

        ViewData["EmailSent"] = true;
        ViewData["SentTo"] = email;
        return View();
    }

    // ════════════════════════════════════════════════════════
    // ŞİFRE YENİLE — GET
    // ════════════════════════════════════════════════════════
    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        ViewData["UserId"] = userId;
        ViewData["Token"] = token;
        return View();
    }

    // ════════════════════════════════════════════════════════
    // ŞİFRE YENİLE — POST
    // ════════════════════════════════════════════════════════
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(string userId,
                                                    string token,
                                                    string newPassword,
                                                    string confirmPassword)
    {
        if (newPassword != confirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Şifreler eşleşmiyor.");
            ViewData["UserId"] = userId;
            ViewData["Token"] = token;
            return View();
        }

        var user = await _um.FindByIdAsync(userId);
        if (user == null)
            return RedirectToAction("Index", "Home");

        var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(token));

        var result = await _um.ResetPasswordAsync(user, decodedToken, newPassword);

        if (result.Succeeded)
        {
            ViewData["ResetSuccess"] = true;
            return View();
        }

        foreach (var e in result.Errors)
            ModelState.AddModelError(string.Empty,
                TurkishError(e.Code, e.Description));

        ViewData["UserId"] = userId;
        ViewData["Token"] = token;
        return View();
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private static string TurkishError(string code, string fallback) => code switch
    {
        "DuplicateUserName" => "Bu e-posta zaten kullanılıyor.",
        "PasswordTooShort" => "Şifre en az 6 karakter olmalıdır.",
        "PasswordRequiresDigit" => "Şifre en az bir rakam içermelidir.",
        "InvalidToken" => "Geçersiz veya süresi dolmuş link.",
        _ => fallback
    };
}