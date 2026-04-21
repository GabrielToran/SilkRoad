using Multiplatoform_Project.Models;
using Multiplatoform_Project.Services;

namespace Multiplatoform_Project.Pages;

public partial class EditProfile : ContentPage
{
    private UserProfile? _profile;
    public EditProfile()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadProfileAsync();
    }

    private async Task LoadProfileAsync()
    {
        string? userId = FirebaseAuthServices.Instance.CurrentUserId;
        if (userId == null) return;

        _profile = await FirebaseAuthServices.Instance.GetUserProfileAsync(userId);
        if (_profile == null) return;

        // Populate fields
        FirstNameEntry.Text = _profile.FirstName;
        LastNameEntry.Text = _profile.LastName;
        EmailEntry.Text = _profile.Email;
        PhoneEntry.Text = _profile.Phone;
        AddressEntry.Text = _profile.ShippingAddress;

        // Avatar
        InitialsLabel.Text = _profile.Initials;
        FullNameLabel.Text = _profile.FullName;
        MemberSinceLabel.Text = _profile.MemberSinceFormatted;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_profile == null) return;

        if (string.IsNullOrWhiteSpace(FirstNameEntry.Text) ||
            string.IsNullOrWhiteSpace(LastNameEntry.Text))
        {
            await DisplayAlert("Required", "First and last name are required.", "OK");
            return;
        }

        SaveBtn.IsEnabled = false;
        SaveBtn.Text = "Saving...";

        try
        {
            _profile.FirstName = FirstNameEntry.Text.Trim();
            _profile.LastName = LastNameEntry.Text.Trim();
            _profile.Email = EmailEntry.Text.Trim();
            _profile.Phone = PhoneEntry.Text.Trim();
            _profile.ShippingAddress = AddressEntry.Text.Trim();

            await FirebaseAuthServices.Instance.UpdateUserProfileAsync(_profile);

            // Refresh avatar
            InitialsLabel.Text = _profile.Initials;
            FullNameLabel.Text = _profile.FullName;

            await DisplayAlert("Saved", "Your profile has been updated.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            SaveBtn.IsEnabled = true;
            SaveBtn.Text = "Save changes";
        }
    }

    private async void OnChangePasswordTapped(object sender, TappedEventArgs e)
    {
        string email = _profile?.Email ?? "";
        bool confirm = await DisplayAlert(
            "Reset Password",
            $"Send a password reset email to {email}?",
            "Send", "Cancel");

        if (confirm && !string.IsNullOrEmpty(email))
        {
            try
            {
                await FirebaseAuthServices.Instance.SendPasswordResetAsync(email);
                await DisplayAlert("Email Sent",
                    "Check your inbox for the password reset link.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
        => await Navigation.PopAsync();
}
