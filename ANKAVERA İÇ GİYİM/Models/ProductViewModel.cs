using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ANKAVERA_İÇ_GİYİM.Models;

public class ProductViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [Display(Name = "Ürün Adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, 999999, ErrorMessage = "Geçerli fiyat girin.")]
    [Display(Name = "Fiyat (₺)")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Kategori seçiniz.")]
    [Display(Name = "Kategori")]
    public string CategoryName { get; set; } = string.Empty;

    [Display(Name = "Stok Adedi")]
    [Range(0, 10000)]
    public int StockCount { get; set; } = 99;

    [Display(Name = "Yeni Ürün")]
    public bool IsNew { get; set; }

    [Display(Name = "Çok Satan")]
    public bool IsBestseller { get; set; }

    [Display(Name = "Aktif (Satışta)")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Rozet Metni")]
    public string Badge { get; set; } = string.Empty;

    // Mevcut resim URL (düzenleme için)
    public string ExistingImageUrl { get; set; } = string.Empty;

    // Yeni resim yükleme
    [Display(Name = "Ürün Görseli")]
    public IFormFile? ImageFile { get; set; }

    // Veya URL ile resim
    [Display(Name = "Görsel URL (opsiyonel)")]
    public string ImageUrl { get; set; } = string.Empty;
}