using System.ComponentModel.DataAnnotations;

namespace ANKAVERA_İÇ_GİYİM.Models;

public class CheckoutViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [Display(Name = "Ad Soyad")]
    public string CustomerName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "E-posta")]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
    [Display(Name = "Telefon")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Teslimat adresi zorunludur.")]
    [Display(Name = "Adres")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şehir zorunludur.")]
    [Display(Name = "Şehir")]
    public string City { get; set; } = string.Empty;

    // Yalnızca özet gösterimi için (POST'ta session'dan yeniden doldurulur)
    public CartViewModel Cart { get; set; } = new();
}
