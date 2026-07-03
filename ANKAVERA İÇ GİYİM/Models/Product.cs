using System.ComponentModel.DataAnnotations;

namespace ANKAVERA_İÇ_GİYİM.Models;

public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Fiyat zorunludur.")]
    [Range(0.01, 999999, ErrorMessage = "Geçerli bir fiyat giriniz.")]
    public decimal Price { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kategori zorunludur.")]
    public string CategoryName { get; set; } = string.Empty;

    public bool IsNew { get; set; }
    public bool IsBestseller { get; set; }
    public string Badge { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int StockCount { get; set; } = 99;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}