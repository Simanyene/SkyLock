
using System;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;
using SkyLock.Models;

namespace SkyLock.Presentation;

public partial class GameplayPage : ContentPage
{
    // Game settings
    private readonly string selectedDifficulty;
    private readonly int digitCount;
    private readonly int timeLimitSeconds;
    private readonly int? playerAccountId;

    // Timer and flight clock
    private readonly IDispatcherTimer gameTimer;
    private readonly Stopwatch flightClock = new();

    // Digit controls
    private readonly Label[] digitLabels;
    private readonly Border[] digitBorders;
    private readonly Brush[] defaultBorderBrushes;

    // Correct digits are green on every difficulty.
    private static readonly Color LockedGreen =
        Color.FromArgb("#2E7D32");

    private string secretCode = "";

    // Current guess and locked positions
    private char[] guessSlots = Array.Empty<char>();
    private bool[] lockedSlots = Array.Empty<bool>();

    private int guessCount;
    private bool gameStarted;
    private bool gameFinished;
    private bool isPaused;
    private bool isShowingExitPopup;

    // Default game
    public GameplayPage() : this("Easy", 3, 420)
    {
    }

    // Game with selected difficulty
    public GameplayPage(
        string difficulty,
        int digits,
        int seconds,
        int? accountId = null)
    {
        InitializeComponent();

        selectedDifficulty = difficulty;
        digitCount = Math.Clamp(digits, 3, 5);
        timeLimitSeconds = Math.Max(1, seconds);
        playerAccountId = accountId;

        guessSlots = new char[digitCount];
        lockedSlots = new bool[digitCount];

        // Connect the digit labels.
        digitLabels = new[]
        {
            lblDigitOne,
            lblDigitTwo,
            lblDigitThree,
            lblDigitFour,
            lblDigitFive
        };

        // Connect the digit borders.
        digitBorders = new[]
        {
            brdDigitOne,
            brdDigitTwo,
            brdDigitThree,
            brdDigitFour,
            brdDigitFive
        };

        // Remember the original border backgrounds.
        defaultBorderBrushes = digitBorders
            .Select(border => border.Background)
            .ToArray();

        // Display the correct number of boxes.
        brdDigitFour.IsVisible = digitCount >= 4;
        brdDigitFive.IsVisible = digitCount >= 5;

        lblCodeInstruction.Text =
            $"ENTER {digitCount}-DIGIT OVERRIDE";

        // Create number buttons from 0 to 9.
        CreateNumberButtons();

        // Set up the timer.
        gameTimer = Dispatcher.CreateTimer();

        gameTimer.Interval =
            TimeSpan.FromMilliseconds(200);

        gameTimer.Tick += OnGameTimerTick;
    }

    // Create the number keypad.
    private void CreateNumberButtons()
    {
        for (int digit = 0; digit <= 9; digit++)
        {
            Button numberButton = new Button
            {
                Text = digit.ToString(),
                TextColor = Colors.White,
                StyleId = digit.ToString(),

                Style = (Style)Application.Current!
                    .Resources["WelcomeGuestStyle"]
            };

            numberButton.Clicked += OnNumberClicked;

            int row = digit == 0
                ? 3
                : (digit - 1) / 3;

            int column = digit == 0
                ? 1
                : (digit - 1) % 3;

            Grid.SetRow(numberButton, row);
            Grid.SetColumn(numberButton, column);

            grdKeypad.Children.Add(numberButton);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!gameStarted)
        {
            StartNewGame();
        }
    }

    // Start a new flight.
    private void StartNewGame()
    {
        StopFlightTimer();
        flightClock.Reset();

        gameStarted = true;
        gameFinished = false;
        isPaused = false;
        isShowingExitPopup = false;

        guessCount = 0;

        ClearGuessSlots();
        GenerateSecretCode();
        UpdateDigitBoxes();

        lblHits.Text = "HITS: —";
        lblMatches.Text = "MATCHES: —";
        lblGuessCount.Text = "Guesses: 0";

        lblGuessHistory.Text =
            "Your guesses will appear here.";

        lblGameMessage.Text =
            $"{selectedDifficulty}: Enter " +
            $"{digitCount} different digits.";

        btnPause.Text = "Pause";
        btnPause.IsEnabled = true;
        btnBack.IsEnabled = true;
        grdKeypad.IsEnabled = true;

        flightClock.Start();
        gameTimer.Start();

        UpdateFlightDisplay();
    }

    // Reset all digit positions.
    private void ClearGuessSlots()
    {
        Array.Clear(guessSlots);
        Array.Clear(lockedSlots);
    }

    // Generate a secret code with unique digits.
    private void GenerateSecretCode()
    {
        secretCode =
            Random.Shared.Next(1, 10).ToString();

        while (secretCode.Length < digitCount)
        {
            string nextDigit =
                Random.Shared.Next(0, 10).ToString();

            if (!secretCode.Contains(nextDigit))
            {
                secretCode += nextDigit;
            }
        }
    }

    // Check whether the player can enter digits.
    private bool CanPlay()
    {
        if (gameFinished || isPaused ||
            isShowingExitPopup)
        {
            return false;
        }

        if (flightClock.Elapsed.TotalSeconds >=
            timeLimitSeconds)
        {
            FinishGame(false);
            return false;
        }

        return true;
    }

    // Find the next empty unlocked position.
    private int NextOpenSlot()
    {
        for (int index = 0;
             index < digitCount;
             index++)
        {
            if (!lockedSlots[index] &&
                guessSlots[index] == '\0')
            {
                return index;
            }
        }

        return -1;
    }

    // Handle number button clicks.
    private void OnNumberClicked(
        object? sender,
        EventArgs e)
    {
        if (!CanPlay() ||
            sender is not Button numberButton)
        {
            return;
        }

        if (string.IsNullOrEmpty(numberButton.StyleId))
            return;

        char selectedDigit = numberButton.StyleId[0];

        int slot = NextOpenSlot();

        if (slot < 0)
            return;

        // Prevent repeated digits.
        if (Array.IndexOf(
                guessSlots,
                selectedDigit) >= 0)
        {
            lblGameMessage.Text =
                "Each digit must be different.";
            return;
        }

        // First digit cannot be zero.
        if (slot == 0 && selectedDigit == '0')
        {
            lblGameMessage.Text =
                "The first digit cannot be zero.";
            return;
        }

        guessSlots[slot] = selectedDigit;

        lblGameMessage.Text =
            "Press Enter to check your code.";

        UpdateDigitBoxes();
    }

    // Delete the last unlocked digit.
    private void OnDeleteClicked(
        object? sender,
        EventArgs e)
    {
        if (!CanPlay())
            return;

        for (int index = digitCount - 1;
             index >= 0;
             index--)
        {
            if (!lockedSlots[index] &&
                guessSlots[index] != '\0')
            {
                guessSlots[index] = '\0';

                UpdateDigitBoxes();
                return;
            }
        }
    }

    // Update the digit boxes.
    private void UpdateDigitBoxes()
    {
        for (int index = 0;
             index < digitCount;
             index++)
        {
            bool isLocked = lockedSlots[index];

            Label digitLabel = digitLabels[index];
            Border digitBorder = digitBorders[index];

            digitLabel.Text =
                guessSlots[index] == '\0'
                    ? "_"
                    : guessSlots[index].ToString();

            // All digit text stays white.
            digitLabel.TextColor = Colors.White;

            // Correct positions turn green.
            digitBorder.Background = isLocked
                ? new SolidColorBrush(LockedGreen)
                : defaultBorderBrushes[index];
        }
    }

    // Check the entered code.
    private void OnEnterClicked(
        object? sender,
        EventArgs e)
    {
        if (!CanPlay())
            return;

        // Make sure every position has a digit.
        if (NextOpenSlot() != -1)
        {
            lblGameMessage.Text =
                $"Please enter all {digitCount} digits.";
            return;
        }

        int hits = 0;
        int matches = 0;

        bool[] hitPositions =
            new bool[digitCount];

        // Compare the guess with the secret code.
        for (int index = 0;
             index < digitCount;
             index++)
        {
            if (guessSlots[index] ==
                secretCode[index])
            {
                hits++;
                hitPositions[index] = true;
            }
            else if (secretCode.Contains(
                guessSlots[index]))
            {
                matches++;
            }
        }

        guessCount++;

        lblHits.Text = $"HITS: {hits}";
        lblMatches.Text = $"MATCHES: {matches}";

        lblGuessCount.Text =
            $"Guesses: {guessCount}";

        string guessText =
            string.Concat(guessSlots);

        string guessRecord =
            $"{guessCount}. {guessText} — " +
            $"{hits} hits, {matches} matches";

        // Add the latest guess to the history.
        lblGuessHistory.Text =
            guessCount == 1
                ? guessRecord
                : guessRecord +
                  Environment.NewLine +
                  lblGuessHistory.Text;

        // All positions are correct.
        if (hits == digitCount)
        {
            // Highlight all correct positions.
            for (int index = 0;
                 index < digitCount;
                 index++)
            {
                lockedSlots[index] = true;
            }

            UpdateDigitBoxes();
            FinishGame(true);
            return;
        }

        // Lock correctly positioned digits.
        // This works for Easy, Medium and Hard.
        for (int index = 0;
             index < digitCount;
             index++)
        {
            if (hitPositions[index])
            {
                lockedSlots[index] = true;
            }

            // Clear digits that are not correct.
            if (!lockedSlots[index])
            {
                guessSlots[index] = '\0';
            }
        }

        UpdateDigitBoxes();

        if (hits > 0)
        {
            lblGameMessage.Text =
                $"{hits} correct position(s) " +
                "locked in green. Keep guessing!";
        }
        else
        {
            lblGameMessage.Text =
                "Code rejected. Try another code.";
        }
    }

    // Update the countdown timer.
    private void OnGameTimerTick(
        object? sender,
        EventArgs e)
    {
        if (gameFinished || isPaused)
            return;

        UpdateFlightDisplay();

        if (flightClock.Elapsed.TotalSeconds >=
            timeLimitSeconds)
        {
            FinishGame(false);
        }
    }

    // Update altitude and time.
    private void UpdateFlightDisplay()
    {
        double remainingSeconds = Math.Max(
            0,
            timeLimitSeconds -
            flightClock.Elapsed.TotalSeconds);

        int displayedSeconds =
            (int)Math.Ceiling(remainingSeconds);

        TimeSpan remainingTime =
            TimeSpan.FromSeconds(displayedSeconds);

        lblTimeLeft.Text =
            remainingTime.ToString(@"mm\:ss");

        int altitude = 1200 + (int)(
            8800 *
            remainingSeconds /
            timeLimitSeconds);

        lblAltitude.Text =
            $"{altitude:N0} FT";

        // Show warning during the final 30 seconds.
        cvTimeWarning.UpdateTime(
            gameFinished
                ? 0
                : displayedSeconds);
    }

    // Pause the flight.
    private void PauseFlight()
    {
        if (gameFinished)
            return;

        isPaused = true;

        flightClock.Stop();
        gameTimer.Stop();

        btnPause.Text = "Resume";
        grdKeypad.IsEnabled = false;

        lblGameMessage.Text =
            "Flight paused.";
    }

    // Resume the flight.
    private void ResumeFlight()
    {
        if (gameFinished)
            return;

        isPaused = false;

        flightClock.Start();
        gameTimer.Start();

        btnPause.Text = "Pause";
        grdKeypad.IsEnabled = true;

        lblGameMessage.Text =
            "Flight resumed.";
    }

    // Handle Pause / Resume button.
    private void OnPauseClicked(
        object? sender,
        EventArgs e)
    {
        if (gameFinished || isShowingExitPopup)
            return;

        if (isPaused)
        {
            ResumeFlight();
        }
        else
        {
            PauseFlight();
        }
    }

    // Show confirmation before exiting the game.
    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        await ConfirmExitAsync();
    }

    private async Task ConfirmExitAsync()
    {
        if (isShowingExitPopup || gameFinished)
            return;

        isShowingExitPopup = true;

        bool wasPaused = isPaused;

        // Stop the timer while the popup is open.
        if (!wasPaused)
        {
            PauseFlight();
        }

        try
        {
            bool exitFlight = await DisplayAlert(
                "Exit Flight?",
                "Your current flight is unfinished. " +
                "Do you want to exit without saving?",
                "Exit Flight",
                "Continue Playing");

            if (exitFlight)
            {
                gameFinished = true;

                StopFlightTimer();
                cvTimeWarning.UpdateTime(0);

                if (Navigation.NavigationStack.Count > 1)
                {
                    await Navigation.PopAsync();
                }
                else if (Window is not null)
                {
                    Window.Page =
                        new HomePage(playerAccountId);
                }
            }
            else if (!wasPaused)
            {
                ResumeFlight();
            }
        }
        finally
        {
            isShowingExitPopup = false;
        }
    }

    // Stop the flight timer.
    private void StopFlightTimer()
    {
        flightClock.Stop();
        gameTimer?.Stop();
    }

    // Finish the game and save the result.
    private async void FinishGame(bool won)
    {
        if (gameFinished)
            return;

        gameFinished = true;

        StopFlightTimer();
        cvTimeWarning.UpdateTime(0);

        grdKeypad.IsEnabled = false;
        btnPause.IsEnabled = false;
        btnBack.IsEnabled = false;

        lblGameMessage.Text =
            won
                ? "Code accepted! Landing successful."
                : "Time is up! Flight failed.";

        double elapsedSeconds = Math.Clamp(
            flightClock.Elapsed.TotalSeconds,
            0,
            timeLimitSeconds);

        FlightRecord flightRecord = new FlightRecord
        {
            PlayerAccountId = playerAccountId,
            Difficulty = selectedDifficulty,
            DigitCount = digitCount,
            TimeLimitSeconds = timeLimitSeconds,
            GuessCount = guessCount,
            TimeTakenSeconds = elapsedSeconds,
            Won = won,
            PlayedAtUtc = DateTime.UtcNow
        };

        bool resultSaved = false;

        try
        {
            // Save the completed flight.
            await App.FlightRecordsService
                .AddRecordAsync(flightRecord);

            resultSaved = true;
        }
        catch (Exception exception)
        {
            Debug.WriteLine(
                $"Could not save flight: {exception}");
        }

        // Open the Results Page.
        ResultsPage resultsPage = new ResultsPage(
            won,
            selectedDifficulty,
            digitCount,
            timeLimitSeconds,
            guessCount,
            elapsedSeconds,
            secretCode,
            playerAccountId,
            resultSaved);

        if (Navigation.NavigationStack.Count > 0)
        {
            await Navigation.PushAsync(resultsPage);

            if (Navigation.NavigationStack.Contains(this))
            {
                Navigation.RemovePage(this);
            }
        }
        else if (Window is not null)
        {
            Window.Page = new NavigationPage(resultsPage);
        }
    }

    // Handle the Android Back button.
    protected override bool OnBackButtonPressed()
    {
        if (gameFinished)
            return true;

        MainThread.BeginInvokeOnMainThread(
            async () => await ConfirmExitAsync());

        return true;
    }

    // Stop the timer when leaving the page.
    protected override void OnDisappearing()
    {
        StopFlightTimer();
        cvTimeWarning.UpdateTime(0);

        if (gameStarted && !gameFinished)
        {
            isPaused = true;

            btnPause.Text = "Resume";
            grdKeypad.IsEnabled = false;

            lblGameMessage.Text =
                "Flight paused.";
        }

        base.OnDisappearing();
    }
}
