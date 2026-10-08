namespace SkyLock.Presentation;

public partial class WelcomePage : ContentPage
{
	public WelcomePage()
	{
		InitializeComponent();
	}

    private bool isOpeningSignIn;

    private async void OnSignInClicked(object? sender, EventArgs e)
    {
        if (isOpeningSignIn)
            return;

        isOpeningSignIn = true;

        try
        {
            await Navigation.PushModalAsync(new SignInPage());
        }
        finally
        {
            isOpeningSignIn = false;
        }
    }

    private void OnGuestClicked(object? sender, EventArgs e)
    {
        if (Window is not null)
        {
            Window.Page = new NavigationPage(new HomePage());
        }
    }
}