namespace SkyLock.Presentation;

public partial class HomePage : ContentPage
{
    private readonly int? playerAccountId;

    // Used when continuing as a guest.
    public HomePage() : this(null)
    {
    }

    public HomePage(int? accountId)
    {
        InitializeComponent();

        playerAccountId = accountId;
    }

    private async void OnStartFlightClicked(
        object? sender, EventArgs e)
    {
        if (!btnStartFlight.IsEnabled)
            return;

        btnStartFlight.IsEnabled = false;

        try
        {
            await Navigation.PushAsync(
                new DifficultyPage(playerAccountId));
        }
        finally
        {
            btnStartFlight.IsEnabled = true;
        }
    }

    private async void OnHowToPlayClicked(
        object? sender, EventArgs e)
    {
        if (!btnHowToPlay.IsEnabled)
            return;

        btnHowToPlay.IsEnabled = false;

        try
        {
            await Navigation.PushAsync(new HowToPlayPage());
        }
        finally
        {
            btnHowToPlay.IsEnabled = true;
        }
    }

    private async void OnFlightRecordsClicked(
        object? sender, EventArgs e)
    {
        if (!btnFlightRecords.IsEnabled)
            return;

        btnFlightRecords.IsEnabled = false;

        try
        {
            await Navigation.PushAsync(
                new FlightRecordsPage(playerAccountId));
        }
        finally
        {
            btnFlightRecords.IsEnabled = true;
        }
    }
}