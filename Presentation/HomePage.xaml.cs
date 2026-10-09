
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

    // Show or hide the side menu.
    private void SetMenuVisible(bool visible)
    {
        grdMenuOverlay.IsVisible = visible;
        grdHomeContent.IsEnabled = !visible;
    }

    // Open the side menu.
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

    // Close the side menu.
    private void OnMenuCloseRequested(
        object? sender,
        EventArgs e)
    {
        SetMenuVisible(false);
    }

    // Close the menu when tapping outside it.
    private void OnMenuBackdropTapped(
        object? sender,
        TappedEventArgs e)
    {
        SetMenuVisible(false);
    }

    // Navigate to another page.
    private async Task OpenPageAsync(
        Func<Page> createPage)
    {
        if (isNavigating)
            return;

        isNavigating = true;
        SetMenuVisible(false);

        try
        {
            if (Navigation.NavigationStack.Count > 0)
            {
                await Navigation.PushAsync(createPage());
            }
            else if (Window is not null)
            {
                Window.Page = new NavigationPage(createPage());
            }
        }
        finally
        {
            isNavigating = false;
        }
    }

    // Start Flight button.
    private async void OnStartFlightClicked(
        object? sender,
        EventArgs e)
    {
        await OpenPageAsync(
            () => new DifficultyPage(playerAccountId));
    }

    // How to Play button.
    private async void OnHowToPlayClicked(
        object? sender,
        EventArgs e)
    {
        await OpenPageAsync(
            () => new HowToPlayPage());
    }

    // Back button.
    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await GoBackAsync();
    }

    // Return to the previous page, or Welcome Page.
    private async Task GoBackAsync()
    {
        if (isNavigating)
            return;

        if (grdMenuOverlay.IsVisible)
        {
            SetMenuVisible(false);
            return;
        }

        isNavigating = true;

        try
        {
            if (Navigation.NavigationStack.Count > 1)
            {
                await Navigation.PopAsync();
            }
            else if (Window is not null)
            {
                Window.Page = new NavigationPage(
                    new WelcomePage());
            }
        }
        finally
        {
            isNavigating = false;
        }
    }

    // Handle side menu selections.
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

            case "SignOut":
                SetMenuVisible(false);

                if (Window is not null)
                {
                    Window.Page = new NavigationPage(
                        new WelcomePage());
                }
                break;
        }
    }

    // Android physical Back button.
    protected override bool OnBackButtonPressed()
    {
        if (grdMenuOverlay.IsVisible)
        {
            SetMenuVisible(false);
            return true;
        }

        MainThread.BeginInvokeOnMainThread(
            async () => await GoBackAsync());

        return true;
    }
}
