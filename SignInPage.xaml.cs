
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project;

public partial class SignInPage : ContentPage
{
    private FirebaseAuthServices _authService = new FirebaseAuthServices();
    public SignInPage()
    {
        InitializeComponent();
    }


    private async void OnSignInClicked(object sender, EventArgs e)
    {
        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            result.Text = "Please enter your email and password.";
            return;
        }


        try
        {
            await FirebaseAuthServices.Instance.SignInAsync(email, password);
            await SecureStorage.SetAsync("firebase_token", FirebaseAuthServices.Instance.IdToken ?? "");
            //go to another page
            Application.Current.MainPage = new NavigationPage(new HomePage());
        }
        catch (Exception ex)
        {
            result.Text = ex.Message;


        }

    }

    private async void OnTextClicked(object sender, EventArgs e)
    {

        await Navigation.PushAsync(new ResetPassword());
    }


    private async void OnGoToSignUpPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignUpPage());

    }
}