namespace SkyLock.Presentation;

public partial class HomePage : ContentPage
{
    private readonly int? playerAccountId;
    private bool isNavigating;

    // Guest Home page
    public HomePage() : this(null)
    {
    }

    // Signed-in Home page
    public HomePage(int? accountId)
    {
        InitializeComponent();

        playerAccountId = accountId;
    }

    private void SetMenuVisible(bool visible)
    {
        grdMenuOverlay.IsVisible = visible;
        grdHomeContent.IsEnabled = !visible;
    }

    private async void OnOpenMenuClicked(
        object? sender,
        EventArgs e)
    {
        if (isNavigating)
            return;

        await sideMenuView.LoadProfileAsync(
            playerAccountId);

        SetMenuVisible(true);
    }

    private void OnMenuCloseRequested(
        object? sender,
        EventArgs e)
    {
        SetMenuVisible(false);
    }

    private void OnMenuBackdropTapped(
        object? sender,
        TappedEventArgs e)
    {
        SetMenuVisible(false);
    }

    private async Task OpenPageAsync(
        Func<Page> createPage)
    {
        if (isNavigating)
            return;

        isNavigating = true;
        SetMenuVisible(false);

        try
        {
            await Navigation.PushAsync(createPage());
        }
        finally
        {
            isNavigating = false;
        }
    }

    private async void OnStartFlightClicked(
        object? sender,
        EventArgs e)
    {
        await OpenPageAsync(
            () => new DifficultyPage(playerAccountId));
    }

    private async void OnHowToPlayClicked(
        object? sender,
        EventArgs e)
    {
        await OpenPageAsync(
            () => new HowToPlayPage());
    }

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        if (isNavigating)
            return;

        await Navigation.PopAsync();
    }

    private async void OnMenuItemSelected(
        object? sender,
        string destination)
    {
        if (isNavigating)
            return;

        switch (destination)
        {
            case "Home":
                SetMenuVisible(false);
                break;

            case "StartFlight":
                await OpenPageAsync(
                    () => new DifficultyPage(playerAccountId));
                break;

            case "HowToPlay":
                await OpenPageAsync(
                    () => new HowToPlayPage());
                break;

            case "FlightRecords":
                await OpenPageAsync(
                    () => new FlightRecordsPage(playerAccountId));
                break;

            case "Profile":
                await OpenPageAsync(
                    () => new ProfilePage(playerAccountId));
                break;

            case "Settings":
                await OpenPageAsync(
                    () => new SettingsPage(playerAccountId));
                break;

           // case "Chat":
             //   await OpenPageAsync(
                   // () => new ChatPage());
              //  break;

            case "SignOut":
                if (Window is not null)
                {
                    Window.Page = new WelcomePage();
                }
                break;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        if (grdMenuOverlay.IsVisible)
        {
            SetMenuVisible(false);
            return true;
        }

        return base.OnBackButtonPressed();
    }
}