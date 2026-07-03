using Microsoft.AspNetCore.Identity;

namespace ANKAVERA_İÇ_GİYİM.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; } = DateTime.Now;

    // Ticari elektronik ileti (kampanya/pazarlama) açık rızası — KVKK gereği opsiyonel
    public bool AcceptsMarketing { get; set; } = false;
}