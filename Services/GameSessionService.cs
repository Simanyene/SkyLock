using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyLock.Models;

namespace SkyLock.Services;

public static class GameSessionService
{
    public static GameDifficulty Difficulty { get; private set; } =
        GameDifficulty.Easy;

    // Captured when the game starts.
    public static int? PlayerAccountId { get; private set; }

    public static bool HasStarted { get; private set; }

    public static bool HasFinished { get; private set; }

    public static bool Won { get; private set; }

    public static double SecondsTaken { get; private set; }

    public static int GuessCount { get; private set; }

    public static string? Message { get; private set; }

    public static string? SecretCode { get; private set; }

    public static DateTime? FinishedAtUtc { get; private set; }

    public static event Action? AppPaused;
    public static event Action? AppResumed;

    public static void StartSolo()
    {
        Start(SettingsService.DefaultDifficulty);
    }

    public static void Start(GameDifficulty difficulty)
    {
        // Validate the difficulty before starting.
        _ = GameDifficultyService.GetDigitCount(difficulty);

        Reset();

        Difficulty = difficulty;
        PlayerAccountId = PlayerSessionService.CurrentAccountId;
        HasStarted = true;
    }

    public static void Finish(
        bool won,
        double secondsTaken,
        int guessCount,
        string secretCode,
        string? message = null)
    {
        if (!HasStarted || HasFinished)
        {
            throw new InvalidOperationException(
                "Start an unfinished game before recording its result.");
        }

        if (!double.IsFinite(secondsTaken) || secondsTaken < 0)
            throw new ArgumentOutOfRangeException(nameof(secondsTaken));

        if (guessCount < 0)
            throw new ArgumentOutOfRangeException(nameof(guessCount));

        Won = won;
        SecondsTaken = secondsTaken;
        GuessCount = guessCount;
        SecretCode = secretCode;
        Message = message;
        FinishedAtUtc = DateTime.UtcNow;
        HasFinished = true;
    }

    public static FlightRecord CreateFlightRecord()
    {
        if (!HasFinished)
        {
            throw new InvalidOperationException(
                "Finish the game before creating its record.");
        }

        return new FlightRecord
        {
            PlayerAccountId = GameSessionService.PlayerAccountId,
            Difficulty = Difficulty.ToString(),
            DigitCount = GameDifficultyService.GetDigitCount(Difficulty),
            TimeLimitSeconds =
                GameDifficultyService.GetTimeLimitSeconds(Difficulty),
            GuessCount = GameSessionService.GuessCount,
            TimeTakenSeconds = SecondsTaken,
            Won = GameSessionService.Won,
            PlayedAtUtc = FinishedAtUtc!.Value
        };
    }

    public static void RaiseAppPaused()
    {
        AppPaused?.Invoke();
    }

    public static void RaiseAppResumed()
    {
        AppResumed?.Invoke();
    }

    public static void Reset()
    {
        Difficulty = GameDifficulty.Easy;
        PlayerAccountId = null;
        HasStarted = false;
        HasFinished = false;
        Won = false;
        SecondsTaken = 0;
        GuessCount = 0;
        Message = null;
        SecretCode = null;
        FinishedAtUtc = null;
    }
}
