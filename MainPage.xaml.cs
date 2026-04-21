namespace Multiplatoform_Project
{
    public partial class MainPage : ContentPage
    {


        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {

        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new SignInPage());


        }


        private async void OnSignUpClicked(object sender, EventArgs e)
        {

            await Navigation.PushAsync(new SignUpPage());

        }

        private async void OnAdminTapped(object sender, TappedEventArgs e)
          => await Navigation.PushAsync(new AdminPages.AdminLoginPage());
    }
}
