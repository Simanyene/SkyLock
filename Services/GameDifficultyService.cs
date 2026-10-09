using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyLock.Models;

namespace SkyLock.Services;

public static class GameDifficultyService
{
    public static int GetDigitCount(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => 3,
            GameDifficulty.Medium => 4,
            GameDifficulty.Hard => 6,
            _ => throw new ArgumentOutOfRangeException(
                nameof(difficulty))
        };
    }

    public static int GetTimeLimitSeconds(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => 240,
            GameDifficulty.Medium => 180,
            GameDifficulty.Hard => 120,
            _ => throw new ArgumentOutOfRangeException(
                nameof(difficulty))
        };
    }

    public static bool ShouldLockHits(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => true,
            GameDifficulty.Medium => false,
            GameDifficulty.Hard => false,
            _ => throw new ArgumentOutOfRangeException(
                nameof(difficulty))
        };
    }

    public static string ToKey(GameDifficulty difficulty)
    {
        return difficulty switch
        {
            GameDifficulty.Easy => "easy",
            GameDifficulty.Medium => "medium",
            GameDifficulty.Hard => "hard",
            _ => throw new ArgumentOutOfRangeException(
                nameof(difficulty))
        };
    }

    public static GameDifficulty FromKey(string? key)
    {
        return key?.Trim().ToLowerInvariant() switch
        {
            "medium" => GameDifficulty.Medium,
            "hard" => GameDifficulty.Hard,
            _ => GameDifficulty.Easy
        };
    }
}