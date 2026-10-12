using SkyLock.Models;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class DifficultyPage : ContentPage
{
    private readonly int? playerAccountId;

    private string selectedDifficulty = "Easy";
    private int digitCount = 3;
    private int timeLimitSeconds = 420;

    // Used when no account ID is supplied.
    public DifficultyPage() : this(null)
    {
    }

    public DifficultyPage(int? accountId)
    {
        InitializeComponent();

        playerAccountId = accountId;

        switch (SettingsService.DefaultDifficulty)
        {
            case GameDifficulty.Medium:
                SelectDifficulty("Medium", 4, 240);
                break;

            case GameDifficulty.Hard:
                SelectDifficulty("Hard", 5, 120);
                break;

            default:
                SelectDifficulty("Easy", 3, 420);
                break;
        }
    }

    private void OnEasyTapped(object? sender, TappedEventArgs e)
    {
        SelectDifficulty("Easy", 3, 240);
    }

    private void OnMediumTapped(object? sender, TappedEventArgs e)
    {
        SelectDifficulty("Medium", 4, 180);
    }

    private void OnHardTapped(object? sender, TappedEventArgs e)
    {
        SelectDifficulty("Hard", 5, 120);
    }

    private void SelectDifficulty(
        string difficulty, int digits, int seconds)
    {
        selectedDifficulty = difficulty;
        digitCount = digits;
        timeLimitSeconds = seconds;

        VisualStateManager.GoToState(
            brdEasy,
            difficulty == "Easy" ? "Selected" : "Normal");

        VisualStateManager.GoToState(
            brdMedium,
            difficulty == "Medium" ? "Selected" : "Normal");

        VisualStateManager.GoToState(
            brdHard,
            difficulty == "Hard" ? "Selected" : "Normal");
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
                new GameplayPage(
                    selectedDifficulty,
                    digitCount,
                    timeLimitSeconds,
                    playerAccountId));
        }
        finally
        {
            btnStartFlight.IsEnabled = true;
        }
    }

    private async void OnBackClicked(
        object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}