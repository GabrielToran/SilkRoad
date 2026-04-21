using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;


namespace Multiplatoform_Project.Pages;

public partial class ProductDetailPage : ContentPage
{

    private readonly Product _product;
    private int _quantity = 1;

    public ProductDetailPage(Product product)
    {
        InitializeComponent();
        _product = product;
        PopulateUI();
    }


    private void PopulateUI()
    {
        // Image from Firebase Storage URL
        ProductImage.Source = string.IsNullOrEmpty(_product.imageUrl)
            ? "placeholder_product.png"
            : ImageSource.FromUri(new Uri(_product.imageUrl));

        ProductName.Text = _product.Name;
        CategoryLabel.Text = _product.categoryName;
        PriceLabel.Text = _product.FormattedPrice;
        DescriptionLabel.Text = _product.Description;
        QtyLabel.Text = "1";

        // Badge
        if (_product.HasBadge)
        {
            BadgeBorder.IsVisible = true;
            BadgeLabel.Text = _product.badge;
            BadgeBorder.BackgroundColor = _product.BadgeColor;
        }

        // Availability
        if (!_product.available)
        {
            AvailabilityDot.Fill = new SolidColorBrush(Color.FromArgb("#C8683A"));
            AvailabilityLabel.Text = "Out of stock";
            AvailabilityLabel.TextColor = Color.FromArgb("#C8683A");
            AddToCartBtn.IsEnabled = false;
            AddToCartBtn.BackgroundColor = Color.FromArgb("#CCCCCC");
        }
    }


    private void OnIncreaseQty(object sender, EventArgs e)
    {
        _quantity++;
        QtyLabel.Text = _quantity.ToString();
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (_quantity > 1)
        {
            _quantity--;
            QtyLabel.Text = _quantity.ToString();
        }
    }

    private async void OnAddToCartClicked(object sender, EventArgs e)
    {
        CartService.Instance.AddItem(_product, quantity: _quantity);

        // Animate button briefly
        AddToCartBtn.Text = "✓ Added!";
        await Task.Delay(900);
        AddToCartBtn.Text = "Add to cart";

        // Ask user if they want to go to cart
        bool goToCart = await DisplayAlert(
            "Added to Cart",
            $"{_quantity}× {_product.Name} added successfully.",
            "View Cart", "Continue Shopping");

        if (goToCart)
            await Navigation.PushAsync(new CartPage());
    }

    private async void OnCartClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new CartPage());

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}
