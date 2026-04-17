using System;
using System.Collections.Generic;
using System.Text;

namespace Multiplatoform_Project.Models
{
    public  class Order
    {
        public string OrderId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Delivered
        public double Total { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public string PaymentMethod { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;

        public string FormattedDate => Date.ToString("MMM dd, yyyy");
        public string FormattedTotal => $"${Total:F2}";
        public int ItemCount => Items?.Count ?? 0;

    }

    public class OrderItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public double Price { get; set; }
        public int Quantity { get; set; }
        public string Variant { get; set; } = string.Empty;

        public double LineTotal => Price * Quantity;
        public string FormattedPrice => $"${Price:F2}";
    }
}
