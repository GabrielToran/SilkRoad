using Multiplatoform_Project.Services;
using Multiplatoform_Project.Pages;

namespace Multiplatoform_Project.Pages;

public partial class OrderHistoryPage : ContentPage
{
    public OrderHistoryPage()
    {
        InitializeComponent();
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
            string userId = FirebaseAuthServices.Instance.CurrentUserId ?? "";
            var orders = await FirebaseAuthServices.Instance.GetUserOrdersAsync(userId);
            OrdersCollection.ItemsSource = orders;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load orders: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}