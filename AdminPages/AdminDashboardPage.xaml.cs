using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.AdminPages;

public partial class AdminDashboardPage : ContentPage
{
	public AdminDashboardPage()
	{
		InitializeComponent();
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadStatsAsync();
    }

    private async Task LoadStatsAsync()
    {
        try
        {
            var products = await FirebaseAuthServices.Instance.GetAllProductsAsync();
            var categories = await FirebaseAuthServices.Instance.GetCategoriesAsync();
            var orders = await FirebaseAuthServices.Instance.GetAllOrdersAsync();

            ProductCountLabel.Text = products.Count.ToString();
            CategoryCountLabel.Text = categories.Count.ToString();
            OrderCountLabel.Text = orders.Count.ToString();
        }
        catch { /* silently fail on stats */ }
    }

    private async void OnProductsTapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new AdminProductsPage());

    private async void OnCategoriesTapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new AdminCategoriesPage());

    private async void OnOrdersTapped(object sender, TappedEventArgs e)
        => await Navigation.PushAsync(new AdminOrdersPage());

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Logout", "Cancel");
        if (!confirm) return;

        FirebaseAuthServices.Instance.SignOut();
        Application.Current!.MainPage = new NavigationPage(new MainPage());
    }
}
