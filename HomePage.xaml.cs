

namespace Multiplatoform_Project;

public partial class HomePage : ContentPage
{
	public HomePage(string email)
	{
		InitializeComponent();
        EmailLabel.Text = $"Logged as {email}";
    }

    private async void OnLogOutClicked(object sender, EventArgs e)
    {
        SecureStorage.Remove("firebase_token");
        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}