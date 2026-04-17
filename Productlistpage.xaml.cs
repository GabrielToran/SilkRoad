using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;


namespace Multiplatoform_Project.Pages;

public partial class Productlistpage : ContentPage
{
    private readonly string _categoryId;
    private readonly string _categoryName;
    private List<Product> _products = new();
   

    public bool IsRefreshing { get; set; }
    public List<Product> Products
    {
        get => _products;
        set { _products = value; OnPropertyChanged(); }
    }

    public Productlistpage()
	{
		InitializeComponent();
        _categoryId = categoryId;
        _categoryName = categoryName;
        BindingContext = this;

        PageTitleLabel.Text = categoryName;

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
            var items = await FirebaseService.Instance
                .GetProductsByCategoryAsync(_categoryId);
            Products = items;
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

    private async void OnProductTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Product product)
            await Navigation.PushAsync(new ProductDetailPage(product));
    }

    private void OnAddToCartClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is Product product)
        {
            CartService.Instance.AddItem(product);
            // Brief visual feedback
            DisplayAlert("Added", $"{product.Name} added to cart.", "OK");
        }
    }

    private async void OnCartClicked(object sender, EventArgs e)
        => await Navigation.PushAsync(new CartPage());

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();

    public System.Windows.Input.ICommand RefreshCommand =>
        new Command(async () => await LoadProductsAsync());
}
