using Multiplatoform_Project.Pages;
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project;

public partial class HomePage : ContentPage
{
    public HomePage()
    {
        InitializeComponent();

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadGreetingAndCategories();
    }

    private async Task LoadGreetingAndCategories()
    {
        LoadingIndicator.IsVisible = true;
        try
        {
            string? userId = FirebaseAuthServices.Instance.CurrentUserId;
            if (userId != null)
            {
                var profile = await FirebaseAuthServices.Instance.GetUserProfileAsync(userId);
                if (profile != null)
                    GreetingLabel.Text = $"Hello, {profile.FirstName}!";
            }

            var categories = await FirebaseAuthServices.Instance.GetCategoriesAsync();
            CategoriesCollection.ItemsSource = categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load categories: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
        }
    }

    private async void OnCategoryTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is not Category cat) return;
        await Navigation.PushAsync(new Productlistpage(cat.Id, cat.Name));
    }

    private async void OnOrdersTapped(object sender, EventArgs e)
        => await Navigation.PushAsync(new OrderHistoryPage());

    private async void OnProfileTapped(object sender, EventArgs e)
        => await Navigation.PushAsync(new EditProfile());

    private async void OnCartClicked(object sender, EventArgs e)
    => await Navigation.PushAsync(new Pages.CartPage());

    private async void OnLogOutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Logout", "Are you sure you want to logout?", "Logout", "Cancel");
        if (!confirm) return;

        FirebaseAuthServices.Instance.SignOut();
        SecureStorage.Remove("firebase_token");
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}