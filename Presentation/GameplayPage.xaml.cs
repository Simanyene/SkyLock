using Microsoft.Maui.Dispatching;
using SkyLock.Models;
using System.Diagnostics;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class GameplayPage : ContentPage
{
    private readonly string selectedDifficulty;
    private readonly int digitCount;
    private readonly int timeLimitSeconds;
    private readonly int? playerAccountId;

    // Correct digits are locked in place after a guess. Easy only, same rule as DifficultyInfo.LocksHits.
    private readonly bool locksHits;

    private readonly IDispatcherTimer gameTimer;
    private readonly Stopwatch flightClock = new();
    private readonly Label[] digitLabels;

    // Colours for locked (correct) digits
    private static readonly Color LockedGreen = Color.FromArgb("#2E7D32");
    private readonly Border[] digitBorders;
    private readonly Brush[] defaultBorderBrushes;
    private readonly Color? defaultTextColor;

    private string secretCode = "";

    // One slot per digit position. '\0' = empty, otherwise the digit typed or locked there.
    private char[] guessSlots = Array.Empty<char>();
    private bool[] lockedSlots = Array.Empty<bool>();

    private int guessCount;
    private bool gameStarted;
    private bool gameFinished;
    private bool isPaused;

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

        locksHits = difficulty.Equals("Easy", StringComparison.OrdinalIgnoreCase);

        guessSlots = new char[digitCount];
        lockedSlots = new bool[digitCount];

        digitLabels = new[]
        {
            lblDigitOne,
            lblDigitTwo,
            lblDigitThree,
            lblDigitFour,
            lblDigitFive
        };

        digitBorders = Array.ConvertAll(digitLabels, label => (Border)label.Parent!);
        defaultBorderBrushes = Array.ConvertAll(digitBorders, border => border.Background);
        defaultTextColor = lblDigitOne.TextColor;

        brdDigitFour.IsVisible = digitCount >= 4;
        brdDigitFive.IsVisible = digitCount == 5;

        lblCodeInstruction.Text =
            $"ENTER {digitCount}-DIGIT OVERRIDE";

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

            int row = digit == 0 ? 3 : (digit - 1) / 3;
            int column = digit == 0 ? 1 : (digit - 1) % 3;

            Grid.SetRow(btnDigit, row);
            Grid.SetColumn(btnDigit, column);

            grdKeypad.Children.Add(btnDigit);
        }

        btnDelete.Clicked += OnDeleteClicked;
        btnEnter.Clicked += OnEnterClicked;
        btnPause.Clicked += OnPauseClicked;
        btnBack.Clicked += OnBackClicked;

        gameTimer = Dispatcher.CreateTimer();
        gameTimer.Interval = TimeSpan.FromMilliseconds(200);
        gameTimer.Tick += OnGameTimerTick;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (!gameStarted)
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
        ClearGuessSlots();

        GenerateSecretCode();
        UpdateDigitBoxes();

        lblHits.Text = "HITS: —";
        lblMatches.Text = "MATCHES: —";
        lblGuessCount.Text = "Guesses: 0";
        lblGuessHistory.Text =
            "Your guesses will appear here.";

        lblGameMessage.Text =
            $"{selectedDifficulty}: enter {digitCount} different digits.";

        btnPause.Text = "Pause";
        btnPause.IsEnabled = true;
        btnBack.IsEnabled = true;
        grdKeypad.IsEnabled = true;

        flightClock.Start();
        gameTimer.Start();

        UpdateFlightDisplay();
    }

    private void ClearGuessSlots()
    {
        Array.Clear(guessSlots);
        Array.Clear(lockedSlots);
    }

    private void GenerateSecretCode()
    {
        secretCode =
            Random.Shared.Next(1, 10).ToString();

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

    /// <summary>First position that is not locked and still empty, or -1 if none.</summary>
    private int NextOpenSlot()
    {
        for (int i = 0; i < digitCount; i++)
        {
            if (!lockedSlots[i] && guessSlots[i] == '\0')
                return i;
        }

        return -1;
    }

    private void OnNumberClicked(
        object? sender,
        EventArgs e)
    {
        if (!CanPlay() ||
            sender is not Button btnDigit)
        {
            return;
        }

        char selectedDigit = btnDigit.StyleId[0];

        int slot = NextOpenSlot();
        if (slot < 0)
            return;

        if (Array.IndexOf(guessSlots, selectedDigit) >= 0)
        {
            lblGameMessage.Text =
                "Each digit must be different.";
            return;
        }

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

    private void OnDeleteClicked(
        object? sender,
        EventArgs e)
    {
        if (!CanPlay())
            return;

        // Remove the last digit that the player typed (locked digits stay).
        for (int i = digitCount - 1; i >= 0; i--)
        {
            if (!lockedSlots[i] && guessSlots[i] != '\0')
            {
                guessSlots[i] = '\0';
                UpdateDigitBoxes();
                return;
            }
        }
    }

    private void UpdateDigitBoxes()
    {
        for (int digitIndex = 0;
             digitIndex < digitCount;
             digitIndex++)
        {
            bool locked = lockedSlots[digitIndex];
            Label label = digitLabels[digitIndex];

            label.Text =
                guessSlots[digitIndex] == '\0'
                    ? "_"
                    : guessSlots[digitIndex].ToString();

            label.TextColor = locked ? Colors.White : defaultTextColor;
            digitBorders[digitIndex].Background = locked
                ? new SolidColorBrush(LockedGreen)
                : defaultBorderBrushes[digitIndex];
        }
    }

    private void OnEnterClicked(
        object? sender,
        EventArgs e)
    {
        if (!CanPlay())
            return;

        if (NextOpenSlot() != -1)
        {
            lblGameMessage.Text =
                $"Please enter all {digitCount} digits.";
            return;
        }

        int hits = 0;
        int matches = 0;
        bool[] hitPositions = new bool[digitCount];

        for (int digitIndex = 0;
             digitIndex < digitCount;
             digitIndex++)
        {
            if (guessSlots[digitIndex] ==
                secretCode[digitIndex])
            {
                hits++;
                hitPositions[digitIndex] = true;
            }
            else if (secretCode.Contains(
                guessSlots[digitIndex]))
            {
                matches++;
            }
        }

        guessCount++;

        lblHits.Text = $"HITS: {hits}";
        lblMatches.Text = $"MATCHES: {matches}";
        lblGuessCount.Text = $"Guesses: {guessCount}";

        string guessText = string.Concat(guessSlots);
        string guessRecord =
            $"{guessCount}. {guessText} — " +
            $"{hits} hits, {matches} matches";

        lblGuessHistory.Text =
            guessCount == 1
                ? guessRecord
                : guessRecord +
                  Environment.NewLine +
                  lblGuessHistory.Text;

        if (hits == digitCount)
        {
            FinishGame(true);
            return;
        }

        // Lock correct digits in place (Easy), then clear everything else.
        for (int digitIndex = 0;
             digitIndex < digitCount;
             digitIndex++)
        {
            if (locksHits && hitPositions[digitIndex])
                lockedSlots[digitIndex] = true;

            if (!lockedSlots[digitIndex])
                guessSlots[digitIndex] = '\0';
        }

        UpdateDigitBoxes();

        lblGameMessage.Text =
            locksHits && hits > 0
                ? $"{hits} digit(s) locked in place. Try another code."
                : "Code rejected. Try another code.";
    }

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

        cvTimeWarning.UpdateTime(
            gameFinished
                ? 0
                : displayedSeconds);
    }

    private void PauseFlight()
    {
        isPaused = true;
        flightClock.Stop();
        gameTimer.Stop();

        btnPause.Text = LocalizationService.T("resume");
        grdKeypad.IsEnabled = false;
        lblGameMessage.Text = "Flight paused.";
    }
    private async void OnBackClicked(object? sender, EventArgs e)
    {
        if (gameFinished)
            return;

        bool wasPaused = isPaused;

        if (!wasPaused)
            PauseFlight();

        string choice = await DisplayActionSheet(
            LocalizationService.T("back_title"),
            LocalizationService.T("resume"),          // cancel: keep flying
            null,
            LocalizationService.T("back_quit"),
            LocalizationService.T("back_side_menu"));

        if (choice == LocalizationService.T("back_quit"))
        {
            // Leave without saving a result
            gameFinished = true;
            StopFlightTimer();
            await Navigation.PopAsync();
        }
        else if (choice == LocalizationService.T("back_side_menu"))
        {
            gameFinished = true;
            StopFlightTimer();
            await Navigation.PopAsync();

            if (Shell.Current is not null)
                Shell.Current.FlyoutIsPresented = true;
        }
        else if (!wasPaused)
        {
            // Resume, or the popup was dismissed
            ResumeFlight();
        }
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

        if (isPaused)
            ResumeFlight();
        else
            PauseFlight();
    }

    private void ResumeFlight()
    {
        isPaused = false;
        flightClock.Start();
        gameTimer.Start();

        btnPause.Text = LocalizationService.T("pause");
        grdKeypad.IsEnabled = true;
        lblGameMessage.Text = "Flight resumed.";
    }

    private void StopFlightTimer()
    {
        flightClock.Stop();
        gameTimer.Stop();
    }

    private async void FinishGame(bool won)
    {
        if (gameFinished)
            return;

        gameFinished = true;

        flightClock.Stop();
        gameTimer.Stop();

        UpdateFlightDisplay();

        grdKeypad.IsEnabled = false;
        btnPause.IsEnabled = false;
        btnBack.IsEnabled = false;

        lblGameMessage.Text =
            "Saving flight result...";

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
            await App.FlightRecordsService
                .AddRecordAsync(flightRecord);

            resultSaved = true;
        }
        catch (Exception exception)
        {
            Debug.WriteLine(
                $"Could not save flight result: {exception}");
        }

        ResultsPage resultsPage =
            new ResultsPage(
                won,
                selectedDifficulty,
                digitCount,
                timeLimitSeconds,
                guessCount,
                elapsedSeconds,
                secretCode,
                playerAccountId,
                resultSaved);

        await Navigation.PushAsync(resultsPage);

        Navigation.RemovePage(this);
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
            lblGameMessage.Text =
                "Flight paused.";
        }

        base.OnDisappearing();
    }
}