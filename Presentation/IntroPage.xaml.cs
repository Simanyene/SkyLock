namespace SkyLock.Presentation;

public partial class IntroPage : ContentPage
{
    private bool hasOpenedLoadingPage;

    public IntroPage()
    {
        InitializeComponent();
    }

    private void OnIntroVideoEnded(object? sender, EventArgs e)
    {
        // Open the loading page once the video finishes.
        Dispatcher.Dispatch(() =>
        {
            if (hasOpenedLoadingPage || Window is null)
                return;

            hasOpenedLoadingPage = true;
            Window.Page = new LoadingPage();
        });
    }

    protected override void OnDisappearing()
    {
        medIntroVideo.Stop();

        base.OnDisappearing();
    }
}