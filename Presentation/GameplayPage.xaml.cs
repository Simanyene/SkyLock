using System.Diagnostics;
using Microsoft.Maui.Dispatching;
using SkyLock.Models;

namespace SkyLock.Presentation;

public partial class GameplayPage : ContentPage
{
    private readonly string selectedDifficulty;
    private readonly int digitCount;
    private readonly int timeLimitSeconds;
    private readonly int? playerAccountId;

    private readonly IDispatcherTimer gameTimer;
    private readonly Stopwatch flightClock = new();

    private readonly Label[] digitLabels;

    private string secretCode = "";
    private string currentGuess = "";

    private int guessCount;
    private bool gameStarted;
    private bool gameFinished;
    private bool isPaused;

    // Default: Easy, 3 digits, 7 minutes.
    public GameplayPage() : this("Easy", 3, 420)
    {
    }

    public GameplayPage(
        string difficulty,
        int digits,
        int seconds,
        int? accountId = null)
    {
        InitializeComponent();

        selectedDifficulty = difficulty;
        digitCount = digits;
        timeLimitSeconds = seconds;
        playerAccountId = accountId;

        digitLabels = new[]
        {
            lblDigitOne,
            lblDigitTwo,
            lblDigitThree,
            lblDigitFour,
            lblDigitFive
        };

        brdDigitFour.IsVisible = digitCount >= 4;
        brdDigitFive.IsVisible = digitCount == 5;

        lblCodeInstruction.Text =
            $"ENTER {digitCount}-DIGIT OVERRIDE";

        // Create number buttons and store each digit in StyleId.
        for (int digit = 0; digit <= 9; digit++)
        {
            Button btnDigit = new Button
            {
                Text = digit.ToString(),
                StyleId = digit.ToString(),
                Style = (Style)Application.Current!
                    .Resources["WelcomeGuestStyle"]
            };

            btnDigit.Clicked += OnNumberClicked;

            // Place 1–9 in three rows and 0 below them.
            int row = digit == 0 ? 3 : (digit - 1) / 3;
            int column = digit == 0 ? 1 : (digit - 1) % 3;

            Grid.SetRow(btnDigit, row);
            Grid.SetColumn(btnDigit, column);

            grdKeypad.Children.Add(btnDigit);
        }

        btnDelete.Clicked += OnDeleteClicked;
        btnEnter.Clicked += OnEnterClicked;
        btnPause.Clicked += OnPauseClicked;
        btnChat.Clicked += OnChatClicked;

        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(200);
        gameTimer.Tick += OnGameTimerTick;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (gameStarted)
            return;

        StartNewGame();
    }

    private void StartNewGame()
    {
        gameTimer.Stop();
        flightClock.Reset();

        gameStarted = true;
        gameFinished = false;
        isPaused = false;

        guessCount = 0;
        currentGuess = "";

        GenerateSecretCode();
        UpdateDigitBoxes();
        UpdateFlightDisplay();

        lblHits.Text = "HITS: —";
        lblMatches.Text = "MATCHES: —";
        lblGuessCount.Text = "Guesses: 0";
        lblGuessHistory.Text = "Your guesses will appear here.";

        lblGameMessage.Text =
            $"{selectedDifficulty}: enter {digitCount} different digits.";

        btnPause.Text = "Pause";
        btnPause.IsEnabled = true;
        btnChat.IsEnabled = true;
        grdKeypad.IsEnabled = true;

        flightClock.Start();
        gameTimer.Start();
    }

    private void GenerateSecretCode()
    {
        // The first digit cannot be zero.
        secretCode = Random.Shared.Next(1, 10).ToString();

        while (secretCode.Length < digitCount)
        {
            string nextDigit =
                Random.Shared.Next(0, 10).ToString();

            if (!secretCode.Contains(nextDigit))
                secretCode += nextDigit;
        }
    }

    private bool CanPlay()
    {
        if (gameFinished || isPaused)
            return false;

        if (flightClock.Elapsed.TotalSeconds >= timeLimitSeconds)
        {
            FinishGame(false);
            return false;
        }

        return true;
    }

    private void OnNumberClicked(object? sender, EventArgs e)
    {
        if (!CanPlay() || sender is not Button btnDigit)
            return;

        string selectedDigit = btnDigit.StyleId;

        if (currentGuess.Length >= digitCount)
            return;

        if (currentGuess.Contains(selectedDigit))
        {
            lblGameMessage.Text =
                "Each digit must be different.";
            return;
        }

        if (currentGuess.Length == 0 && selectedDigit == "0")
        {
            lblGameMessage.Text =
                "The first digit cannot be zero.";
            return;
        }

        currentGuess += selectedDigit;

        lblGameMessage.Text =
            "Press Enter to check your code.";

        UpdateDigitBoxes();
    }

    private void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (!CanPlay() || currentGuess.Length == 0)
            return;

        currentGuess = currentGuess.Remove(
            currentGuess.Length - 1);

        UpdateDigitBoxes();
    }

    private void UpdateDigitBoxes()
    {
        for (int digitIndex = 0;
             digitIndex < digitCount;
             digitIndex++)
        {
            digitLabels[digitIndex].Text =
                digitIndex < currentGuess.Length
                    ? currentGuess[digitIndex].ToString()
                    : "_";
        }
    }

    private void OnEnterClicked(object? sender, EventArgs e)
    {
        if (!CanPlay())
            return;

        if (currentGuess.Length != digitCount)
        {
            lblGameMessage.Text =
                $"Please enter all {digitCount} digits.";
            return;
        }

        int hits = 0;
        int matches = 0;

        for (int digitIndex = 0;
             digitIndex < digitCount;
             digitIndex++)
        {
            if (currentGuess[digitIndex] == secretCode[digitIndex])
            {
                // Correct digit in the correct position.
                hits++;
            }
            else if (secretCode.Contains(currentGuess[digitIndex]))
            {
                // Correct digit in the wrong position.
                matches++;
            }
        }

        guessCount++;

        lblHits.Text = $"HITS: {hits}";
        lblMatches.Text = $"MATCHES: {matches}";
        lblGuessCount.Text = $"Guesses: {guessCount}";

        string guessRecord =
            $"{guessCount}. {currentGuess} — " +
            $"{hits} hits, {matches} matches";

        // Show the latest guess first.
        lblGuessHistory.Text = guessCount == 1
            ? guessRecord
            : guessRecord + Environment.NewLine
                + lblGuessHistory.Text;

        if (hits == digitCount)
        {
            FinishGame(true);
            return;
        }

        currentGuess = "";
        UpdateDigitBoxes();

        lblGameMessage.Text =
            "Code rejected. Try another code.";
    }

    private void OnGameTimerTick(object? sender, EventArgs e)
    {
        if (gameFinished || isPaused)
            return;

        UpdateFlightDisplay();

        if (flightClock.Elapsed.TotalSeconds >= timeLimitSeconds)
            FinishGame(false);
    }

    private void UpdateFlightDisplay()
    {
        double remainingSeconds = Math.Max(
            0,
            timeLimitSeconds - flightClock.Elapsed.TotalSeconds);

        int displayedSeconds =
            (int)Math.Ceiling(remainingSeconds);

        TimeSpan remainingTime =
            TimeSpan.FromSeconds(displayedSeconds);

        lblTimeLeft.Text = remainingTime.ToString(@"mm\:ss");

        // The altitude display follows the remaining time.
        int altitude = 1200 + (int)(
            8800 * remainingSeconds / timeLimitSeconds);

        lblAltitude.Text = $"{altitude:N0} FT";

        cvTimeWarning.UpdateTime(
            gameFinished ? 0 : displayedSeconds);
    }

    private void OnPauseClicked(object? sender, EventArgs e)
    {
        if (gameFinished)
            return;

        if (!isPaused &&
            flightClock.Elapsed.TotalSeconds >= timeLimitSeconds)
        {
            FinishGame(false);
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            flightClock.Stop();
            gameTimer.Stop();

            btnPause.Text = "Resume";
            lblGameMessage.Text = "Flight paused.";
        }
        else
        {
            flightClock.Start();
            gameTimer.Start();

            btnPause.Text = "Pause";
            lblGameMessage.Text = "Flight resumed.";
        }

        grdKeypad.IsEnabled = !isPaused;
    }

    private async void OnChatClicked(object? sender, EventArgs e)
    {
        await DisplayAlert(
            "Flight Chat",
            "The chat page has not been connected yet.",
            "OK");
    }

    private async void FinishGame(bool won)
    {
        // Prevent the same round from being completed twice.
        if (gameFinished)
            return;

        gameFinished = true;

        flightClock.Stop();
        gameTimer.Stop();
        UpdateFlightDisplay();

        grdKeypad.IsEnabled = false;
        btnPause.IsEnabled = false;
        btnChat.IsEnabled = false;

        string resultTitle =
            won ? "Code Accepted!" : "Game Over";

        lblGameMessage.Text = "Saving flight result...";

        FlightRecord flightRecord = new FlightRecord
        {
            PlayerAccountId = playerAccountId,
            Difficulty = selectedDifficulty,
            DigitCount = digitCount,
            TimeLimitSeconds = timeLimitSeconds,
            GuessCount = guessCount,

            TimeTakenSeconds = Math.Clamp(
                flightClock.Elapsed.TotalSeconds,
                0,
                timeLimitSeconds),

            Won = won,
            PlayedAtUtc = DateTime.UtcNow
        };

        bool resultSaved = false;

        try
        {
            await App.FlightRecordsService.AddRecordAsync(
                flightRecord);

            resultSaved = true;
        }
        catch (Exception exception)
        {
            Debug.WriteLine(
                $"Could not save flight result: {exception}");
        }

        string resultMessage = won
            ? $"Flight control restored!\nGuesses used: {guessCount}"
            : $"Time has run out.\nThe code was {secretCode}." +
              $"\nGuesses used: {guessCount}";

        if (!resultSaved)
        {
            resultMessage +=
                "\n\nThis result could not be saved to Flight Records.";
        }

        lblGameMessage.Text = resultTitle;

        bool playAgain = await DisplayAlert(
            resultTitle,
            resultMessage,
            "Play Again",
            "Stay Here");

        btnChat.IsEnabled = true;

        if (playAgain)
            StartNewGame();
    }

    protected override void OnDisappearing()
    {
        gameTimer.Stop();
        flightClock.Stop();

        if (gameStarted && !gameFinished)
        {
            isPaused = true;
            btnPause.Text = "Resume";
            grdKeypad.IsEnabled = false;
            lblGameMessage.Text = "Flight paused.";
        }

        base.OnDisappearing();
    }
}