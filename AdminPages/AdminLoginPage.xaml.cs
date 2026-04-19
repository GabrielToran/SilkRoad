using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.AdminPages;

public partial class AdminLoginPage : ContentPage
{
	public AdminLoginPage()
	{
		InitializeComponent();
	}

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        ErrorLabel.IsVisible = false;

        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowError("Please enter your email and password.");
            return;
        }

        SignInBtn.IsEnabled = false;
        SignInBtn.Text = "Signing in...";

        try
        {
            await FirebaseAuthServices.Instance.SignInAsync(email, password);

            // Verify this account is actually an admin
            var profile = await FirebaseAuthServices.Instance
                .GetUserProfileAsync(FirebaseAuthServices.Instance.CurrentUserId!);

            if (profile == null || !profile.IsAdmin)
            {
                FirebaseAuthServices.Instance.SignOut();
                ShowError("Access denied. This account is not an admin.");
                return;
            }

            // Navigate to admin dashboard
            Application.Current!.MainPage =
                new NavigationPage(new AdminDashboardPage());
        }
        catch (Exception ex)
        {
            string msg = ex.Message.Contains("INVALID_PASSWORD") ||
                         ex.Message.Contains("EMAIL_NOT_FOUND")  ||
                         ex.Message.Contains("INVALID_LOGIN_CREDENTIALS")
                ? "Invalid email or password."
                : "Sign in failed. Please try again.";
            ShowError(msg);
        }
        finally
        {
            SignInBtn.IsEnabled = true;
            SignInBtn.Text = "Sign in to admin";
        }
    }

         private async void OnBackToStoreTapped(object sender, TappedEventArgs e)
            => await Navigation.PopAsync();

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
