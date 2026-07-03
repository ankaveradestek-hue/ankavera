using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ANKAVERA_İÇ_GİYİM.Models;

namespace ANKAVERA_İÇ_GİYİM.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    // ── Genel Mail Gönder ─────────────────────────────────────────────────
    public async Task SendEmailAsync(string toEmail, string toName,
                                     string subject, string htmlBody)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_settings.FromName,
                                            _settings.FromEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(_settings.SmtpHost,
                                   _settings.SmtpPort,
                                   SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(_settings.SmtpUser,
                                        _settings.SmtpPass);

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    // ── E-posta Onay Maili ────────────────────────────────────────────────
    public async Task SendConfirmationEmailAsync(string toEmail,
                                                  string toName,
                                                  string confirmLink)
    {
        var html = $@"
<!DOCTYPE html>
<html lang='tr'>
<head>
<meta charset='UTF-8' />
<meta name='viewport' content='width=device-width,initial-scale=1.0'/>
<title>E-posta Onayı</title>
</head>
<body style='margin:0;padding:0;background:#fff5f7;
             font-family:Georgia,serif;'>

  <table width='100%' cellpadding='0' cellspacing='0'
         style='background:#fff5f7;padding:40px 20px;'>
    <tr>
      <td align='center'>
        <table width='560' cellpadding='0' cellspacing='0'
               style='background:white;border-radius:20px;
                      box-shadow:0 10px 40px rgba(225,29,72,0.12);
                      overflow:hidden;max-width:560px;width:100%;'>

          <!-- Header -->
          <tr>
            <td style='background:linear-gradient(135deg,#f472b6,#e11d48,#be185d);
                        padding:40px 40px 35px;text-align:center;'>
              <h1 style='margin:0;font-family:Georgia,serif;font-size:2.2rem;
                          font-weight:300;color:white;letter-spacing:0.12em;'>
                Ankavera
              </h1>
              <p style='margin:8px 0 0;color:rgba(255,255,255,0.8);
                         font-size:0.85rem;font-weight:300;letter-spacing:0.06em;'>
                Lüks İç Giyim
              </p>
            </td>
          </tr>

          <!-- İçerik -->
          <tr>
            <td style='padding:40px 40px 30px;'>
              <h2 style='margin:0 0 16px;font-family:Georgia,serif;
                          font-size:1.6rem;font-weight:400;color:#881337;
                          letter-spacing:0.02em;'>
                Hoş Geldiniz, {toName}! 🌸
              </h2>
              <p style='margin:0 0 12px;font-size:0.9rem;color:#7c3f60;
                         font-weight:300;line-height:1.7;'>
                Ankavera ailesine katıldığınız için teşekkür ederiz.
                Hesabınızı aktive etmek için aşağıdaki butona tıklayın.
              </p>
              <p style='margin:0 0 30px;font-size:0.85rem;color:#be7094;
                         font-weight:300;line-height:1.6;'>
                Bu link <strong>24 saat</strong> geçerlidir.
              </p>

              <!-- Buton -->
              <table width='100%' cellpadding='0' cellspacing='0'>
                <tr>
                  <td align='center'>
                    <a href='{confirmLink}'
                       style='display:inline-block;
                               padding:14px 40px;
                               background:linear-gradient(135deg,#f472b6,#e11d48,#be185d);
                               color:white;text-decoration:none;
                               border-radius:50px;font-size:0.85rem;
                               font-weight:600;letter-spacing:0.12em;
                               text-transform:uppercase;
                               box-shadow:0 6px 20px rgba(225,29,72,0.35);'>
                      E-postamı Onayla
                    </a>
                  </td>
                </tr>
              </table>

              <!-- Alternatif Link -->
              <p style='margin:25px 0 0;font-size:0.75rem;color:#d4a0b4;
                         font-weight:300;line-height:1.6;'>
                Buton çalışmıyorsa bu linki kopyalayıp tarayıcınıza yapıştırın:<br/>
                <a href='{confirmLink}'
                   style='color:#f43f7a;word-break:break-all;'>
                  {confirmLink}
                </a>
              </p>
            </td>
          </tr>

          <!-- Divider -->
          <tr>
            <td style='padding:0 40px;'>
              <div style='height:1px;
                           background:linear-gradient(90deg,transparent,#fce7f3,transparent);'>
              </div>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding:25px 40px 35px;text-align:center;'>
              <p style='margin:0 0 8px;font-size:0.75rem;color:#d4a0b4;font-weight:300;'>
                Bu e-postayı siz talep ettiniz. Eğer kayıt olmadıysanız
                dikkate almayınız.
              </p>
              <p style='margin:0;font-size:0.75rem;color:#e4b0c8;font-weight:300;'>
                © {DateTime.Now.Year} Ankavera · Lüks İç Giyim
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>

</body>
</html>";

        await SendEmailAsync(toEmail, toName,
            "✉ Ankavera — E-posta Adresinizi Onaylayın", html);
    }

    // ── Hoş Geldiniz Maili (Onay Sonrası) ────────────────────────────────
    public async Task SendWelcomeEmailAsync(string toEmail, string toName)
    {
        var html = $@"
<!DOCTYPE html>
<html lang='tr'>
<head><meta charset='UTF-8'/></head>
<body style='margin:0;padding:0;background:#fff5f7;font-family:Georgia,serif;'>
  <table width='100%' cellpadding='0' cellspacing='0'
         style='background:#fff5f7;padding:40px 20px;'>
    <tr>
      <td align='center'>
        <table width='560' cellpadding='0' cellspacing='0'
               style='background:white;border-radius:20px;
                      box-shadow:0 10px 40px rgba(225,29,72,0.12);
                      overflow:hidden;max-width:560px;width:100%;'>

          <tr>
            <td style='background:linear-gradient(135deg,#f472b6,#e11d48,#be185d);
                        padding:40px;text-align:center;'>
              <h1 style='margin:0;font-family:Georgia,serif;font-size:2.2rem;
                          font-weight:300;color:white;letter-spacing:0.12em;'>
                Ankavera
              </h1>
            </td>
          </tr>

          <tr>
            <td style='padding:40px;text-align:center;'>
              <div style='font-size:3rem;margin-bottom:16px;'>🌸</div>
              <h2 style='margin:0 0 16px;font-family:Georgia,serif;
                          font-size:1.6rem;font-weight:400;color:#881337;'>
                Hesabınız Onaylandı!
              </h2>
              <p style='margin:0 0 30px;font-size:0.88rem;color:#7c3f60;
                         font-weight:300;line-height:1.7;'>
                Merhaba {toName}, e-posta adresiniz başarıyla doğrulandı.
                Artık tüm koleksiyonlarımıza erişebilirsiniz.
              </p>
              <a href='https://ankavera.com/Collections'
                 style='display:inline-block;padding:14px 35px;
                         background:linear-gradient(135deg,#f472b6,#e11d48);
                         color:white;text-decoration:none;border-radius:50px;
                         font-size:0.82rem;font-weight:600;
                         letter-spacing:0.1em;text-transform:uppercase;'>
                Alışverişe Başla
              </a>
            </td>
          </tr>

          <tr>
            <td style='padding:20px 40px 30px;text-align:center;'>
              <p style='margin:0;font-size:0.73rem;color:#e4b0c8;font-weight:300;'>
                © {DateTime.Now.Year} Ankavera · Lüks İç Giyim
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        await SendEmailAsync(toEmail, toName,
            "🌸 Ankavera'ya Hoş Geldiniz!", html);
    }

    // ── Sipariş Onay + Takip Kodu Maili ──────────────────────────────────
    public async Task SendOrderConfirmationEmailAsync(string toEmail, string toName,
                                                      Order order, List<CartItem> items)
    {
        // Ürün satırlarını oluştur
        var rows = "";
        foreach (var it in items)
        {
            rows += $@"
              <tr>
                <td style='padding:12px 0;border-bottom:1px solid #fce7f3;
                            font-size:0.85rem;color:#7c3f60;font-weight:300;'>
                  {it.Name}
                  <span style='color:#be7094;'>× {it.Quantity}</span>
                </td>
                <td style='padding:12px 0;border-bottom:1px solid #fce7f3;
                            text-align:right;font-size:0.85rem;color:#881337;
                            font-weight:600;white-space:nowrap;'>
                  {it.Total.ToString("N2")} ₺
                </td>
              </tr>";
        }

        var html = $@"
<!DOCTYPE html>
<html lang='tr'>
<head><meta charset='UTF-8'/><meta name='viewport' content='width=device-width,initial-scale=1.0'/></head>
<body style='margin:0;padding:0;background:#fff5f7;font-family:Georgia,serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#fff5f7;padding:40px 20px;'>
    <tr>
      <td align='center'>
        <table width='560' cellpadding='0' cellspacing='0'
               style='background:white;border-radius:20px;
                      box-shadow:0 10px 40px rgba(225,29,72,0.12);
                      overflow:hidden;max-width:560px;width:100%;'>

          <!-- Header -->
          <tr>
            <td style='background:linear-gradient(135deg,#f472b6,#e11d48,#be185d);
                        padding:40px;text-align:center;'>
              <h1 style='margin:0;font-family:Georgia,serif;font-size:2.2rem;
                          font-weight:300;color:white;letter-spacing:0.12em;'>Ankavera</h1>
              <p style='margin:8px 0 0;color:rgba(255,255,255,0.85);
                         font-size:0.85rem;font-weight:300;letter-spacing:0.06em;'>
                Siparişiniz Alındı 🎀
              </p>
            </td>
          </tr>

          <!-- Selam -->
          <tr>
            <td style='padding:38px 40px 10px;'>
              <div style='font-size:2.6rem;text-align:center;margin-bottom:12px;'>✅</div>
              <h2 style='margin:0 0 14px;font-family:Georgia,serif;font-size:1.55rem;
                          font-weight:400;color:#881337;text-align:center;'>
                Teşekkürler, {toName}!
              </h2>
              <p style='margin:0 0 8px;font-size:0.9rem;color:#7c3f60;
                         font-weight:300;line-height:1.7;text-align:center;'>
                Siparişiniz başarıyla oluşturuldu ve hazırlanmaya alındı.
                Kargoya verildiğinde aşağıdaki takip kodu ile takip edebilirsiniz.
              </p>
            </td>
          </tr>

          <!-- Takip Kodu -->
          <tr>
            <td style='padding:14px 40px 6px;'>
              <table width='100%' cellpadding='0' cellspacing='0'
                     style='background:linear-gradient(135deg,#fff5f7,#fce7f3);
                            border:1px dashed #f9a8d4;border-radius:14px;'>
                <tr>
                  <td style='padding:18px 24px;text-align:center;'>
                    <p style='margin:0 0 6px;font-size:0.68rem;letter-spacing:0.18em;
                               text-transform:uppercase;color:#be7094;font-weight:600;'>
                      Kargo Takip Kodu
                    </p>
                    <p style='margin:0;font-family:monospace;font-size:1.35rem;
                               font-weight:700;color:#be185d;letter-spacing:0.08em;'>
                      {order.TrackingCode}
                    </p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Sipariş Bilgileri -->
          <tr>
            <td style='padding:22px 40px 4px;'>
              <p style='margin:0 0 4px;font-size:0.8rem;color:#881337;font-weight:600;'>
                Sipariş No: <span style='color:#be185d;'>#{order.Id}</span>
              </p>
              <p style='margin:0 0 4px;font-size:0.8rem;color:#7c3f60;font-weight:300;'>
                Tarih: {order.OrderDate:dd.MM.yyyy HH:mm}
              </p>
              <p style='margin:0;font-size:0.8rem;color:#7c3f60;font-weight:300;'>
                Durum: <strong style='color:#be185d;'>{order.OrderStatus}</strong>
              </p>
            </td>
          </tr>

          <!-- Ürünler -->
          <tr>
            <td style='padding:18px 40px 0;'>
              <h3 style='margin:0 0 6px;font-family:Georgia,serif;font-size:1.05rem;
                          font-weight:500;color:#881337;'>Sipariş Özeti</h3>
              <table width='100%' cellpadding='0' cellspacing='0'>
                {rows}
                <tr>
                  <td style='padding:16px 0 0;font-size:1rem;color:#881337;
                              font-weight:700;'>Toplam</td>
                  <td style='padding:16px 0 0;text-align:right;font-size:1.15rem;
                              color:#be185d;font-weight:700;white-space:nowrap;'>
                    {order.TotalPrice.ToString("N2")} ₺
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Teslimat Adresi -->
          <tr>
            <td style='padding:22px 40px 0;'>
              <div style='background:#fff5f7;border-radius:12px;padding:16px 18px;'>
                <p style='margin:0 0 6px;font-size:0.7rem;letter-spacing:0.12em;
                           text-transform:uppercase;color:#be7094;font-weight:600;'>
                  Teslimat Adresi
                </p>
                <p style='margin:0;font-size:0.82rem;color:#7c3f60;
                           font-weight:300;line-height:1.6;'>
                  {order.CustomerName}<br/>
                  {order.ShippingAddress}{(string.IsNullOrWhiteSpace(order.City) ? "" : " / " + order.City)}<br/>
                  {order.PhoneNumber}
                </p>
              </div>
            </td>
          </tr>

          <!-- Hijyen Notu -->
          <tr>
            <td style='padding:18px 40px 0;'>
              <p style='margin:0;font-size:0.72rem;color:#be7094;font-weight:300;
                         line-height:1.6;background:#fff5f7;border-left:2px solid #f43f7a;
                         border-radius:6px;padding:10px 12px;'>
                ⚠️ Hijyen koşulları gereği iç giyim ürünlerinde iade ve değişim
                yapılmamaktadır.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding:28px 40px 34px;text-align:center;'>
              <p style='margin:0 0 6px;font-size:0.75rem;color:#d4a0b4;font-weight:300;'>
                Sorularınız için: ankaveradestek@gmail.com
              </p>
              <p style='margin:0;font-size:0.73rem;color:#e4b0c8;font-weight:300;'>
                © {DateTime.Now.Year} Ankavera · Lüks İç Giyim
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        await SendEmailAsync(toEmail, toName,
            $"🎀 Siparişiniz Alındı — Takip Kodu: {order.TrackingCode}", html);
    }

    // ── Kargoya Verildi Bildirimi ────────────────────────────────────────
    public async Task SendShippingNotificationEmailAsync(string toEmail, string toName,
                                                         Order order)
    {
        var html = $@"
<!DOCTYPE html>
<html lang='tr'>
<head><meta charset='UTF-8'/><meta name='viewport' content='width=device-width,initial-scale=1.0'/></head>
<body style='margin:0;padding:0;background:#fff5f7;font-family:Georgia,serif;'>
  <table width='100%' cellpadding='0' cellspacing='0' style='background:#fff5f7;padding:40px 20px;'>
    <tr>
      <td align='center'>
        <table width='560' cellpadding='0' cellspacing='0'
               style='background:white;border-radius:20px;
                      box-shadow:0 10px 40px rgba(225,29,72,0.12);
                      overflow:hidden;max-width:560px;width:100%;'>

          <!-- Header -->
          <tr>
            <td style='background:linear-gradient(135deg,#f472b6,#e11d48,#be185d);
                        padding:40px;text-align:center;'>
              <h1 style='margin:0;font-family:Georgia,serif;font-size:2.2rem;
                          font-weight:300;color:white;letter-spacing:0.12em;'>Ankavera</h1>
              <p style='margin:8px 0 0;color:rgba(255,255,255,0.85);
                         font-size:0.85rem;font-weight:300;letter-spacing:0.06em;'>
                Siparişiniz Yola Çıktı 🚚
              </p>
            </td>
          </tr>

          <!-- İçerik -->
          <tr>
            <td style='padding:38px 40px 10px;'>
              <div style='font-size:2.6rem;text-align:center;margin-bottom:12px;'>📦</div>
              <h2 style='margin:0 0 14px;font-family:Georgia,serif;font-size:1.55rem;
                          font-weight:400;color:#881337;text-align:center;'>
                Kargoya Verildi, {toName}!
              </h2>
              <p style='margin:0 0 8px;font-size:0.9rem;color:#7c3f60;
                         font-weight:300;line-height:1.7;text-align:center;'>
                #{order.Id} numaralı siparişiniz kargoya teslim edildi.
                Aşağıdaki bilgilerle gönderinizi takip edebilirsiniz.
              </p>
            </td>
          </tr>

          <!-- Kargo Bilgileri -->
          <tr>
            <td style='padding:14px 40px 6px;'>
              <table width='100%' cellpadding='0' cellspacing='0'
                     style='background:linear-gradient(135deg,#fff5f7,#fce7f3);
                            border:1px dashed #f9a8d4;border-radius:14px;'>
                <tr>
                  <td style='padding:20px 24px;'>
                    <p style='margin:0 0 4px;font-size:0.68rem;letter-spacing:0.16em;
                               text-transform:uppercase;color:#be7094;font-weight:600;'>
                      Kargo Firması
                    </p>
                    <p style='margin:0 0 16px;font-size:1rem;color:#881337;font-weight:600;'>
                      {order.CargoCompany}
                    </p>
                    <p style='margin:0 0 4px;font-size:0.68rem;letter-spacing:0.16em;
                               text-transform:uppercase;color:#be7094;font-weight:600;'>
                      Kargo Takip Numarası
                    </p>
                    <p style='margin:0;font-family:monospace;font-size:1.3rem;
                               font-weight:700;color:#be185d;letter-spacing:0.06em;'>
                      {order.CargoTrackingNumber}
                    </p>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Sipariş Referansı -->
          <tr>
            <td style='padding:20px 40px 0;'>
              <p style='margin:0;font-size:0.8rem;color:#7c3f60;font-weight:300;'>
                Sipariş Referans Kodu:
                <strong style='color:#be185d;font-family:monospace;'>{order.TrackingCode}</strong>
              </p>
            </td>
          </tr>

          <!-- Teslimat Adresi -->
          <tr>
            <td style='padding:18px 40px 0;'>
              <div style='background:#fff5f7;border-radius:12px;padding:16px 18px;'>
                <p style='margin:0 0 6px;font-size:0.7rem;letter-spacing:0.12em;
                           text-transform:uppercase;color:#be7094;font-weight:600;'>
                  Teslimat Adresi
                </p>
                <p style='margin:0;font-size:0.82rem;color:#7c3f60;
                           font-weight:300;line-height:1.6;'>
                  {order.CustomerName}<br/>
                  {order.ShippingAddress}{(string.IsNullOrWhiteSpace(order.City) ? "" : " / " + order.City)}<br/>
                  {order.PhoneNumber}
                </p>
              </div>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style='padding:28px 40px 34px;text-align:center;'>
              <p style='margin:0 0 6px;font-size:0.75rem;color:#d4a0b4;font-weight:300;'>
                Sorularınız için: ankaveradestek@gmail.com
              </p>
              <p style='margin:0;font-size:0.73rem;color:#e4b0c8;font-weight:300;'>
                © {DateTime.Now.Year} Ankavera · Lüks İç Giyim
              </p>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

        await SendEmailAsync(toEmail, toName,
            $"🚚 Siparişiniz Kargoya Verildi — #{order.Id}", html);
    }
}