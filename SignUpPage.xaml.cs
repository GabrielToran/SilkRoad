using Multiplatoform_Project.Services;

namespace Multiplatoform_Project;

public partial class SignUpPage : ContentPage
{
    private FirebaseAuthServices _authService = new FirebaseAuthServices();
    public SignUpPage()
    {
        InitializeComponent();
    }

    private async void OnSignUpClicked(object sender, EventArgs e)
    {
        string firstName = FirstNameEntry.Text?.Trim() ?? "";
        string lastName = LastNameEntry.Text?.Trim() ?? "";
        string email = EmailEntry.Text?.Trim() ?? "";
        string password = PasswordEntry.Text ?? "";

        if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) ||
            string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            result.Text = "Please fill in all fields.";
            return;
        }




        try
        {
            await FirebaseAuthServices.Instance.SignUpAsync(firstName, lastName, email, password);
            await SecureStorage.SetAsync("firebase_token", FirebaseAuthServices.Instance.IdToken ?? "");
            //go to another page
            Application.Current!.MainPage = new NavigationPage(new HomePage());

        }
        catch (Exception ex)
        {
            result.Text = ex.Message.Contains("INVALID_LOGIN_CREDENTIALS") ||
                          ex.Message.Contains("INVALID_PASSWORD") ||
                          ex.Message.Contains("EMAIL_NOT_FOUND")
                ? "Invalid email or password."
                : ex.Message;


        }

    }

    private async void OnGoToSignInPage(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new SignInPage());

    }

}