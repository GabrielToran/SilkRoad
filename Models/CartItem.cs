using System;
using System.Collections.Generic;
using System.Text;

namespace Multiplatoform_Project.Models
{
    public class CartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Variant { get; set; } = string.Empty; // e.g. "White · Queen"
        public double UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;

        public double TotalPrice => UnitPrice * Quantity;
        public string FormattedUnitPrice => $"${UnitPrice:F2}";
        public string FormattedTotal => $"${TotalPrice:F2}";
    }
}
