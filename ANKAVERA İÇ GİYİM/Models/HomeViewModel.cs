namespace ANKAVERA_İÇ_GİYİM.Models;

public class HomeViewModel
{
    public List<Product> FeaturedProducts { get; set; } = new();
    public List<Category> Categories { get; set; } = new();
}