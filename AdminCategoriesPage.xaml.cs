using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.AdminPages;

public partial class AdminCategoriesPage : ContentPage
{
    private List<Category> _categories = new();
    public bool IsRefreshing { get; set; }
    public AdminCategoriesPage()
	{
		InitializeComponent();
        BindingContext = this;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        LoadingIndicator.IsVisible = true;
        try
        {
            _categories = await FirebaseAuthService.Instance.GetCategoriesAsync();
            CategoriesCollection.ItemsSource = _categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not load categories: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            IsRefreshing = false;
            OnPropertyChanged(nameof(IsRefreshing));
        }
    }

    private async void OnAddCategoryClicked(object sender, EventArgs e)
    {
        string name = await DisplayPromptAsync(
            "Add Category",
            "Enter category name:",
            placeholder: "e.g. Rugs",
            maxLength: 40);

        if (string.IsNullOrWhiteSpace(name)) return;

        try
        {
            var cat = new Category { Name = name.Trim(), IconName = "", ProductCount = 0 };
            await FirebaseAuthService.Instance.AddCategoryAsync(cat);
            await LoadCategoriesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not add category: {ex.Message}", "OK");
        }
    }

    private async void OnEditCategoryClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn || btn.CommandParameter is not Category cat)
            return;

        string? newName = await DisplayPromptAsync(
            "Edit Category",
            "Update category name:",
            initialValue: cat.Name,
            maxLength: 40);

        if (string.IsNullOrWhiteSpace(newName) || newName.Trim() == cat.Name)
            return;

        try
        {
            cat.Name = newName.Trim();
            await FirebaseAuthService.Instance.UpdateCategoryAsync(cat);
            CategoriesCollection.ItemsSource = null;
            CategoriesCollection.ItemsSource = _categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not update category: {ex.Message}", "OK");
        }
    }

    private async void OnDeleteCategoryClicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton btn || btn.CommandParameter is not Category cat)
            return;

        bool confirm = await DisplayAlert(
            "Delete Category",
            $"Delete \"{cat.Name}\"? Products in this category will not be deleted but will become uncategorized.",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await FirebaseAuthService.Instance.DeleteCategoryAsync(cat.Id);
            _categories.Remove(cat);
            CategoriesCollection.ItemsSource = null;
            CategoriesCollection.ItemsSource = _categories;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Could not delete category: {ex.Message}", "OK");
        }
    }

    private async void OnRefreshed(object sender, EventArgs e)
        => await LoadCategoriesAsync();

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}