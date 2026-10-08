using SkyLock.Models;

namespace SkyLock.Presentation;

public partial class FlightRecordsPage : ContentPage
{
    private readonly int? playerAccountId;
    private List<FlightRecord> flightRecords = new();
    private bool isLoading;

    public FlightRecordsPage() : this(null)
    {
    }

    public FlightRecordsPage(int? accountId)
    {
        InitializeComponent();

        playerAccountId = accountId;
        pckDifficulty.SelectedIndex = 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (isLoading)
            return;

        isLoading = true;
        pckDifficulty.IsEnabled = false;
        clvFlightRecords.IsVisible = false;
        lblStatistics.Text = "Loading records...";

        try
        {
            flightRecords =
                await App.FlightRecordsService.GetRecordsAsync(
                    playerAccountId);

            UpdateRecordsDisplay();

            clvFlightRecords.IsVisible = true;
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            lblStatistics.Text =
                "Your records could not be loaded. " +
                "Return Home and try again.";
        }
        finally
        {
            pckDifficulty.IsEnabled = true;
            isLoading = false;
        }
    }

    private void OnDifficultyChanged(
        object? sender, EventArgs e)
    {
        if (isLoading)
            return;

        UpdateRecordsDisplay();
    }

    private void UpdateRecordsDisplay()
    {
        string difficulty =
            pckDifficulty.SelectedItem as string ?? "Easy";

        List<FlightRecord> selectedRecords = flightRecords
            .Where(record => record.Difficulty == difficulty)
            .OrderByDescending(record => record.PlayedAtUtc)
            .ToList();

        List<FlightRecord> winningRecords = selectedRecords
            .Where(record => record.Won)
            .ToList();

        int gamesPlayed = selectedRecords.Count;
        int gamesWon = winningRecords.Count;
        int gamesLost = gamesPlayed - gamesWon;

        string fastestWin = "—";
        string fewestGuesses = "—";
        string averageGuesses = "—";

        // Only wins count towards these performance records.
        if (winningRecords.Count > 0)
        {
            double fastestSeconds = winningRecords.Min(
                record => record.TimeTakenSeconds);

            fastestWin = FormatDuration(fastestSeconds);

            fewestGuesses = winningRecords.Min(
                record => record.GuessCount).ToString();

            averageGuesses = winningRecords.Average(
                record => record.GuessCount).ToString("0.0");
        }

        lblStatistics.Text =
            $"Games played: {gamesPlayed}\n" +
            $"Won: {gamesWon}   Lost: {gamesLost}\n\n" +
            $"Fastest win: {fastestWin}\n" +
            $"Fewest guesses in a win: {fewestGuesses}\n" +
            $"Average guesses in wins: {averageGuesses}";

        List<string> recordDescriptions = new();

        foreach (FlightRecord record in selectedRecords)
        {
            string outcome = record.Won
                ? "CODE ACCEPTED"
                : "GAME OVER";

            string playedAt = record.PlayedAtUtc
                .ToLocalTime()
                .ToString("dd MMM yyyy, HH:mm");

            string duration =
                FormatDuration(record.TimeTakenSeconds);

            recordDescriptions.Add(
                $"{outcome}\n" +
                $"{playedAt}\n" +
                $"{record.Difficulty} • {record.DigitCount} digits\n" +
                $"Guesses: {record.GuessCount} • Time: {duration}");
        }

        clvFlightRecords.ItemsSource = recordDescriptions;
    }

    private static string FormatDuration(double seconds)
    {
        int totalSeconds = (int)Math.Ceiling(
            Math.Max(0, seconds));

        int minutes = totalSeconds / 60;
        int remainingSeconds = totalSeconds % 60;

        return $"{minutes:00}:{remainingSeconds:00}";
    }

    private async void OnBackClicked(
        object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}