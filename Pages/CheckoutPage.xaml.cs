using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;
using Stripe;


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

    private static string MapCardNumberToTestToken(string cardNumber)
    {
        string digits = (cardNumber ?? "").Replace(" ", "").Replace("-", "");
        return digits switch
        {
            "4242424242424242" => "tok_visa",
            "4000056655665556" => "tok_visa_debit",
            "5555555555554444" => "tok_mastercard",
            "2223003122003222" => "tok_mastercard",
            "378282246310005" => "tok_amex",
            "371449635398431" => "tok_amex",
            "6011111111111117" => "tok_discover",

            // Decline scenarios
            "4000000000000002" => "tok_chargeDeclined",
            "4000000000009995" => "tok_chargeDeclinedInsufficientFunds",
            "4000000000009987" => "tok_chargeDeclinedLostCard",
            "4000000000009979" => "tok_chargeDeclinedStolenCard",
            "4000000000000069" => "tok_chargeDeclinedExpiredCard",
            "4000000000000127" => "tok_chargeDeclinedIncorrectCvc",
            "4000000000000119" => "tok_chargeDeclinedProcessingError",


            _ => "tok_visa"
        };
    }

    // ── Pay ─────────────────────────────────────────────────
    private async void OnPayClicked(object sender, EventArgs e)
    {
        // ── Validation for credit card ──────────────────────────────
        int expMonth = 0, expYear = 0;
        if (_paymentMethod == "Credit card")
        {
            if (string.IsNullOrWhiteSpace(CardholderEntry.Text))
            { await DisplayAlert("Missing info", "Please enter the cardholder name.", "OK"); return; }

            string cardNum = CardNumberEntry.Text?.Replace(" ", "") ?? "";
            if (cardNum.Length < 13)
            { await DisplayAlert("Missing info", "Please enter a valid card number.", "OK"); return; }

            var parts = (ExpiryEntry.Text ?? "").Split('/');
            if (parts.Length != 2 ||
                !int.TryParse(parts[0], out expMonth) ||
                !int.TryParse(parts[1], out expYear))
            {
                await DisplayAlert("Invalid expiry", "Use MM/YY format (e.g. 12/28).", "OK");
                return;
            }
            if (expYear < 100) expYear += 2000;

            if (string.IsNullOrWhiteSpace(CvvEntry.Text))
            { await DisplayAlert("Missing info", "Please enter the CVV.", "OK"); return; }
        }

        PayBtn.IsEnabled = false;
        PayBtn.Text = "Processing...";

        try
        {
            string paymentId = "";

            // ── Charge via Stripe if the user picked a credit card ──
            if (_paymentMethod == "Credit card")
            {
                string token = MapCardNumberToTestToken(CardNumberEntry.Text!);
                var intent = await StripeService.Instance.ChargeCardAsync(
                    amount: _cart.Total,
                    currency: "cad",                       // change if needed
                    paymentMethodToken: token,
                    description: $"Silkroad order ({_cart.TotalCount} item(s))");

                paymentId = intent.Id;
            }
            // PayPal / Cash on delivery fall through without charging Stripe.

            // ── Persist the order to Firestore ──────────────────────
            string userId = FirebaseAuthServices.Instance.CurrentUserId ?? "";
            var profile = await FirebaseAuthServices.Instance.GetUserProfileAsync(userId);

            var order = new Order
            {
                UserId = userId,
                CustomerName = profile?.FullName ?? "Guest",
                Date = DateTime.UtcNow,
                Status = "Pending",
                Total = _cart.Total,
                PaymentMethod = _paymentMethod,
                PaymentId = paymentId,
                ShippingAddress = profile?.ShippingAddress ?? "",
                Items = _cart.ToOrderItems()
            };

            string orderId = await FirebaseAuthServices.Instance.PlaceOrderAsync(order);
            _cart.Clear();

            await DisplayAlert("Order Confirmed! 🎉",
                $"Your order {orderId} has been placed.\nWe'll update you when it ships.",
                "Back to Shop");

            await Navigation.PopToRootAsync();
        }
        catch (StripeException sx)
        {
            await DisplayAlert("Payment Declined",
                sx.StripeError?.Message ?? sx.Message, "OK");
            PayBtn.IsEnabled = true;
            PayBtn.Text = $"Pay {_cart.FormattedTotal}";
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
