using Multiplatoform_Project.Services;
using Multiplatoform_Project.Models;

namespace Multiplatoform_Project.Pages;

public partial class CartPage : ContentPage
{
    private readonly CartService _cart = CartService.Instance;

    public CartPage()
	{
		InitializeComponent();
        _cart.CartChanged += OnCartChanged;
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        RefreshUI();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _cart.CartChanged -= OnCartChanged;
    }

    private void OnCartChanged() =>
        MainThread.BeginInvokeOnMainThread(RefreshUI);

    private void RefreshUI()
    {
        CartCollection.ItemsSource = null;
        CartCollection.ItemsSource = _cart.Items.ToList();

        SubtotalLabel.Text = _cart.FormattedSubtotal;
        TotalLabel.Text = _cart.FormattedTotal;

        bool hasItems = _cart.Items.Count > 0;
        SummaryCard.IsVisible = hasItems;
        CheckoutBtn.IsEnabled = hasItems;
        CheckoutBtn.BackgroundColor = hasItems
            ? Color.FromArgb("#C8683A")
            : Color.FromArgb("#CCCCCC");
    }

    private void OnDecreaseQty(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
            _cart.UpdateQuantity(item, -1);
    }

    private void OnIncreaseQty(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
            _cart.UpdateQuantity(item, +1);
    }

    private async void OnRemoveItem(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is CartItem item)
        {
            bool confirm = await DisplayAlert(
                "Remove item",
                $"Remove {item.Name} from cart?",
                "Remove", "Cancel");
            if (confirm)
                _cart.RemoveItem(item);
        }
    }

    private async void OnCheckoutClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new CheckoutPage());

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}

