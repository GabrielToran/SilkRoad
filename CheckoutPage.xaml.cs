using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;


namespace Multiplatoform_Project.Pages;

public partial class CheckoutPage : ContentPage
{
    private readonly CartService _cart = CartService.Instance;
    private string _paymentMethod = "Credit card";

    public CheckoutPage()
	{
		InitializeComponent();
        PopulateOrderSummary();
    }

    private void PopulateOrderSummary()
    {
        // Build display items (name + formatted line total)
        var displayItems = _cart.Items.Select(i => new
        {
            Name = i.Quantity > 1 ? $"{i.Name} ×{i.Quantity}" : i.Name,
            FormattedPrice = $"${i.TotalPrice:F2}"
        }).ToList();

        OrderItemsList.ItemsSource = displayItems;
        TotalLabel.Text = _cart.FormattedTotal;
        PayBtn.Text = $"Pay {_cart.FormattedTotal}";
    }

    // ── Payment method selection ─────────────────────────────
    private void OnSelectCreditCard(object sender, TappedEventArgs e)
    {
        _paymentMethod = "Credit card";
        CardFields.IsVisible = true;
        SetActiveTab(CreditCardTab, PayPalTab, CashTab);
    }

    private void OnSelectPayPal(object sender, TappedEventArgs e)
    {
        _paymentMethod = "PayPal";
        CardFields.IsVisible = false;
        SetActiveTab(PayPalTab, CreditCardTab, CashTab);
    }

    private void OnSelectCash(object sender, TappedEventArgs e)
    {
        _paymentMethod = "Cash on delivery";
        CardFields.IsVisible = false;
        SetActiveTab(CashTab, CreditCardTab, PayPalTab);
    }

    private static void SetActiveTab(Border active, params Border[] inactive)
    {
        active.BackgroundColor = Color.FromArgb("#FDF4EF");
        active.Stroke = new SolidColorBrush(Color.FromArgb("#C8683A"));
        active.StrokeThickness = 1.5;

        foreach (var tab in inactive)
        {
            tab.BackgroundColor = Color.FromArgb("#F8F8F8");
            tab.Stroke = new SolidColorBrush(Color.FromArgb("#E0E0E0"));
            tab.StrokeThickness = 1;
        }
    }

    // ── Pay ─────────────────────────────────────────────────
    private async void OnPayClicked(object sender, EventArgs e)
    {
        // Basic validation for credit card
        if (_paymentMethod == "Credit card")
        {
            if (string.IsNullOrWhiteSpace(CardholderEntry.Text))
            { await DisplayAlert("Missing info", "Please enter the cardholder name.", "OK"); return; }
            if (string.IsNullOrWhiteSpace(CardNumberEntry.Text) || CardNumberEntry.Text.Length < 13)
            { await DisplayAlert("Missing info", "Please enter a valid card number.", "OK"); return; }
            if (string.IsNullOrWhiteSpace(ExpiryEntry.Text))
            { await DisplayAlert("Missing info", "Please enter the expiry date.", "OK"); return; }
            if (string.IsNullOrWhiteSpace(CvvEntry.Text))
            { await DisplayAlert("Missing info", "Please enter the CVV.", "OK"); return; }
        }

        PayBtn.IsEnabled = false;
        PayBtn.Text = "Processing...";

        try
        {
            // Get current user profile
            string userId = FirebaseAuthService.Instance.CurrentUserId ?? "";
            var profile = await FirebaseAuthService.Instance.GetUserProfileAsync(userId);

            var order = new Order
            {
                UserId = userId,
                CustomerName = profile?.FullName ?? "Guest",
                Date = DateTime.UtcNow,
                Status = "Pending",
                Total = _cart.Total,
                PaymentMethod = _paymentMethod,
                ShippingAddress = profile?.ShippingAddress ?? "",
                Items = _cart.ToOrderItems()
            };

            string orderId = await FirebaseService.Instance.PlaceOrderAsync(order);

            _cart.Clear();

            await DisplayAlert("Order Confirmed! 🎉",
                $"Your order {orderId} has been placed.\nWe'll update you when it ships.",
                "Back to Shop");

            // Pop back to home
            await Navigation.PopToRootAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Payment Failed", ex.Message, "OK");
            PayBtn.IsEnabled = true;
            PayBtn.Text = $"Pay {_cart.FormattedTotal}";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}
