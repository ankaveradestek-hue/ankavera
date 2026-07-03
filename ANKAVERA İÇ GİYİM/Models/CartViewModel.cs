namespace ANKAVERA_İÇ_GİYİM.Models;

public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal => Items.Sum(i => i.Total);
    public decimal Shipping => Subtotal >= 500 ? 0 : 49.90m;
    public decimal GrandTotal => Subtotal + Shipping;
    public int ItemCount => Items.Sum(i => i.Quantity);
}