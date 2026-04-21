using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.AdminPages;

public partial class AdminProductsPage : ContentPage
{
    private List<Product> _allProducts = new();
    public bool IsRefreshing { get; set; }
    public AdminProductsPage()
    {
        InitializeComponent();
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        LoadingIndicator.IsVisible = true;
        try
        {
            _allProducts = await FirebaseAuthServices.Instance.GetAllProductsAsync();
            ProductsCollection.ItemsSource = _allProducts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load products: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLower().Trim() ?? "";

        ProductsCollection.ItemsSource = string.IsNullOrEmpty(query)
            ? _allProducts
            : _allProducts.Where(p =>
                p.Name.ToLower().Contains(query) ||
                p.categoryName.ToLower().Contains(query)).ToList();
    }

    private async void OnAddProductClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new AdminAddEditProductPage(null));

    private async void OnEditProductClicked(object sender, EventArgs e)
    {
        if (sender is ImageButton btn && btn.CommandParameter is Product product)
            await Navigation.PushAsync(new AdminAddEditProductPage(product));
    }

    private async void OnDeleteProductClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn || btn.CommandParameter is not Product product)
            return;

        bool confirm = await DisplayAlert(
            "Delete Product",
            $"Are you sure you want to delete \"{product.Name}\"? This cannot be undone.",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await FirebaseAuthServices.Instance.DeleteProductAsync(product.Id);
            _allProducts.Remove(product);
            ProductsCollection.ItemsSource = null;
            ProductsCollection.ItemsSource = _allProducts;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete product: {ex.Message}", "OK");
        }
    }

    public Command RefreshCommand => new Command(async () =>
    {
        await LoadProductsAsync();
    });

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}
