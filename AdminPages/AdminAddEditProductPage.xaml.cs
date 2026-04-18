using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.AdminPages;

public partial class AdminAddEditProductPage : ContentPage
{
    private readonly Product? _existingProduct;   // null = Add mode
    private List<Category> _categories = new();
    private Stream? _imageStream;
    private string _imageFileName = "";
    private bool _isEditMode => _existingProduct != null;

    public AdminAddEditProductPage(Product? product)
    {
        InitializeComponent();
        _existingProduct = product;
        PageTitleLabel.Text = _isEditMode ? "Edit product" : "Add product";
        SaveBtn.Text = _isEditMode ? "Save changes" : "Save product";
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategoriesAsync();

        if (_isEditMode)
            PopulateFields();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            _categories = await FirebaseAuthServices.Instance.GetCategoriesAsync();
            CategoryPicker.ItemsSource = _categories.Select(c => c.Name).ToList();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load categories: {ex.Message}", "OK");
        }
    }

    private void PopulateFields()
    {
        if (_existingProduct == null) return;

        NameEntry.Text = _existingProduct.Name;
        PriceEntry.Text = _existingProduct.Price.ToString("F2");
        DescriptionEditor.Text = _existingProduct.Description;
        AvailableSwitch.IsToggled = _existingProduct.available;

        // Set category picker
        int catIndex = _categories.FindIndex(c => c.Id == _existingProduct.categoryId);
        if (catIndex >= 0)
            CategoryPicker.SelectedIndex = catIndex;

        // Set badge picker
        string[] badges = { "None", "Best seller", "New", "Sale" };
        int badgeIndex = Array.IndexOf(badges, _existingProduct.badge);
        BadgePicker.SelectedIndex = badgeIndex >= 0 ? badgeIndex : 0;

        // Show existing image
        if (!string.IsNullOrEmpty(_existingProduct.imageUrl))
        {
            ProductImagePreview.Source = ImageSource.FromUri(new Uri(_existingProduct.imageUrl));
            ProductImagePreview.IsVisible = true;
            ImagePlaceholder.IsVisible = false;
        }
    }

    private async void OnPickImageTapped(object sender, TappedEventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Select product image"
            });

            if (result == null) return;

            _imageStream = await result.OpenReadAsync();
            _imageFileName = result.FileName;

            ProductImagePreview.Source = ImageSource.FromStream(() =>
            {
                _imageStream.Position = 0;
                return _imageStream;
            });
            ProductImagePreview.IsVisible = true;
            ImagePlaceholder.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not pick image: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        // ── Validation ──────────────────────────────────────────────────
        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        { ShowError("Product name is required."); return; }

        if (CategoryPicker.SelectedIndex < 0)
        { ShowError("Please select a category."); return; }

        if (!double.TryParse(PriceEntry.Text, out double price) || price <= 0)
        { ShowError("Please enter a valid price."); return; }

        if (string.IsNullOrWhiteSpace(DescriptionEditor.Text))
        { ShowError("Product description is required."); return; }

        // ── Build product ────────────────────────────────────────────────
        var selectedCategory = _categories[CategoryPicker.SelectedIndex];
        string badge = BadgePicker.SelectedItem?.ToString() ?? "None";

        SaveBtn.IsEnabled = false;
        SaveBtn.Text = "Saving...";

        try
        {
            string imageUrl = _existingProduct?.imageUrl ?? "";

            // Upload new image if one was picked
            if (_imageStream != null)
            {
                _imageStream.Position = 0;
                imageUrl = await FirebaseAuthServices.Instance
                    .UploadProductImageAsync(_imageStream, _imageFileName);
            }

            var product = new Product
            {
                Id = _existingProduct?.Id ?? "",
                Name = NameEntry.Text.Trim(),
                categoryId = selectedCategory.Id,
                categoryName = selectedCategory.Name,
                Price = price,
                Description = DescriptionEditor.Text.Trim(),
                imageUrl = imageUrl,
                badge = badge == "None" ? "" : badge,
                available = AvailableSwitch.IsToggled
            };

            if (_isEditMode)
                await FirebaseAuthServices.Instance.UpdateProductAsync(product);
            else
                await FirebaseAuthServices.Instance.AddProductAsync(product);

            await DisplayAlert(
                "Success",
                _isEditMode ? "Product updated successfully." : "Product added successfully.",
                "OK");

            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to save product: {ex.Message}");
        }
        finally
        {
            SaveBtn.IsEnabled = true;
            SaveBtn.Text = _isEditMode ? "Save changes" : "Save product";
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

}