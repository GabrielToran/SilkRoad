using Multiplatoform_Project.Services;



namespace Multiplatoform_Project;

public partial class ResetPassword : ContentPage
{
	public ResetPassword()
	{
		InitializeComponent();
	}

	private async void OnSendClicked(object sender, EventArgs e)
	{
		ErrorLabel.IsVisible = false;
		SuccessLabel.IsVisible = false;

		string email = EmailEntry.Text?.Trim() ?? "";

		if (string.IsNullOrEmpty(email))
		{
			ErrorLabel.Text = "Please enter your email address";
			ErrorLabel.IsVisible = true;
			return;

		}

		SendBtn.IsEnabled = false;
		SendBtn.Text = "Sending...";

		try
		{
			await FirebaseAuthServices.Instance.SendPasswordResetAsync(email);

			SuccessLabel.Text = $"Reset link sent to {email}.\nCheck your inbox.";

			SuccessLabel.IsVisible = true;
			EmailEntry.Text = "  ";

		}
		catch(Exception ex)
		{
			string msg = ex.Message.Contains("EMAIL_NOT_FOUND")
				? "No account found with this email."
				: "Something went wrong. Please try again.";
			ErrorLabel.Text = msg;
			ErrorLabel.IsVisible = true;


		}
		finally
		{
			SendBtn.IsEnabled = true;
			SendBtn.Text = "Send reset link";

		}
	}

        private async void OnBackClicked(object sender, TappedEventArgs e)
        => await Navigation.PopAsync();
}
