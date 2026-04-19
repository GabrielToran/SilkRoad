using System;
using System.Collections.Generic;
using System.Text;

namespace Multiplatoform_Project.Models
{
    public  class Product
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string categoryId { get; set; } = string.Empty;
        public string categoryName { get; set; } = string.Empty;
        public double Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string imageUrl { get; set; } = string.Empty;
        public string badge { get; set; } = string.Empty; // "Best seller", "New", "Sale", ""
        public bool available { get; set; } = true;

        public string ImageUrl => imageUrl;
        public string Badge => badge;
        public bool Available => available;
        public string CategoryName => categoryName;
        public string CategoryId => categoryId;



        // Display helpers
        public string FormattedPrice => $"${Price:F2}";
        public bool HasBadge => !string.IsNullOrEmpty(badge);
        public Color BadgeColor => badge switch
        {
            "Best seller" => Color.FromArgb("#2D2D2D"),
            "New" => Color.FromArgb("#4A7C59"),
            "Sale" => Color.FromArgb("#C8683A"),
            _ => Color.FromArgb("#2D2D2D")
        };
    }
}
