namespace Multiplatoform_Project;

public partial class SplashScreen : ContentPage
{
	public SplashScreen()
	{
		InitializeComponent();
	}


    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Task.Delay(3000);

        Application.Current.MainPage = new NavigationPage(new MainPage());
    }
}