using ANKAVERA_İÇ_GİYİM.Models;

namespace ANKAVERA_İÇ_GİYİM.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string toName,
                        string subject, string htmlBody);

    Task SendConfirmationEmailAsync(string toEmail,
                                    string toName,
                                    string confirmLink);

    Task SendWelcomeEmailAsync(string toEmail, string toName);

    // Sipariş alındı + kargo takip kodu maili
    Task SendOrderConfirmationEmailAsync(string toEmail, string toName,
                                         Order order, List<CartItem> items);

    // Kargoya verildi bildirimi (gerçek kargo firması + takip no)
    Task SendShippingNotificationEmailAsync(string toEmail, string toName, Order order);
}