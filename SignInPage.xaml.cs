
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project;

public partial class SignInPage : ContentPage
{
    private FirebaseAuthService _authService = new FirebaseAuthService();
    public SignInPage()
	{
		InitializeComponent();
	}


    private async void OnSignInClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _authService.SignIn(
                EmailEntry.Text,
                PasswordEntry.Text
                );
            await SecureStorage.SetAsync("firebase_token", result);
            //go to another page
            Application.Current.MainPage = new NavigationPage(new HomePage(EmailEntry.Text));
        }
        catch (Exception ex)
        {
            result.Text = ex.Message;


        }

    }

    private async void OnTextClicked(object sender, EventArgs e) { 
    
        await Navigation.PushAsync(new ResetPassword());
    }


    private async void OnGoToSignUpPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());

    }
}