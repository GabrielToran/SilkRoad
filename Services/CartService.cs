using Multiplatoform_Project.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Multiplatoform_Project.Services
{
    public  class CartService
    {

        private static CartService? _instance;
        public static CartService Instance => _instance ??= new CartService();

        private readonly List<CartItem> _items = new();

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
        public int TotalCount => _items.Sum(i => i.Quantity);
        public double Subtotal => _items.Sum(i => i.TotalPrice);
        public double Shipping => Subtotal > 0 ? 0 : 0; // Free shipping
        public double Total => Subtotal + Shipping;

        public string FormattedSubtotal => $"${Subtotal:F2}";
        public string FormattedTotal => $"${Total:F2}";

        public event Action? CartChanged;

        public void AddItem(Product product, string variant = "", int quantity = 1)
        {
            var existing = _items.FirstOrDefault(
                i => i.ProductId == product.Id && i.Variant == variant);

            if (existing != null)
                existing.Quantity += quantity;
            else
                _items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.imageUrl,
                    Variant = variant,
                    UnitPrice = product.Price,
                    Quantity = quantity
                });

            CartChanged?.Invoke();
        }

        public void RemoveItem(CartItem item)
        {
            _items.Remove(item);
            CartChanged?.Invoke();
        }

        public void UpdateQuantity(CartItem item, int delta)
        {
            item.Quantity += delta;
            if (item.Quantity <= 0)
                _items.Remove(item);
            CartChanged?.Invoke();
        }

        public void Clear()
        {
            _items.Clear();
            CartChanged?.Invoke();
        }

        /// <summary>Converts cart items to order items for checkout.</summary>
        public List<OrderItem> ToOrderItems() =>
            _items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Name = i.Name,
                ImageUrl = i.ImageUrl,
                Price = i.UnitPrice,
                Quantity = i.Quantity,
                Variant = i.Variant
            }).ToList();
    }
}
