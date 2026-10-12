namespace SkyLock.Presentation;

public partial class ResultsPage : ContentPage
{
    private readonly string selectedDifficulty;
    private readonly int digitCount;
    private readonly int timeLimitSeconds;
    private readonly int? playerAccountId;

    private bool isNavigating;

    public ResultsPage(
        bool won,
        string difficulty,
        int digits,
        int seconds,
        int guesses,
        double elapsedSeconds,
        string secretCode,
        int? accountId,
        bool resultSaved)
    {
        InitializeComponent();

        selectedDifficulty = difficulty;
        digitCount = digits;
        timeLimitSeconds = seconds;
        playerAccountId = accountId;

        // Show the panel for this game's outcome.
        brdSuccessPanel.IsVisible = won;
        brdGameOverPanel.IsVisible = !won;

        double timeTaken = Math.Clamp(
            elapsedSeconds, 0, timeLimitSeconds);

        double timeRemaining = timeLimitSeconds - timeTaken;

        lblDifficulty.Text = $"Difficulty: {selectedDifficulty}";
        lblGuesses.Text = $"Guesses used: {guesses}";
        lblTimeTaken.Text =
            $"Time taken: {FormatDuration(timeTaken)}";
        lblTimeRemaining.Text =
            $"Time remaining: {FormatDuration(timeRemaining)}";
        lblSecretCode.Text = $"Secret code: {secretCode}";

        lblSaveStatus.IsVisible = !resultSaved;
        lblSaveStatus.Text =
            "This result could not be saved to Flight Records.";

        btnPlayAgain.Clicked += OnPlayAgainClicked;
        btnReturnHome.Clicked += OnReturnHomeClicked;
    }

    private static string FormatDuration(double seconds)
    {
        int totalSeconds = (int)Math.Floor(
            Math.Max(0, seconds));

        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }

    private async void OnPlayAgainClicked(
        object? sender, EventArgs e)
    {
        if (isNavigating)
            return;

        SetNavigationBusy(true);

        try
        {
            GameplayPage nextFlight = new GameplayPage(
                selectedDifficulty,
                digitCount,
                timeLimitSeconds,
                playerAccountId);

            // Replace Results with a fresh game.
            Navigation.InsertPageBefore(nextFlight, this);
            await Navigation.PopAsync();
        }
        finally
        {
            SetNavigationBusy(false);
        }
    }

    private async void OnReturnHomeClicked(
        object? sender, EventArgs e)
    {
        if (isNavigating)
            return;

        SetNavigationBusy(true);

        try
        {
            // Home is the first page in our NavigationPage.
            await Navigation.PopToRootAsync();
        }
        finally
        {
            SetNavigationBusy(false);
        }
    }

    private void SetNavigationBusy(bool busy)
    {
        isNavigating = busy;
        btnPlayAgain.IsEnabled = !busy;
        btnReturnHome.IsEnabled = !busy;
    }

    protected override bool OnBackButtonPressed()
    {
        if (isNavigating)
            return true;

        return base.OnBackButtonPressed();
    }
}