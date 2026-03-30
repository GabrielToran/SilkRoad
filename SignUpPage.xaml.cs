using Multiplatoform_Project.Services;

namespace Multiplatoform_Project;

public partial class SignUpPage : ContentPage
{
    private FirebaseAuthService _authService = new FirebaseAuthService();
    public SignUpPage()
	{
		InitializeComponent();
	}

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await _authService.Signup(
                FirstNameEntry.Text, LastNameEntry.Text,
                EmailEntry.Text,
                PasswordEntry.Text
                );
            await SecureStorage.SetAsync("firebase_token", result);
            //go to another page

        }
        catch (Exception ex)
        {
            result.Text = ex.Message;


        }

    }

    private async void OnGoToSignInPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage());

    }

}