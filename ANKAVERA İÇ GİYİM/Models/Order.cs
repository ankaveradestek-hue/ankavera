using System;
using System.Collections.Generic;

namespace ANKAVERA_İÇ_GİYİM.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Soru işareti (?) ekleyerek boş olabileceğini belirttik
        public string? AppUserId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public string OrderStatus { get; set; } = "Onay Bekliyor";

        public decimal TotalPrice { get; set; }

        // Siparişi veren kişinin iletişim bilgileri (mail için gerekli)
        public string? CustomerName { get; set; }
        public string? CustomerEmail { get; set; }

        // Kargo takip kodu (sipariş onay mailinde gönderilir — sipariş referansı)
        public string? TrackingCode { get; set; }

        // Gerçek kargo firması bilgisi (admin kargoya verince girer)
        public string? CargoCompany { get; set; }
        public string? CargoTrackingNumber { get; set; }

        public string? ShippingAddress { get; set; }
        public string? City { get; set; }
        public string? PhoneNumber { get; set; }

        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}