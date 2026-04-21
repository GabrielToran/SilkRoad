using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;


namespace Multiplatoform_Project.AdminPages;

public partial class AdminOrdersPage : ContentPage
{
    private List<Order> _allOrders = new();
    private string _activeFilter = "All";
    public bool IsRefreshing { get; set; }

    public AdminOrdersPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        LoadingIndicator.IsVisible = true;
        try
        {
            _allOrders = await FirebaseAuthServices.Instance.GetAllOrdersAsync();
            TotalOrdersLabel.Text = $"{_allOrders.Count} total";
            ApplyFilter(_activeFilter);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load orders: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }

    private void ApplyFilter(string filter)
    {
        _activeFilter = filter;

        var filtered = filter == "All"
            ? _allOrders
            : _allOrders.Where(o => o.Status == filter).ToList();

        OrdersCollection.ItemsSource = filtered;
        UpdateTabStyles(filter);
    }

    private void UpdateTabStyles(string active)
    {
        // Reset all tabs
        var tabs = new[] { (TabAll, "All"), (TabPending, "Pending"),
                               (TabShipped, "Shipped"), (TabDelivered, "Delivered") };

        foreach (var (tab, name) in tabs)
        {
            bool isActive = name == active;
            tab.BackgroundColor = isActive
                ? Color.FromArgb("#1A1A1A")
                : Color.FromArgb("White");
            tab.Stroke = isActive
                ? new SolidColorBrush(Colors.Transparent)
                : new SolidColorBrush(Color.FromArgb("#E0D8D0"));

            // Update label color inside
            if (tab.Content is Label lbl)
            {
                lbl.TextColor = isActive ? Colors.White : Color.FromArgb("#666666");
                lbl.FontAttributes = isActive ? FontAttributes.Bold : FontAttributes.None;
            }
        }
    }

    private void OnFilterAll(object sender, TappedEventArgs e) => ApplyFilter("All");
    private void OnFilterPending(object sender, TappedEventArgs e) => ApplyFilter("Pending");
    private void OnFilterShipped(object sender, TappedEventArgs e) => ApplyFilter("Shipped");
    private void OnFilterDelivered(object sender, TappedEventArgs e) => ApplyFilter("Delivered");

    private async void OnOrderTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Order order) return;
        await Navigation.PushAsync(new AdminOrderDetailPage(order));
    }

    public Command RefreshCommand => new Command(async () =>
    {
        await LoadOrdersAsync();
    });
    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}