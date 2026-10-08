namespace SkyLock.Presentation;

public partial class LoadingPage : ContentPage
{
    private readonly IDispatcherTimer loadingTimer;
    private int loadingPercentage;

    public LoadingPage()
    {
        InitializeComponent();

        loadingTimer = Dispatcher.CreateTimer();
        loadingTimer.Interval = TimeSpan.FromMilliseconds(40);
        loadingTimer.IsRepeating = true;
        loadingTimer.Tick += OnLoadingTimerTick;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        loadingPercentage = 0;
        lblLoadingStatus.Text = "Loading... 0%";
        bxvLoadingFill.WidthRequest = 0;

        loadingTimer.Start();
    }

    private void OnLoadingTimerTick(object? sender, EventArgs e)
    {
        // Wait until the loading track has been measured.
        if (grdLoadingTrack.Width <= 0)
            return;

        // Open the welcome page after displaying 100%.
        if (loadingPercentage >= 100)
        {
            loadingTimer.Stop();

            if (Window is not null)
                Window.Page = new WelcomePage();

            return;
        }

        loadingPercentage++;

        lblLoadingStatus.Text = $"Loading... {loadingPercentage}%";

        bxvLoadingFill.WidthRequest =
            grdLoadingTrack.Width * loadingPercentage / 100.0;
    }

    protected override void OnDisappearing()
    {
        // Stop updating controls when this page closes.
        loadingTimer.Stop();

        base.OnDisappearing();
    }
}