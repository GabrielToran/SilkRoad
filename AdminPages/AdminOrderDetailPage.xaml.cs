using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.AdminPages;

public partial class AdminOrderDetailPage : ContentPage
{
    private Order _order;

    public AdminOrderDetailPage(Order order)
    {
        InitializeComponent();
        _order = order;
        PopulateUI();
    }

    private void PopulateUI()
    {
        OrderIdLabel.Text = _order.OrderId;
        CustomerNameLabel.Text = _order.CustomerName;
        OrderDateLabel.Text = _order.FormattedDate;
        ShippingAddressLabel.Text = _order.ShippingAddress;
        TotalLabel.Text = _order.FormattedTotal;
        PaymentMethodLabel.Text = _order.PaymentMethod;

        ItemsCollection.ItemsSource = _order.Items;

        UpdateStatusBadge(_order.Status);
    }

    private void UpdateStatusBadge(string status)
    {
        StatusLabel.Text = status;

        switch (status)
        {
            case "Pending":
                StatusBadge.BackgroundColor = Color.FromArgb("#FFF3E0");
                StatusLabel.TextColor = Color.FromArgb("#E67E22");
                break;
            case "Shipped":
                StatusBadge.BackgroundColor = Color.FromArgb("#E8F0FE");
                StatusLabel.TextColor = Color.FromArgb("#2E6DA4");
                break;
            case "Delivered":
                StatusBadge.BackgroundColor = Color.FromArgb("#EEF5F0");
                StatusLabel.TextColor = Color.FromArgb("#4A7C59");
                break;
        }

        // Highlight active status button
        PendingBtn.Opacity = status == "Pending" ? 1.0 : 0.5;
        ShippedBtn.Opacity = status == "Shipped" ? 1.0 : 0.5;
        DeliveredBtn.Opacity = status == "Delivered" ? 1.0 : 0.5;
    }
    private async Task UpdateStatusAsync(string newStatus)
    {
        if (_order.Status == newStatus) return;

        bool confirm = await DisplayAlert(
            "Update Status",
            $"Change order status to \"{newStatus}\"?",
            "Confirm", "Cancel");

        if (!confirm) return;

        try
        {
            await FirebaseAuthServices.Instance
                .UpdateOrderStatusAsync(_order.DocId, newStatus);

            _order.Status = newStatus;
            UpdateStatusBadge(newStatus);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update status: {ex.Message}", "OK");
        }
    }

    private async void OnSetPending(object sender, EventArgs e)
            => await UpdateStatusAsync("Pending");

    private async void OnSetShipped(object sender, EventArgs e)
        => await UpdateStatusAsync("Shipped");

    private async void OnSetDelivered(object sender, EventArgs e)
        => await UpdateStatusAsync("Delivered");

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}