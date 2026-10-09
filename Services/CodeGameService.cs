using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkyLock.Models;

namespace SkyLock.Services;

public static class CodeGameService
{
    public static int[] GenerateCode(int digitCount)
    {
        if (digitCount < 1 || digitCount > 10)
        {
            throw new ArgumentOutOfRangeException(
                nameof(digitCount),
                "The code must contain between 1 and 10 digits.");
        }

        int[] pool = Enumerable.Range(0, 10).ToArray();

        // Shuffle the digits so the secret has no repeated digits.
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Shared.Next(i + 1);

            (pool[i], pool[randomIndex]) =
                (pool[randomIndex], pool[i]);
        }

        return pool.Take(digitCount).ToArray();
    }

    public static GuessResult Evaluate(int[] code, int[] guess)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(guess);

        if (code.Length == 0 || code.Length != guess.Length)
        {
            throw new ArgumentException(
                "The code and guess must have the same non-zero length.");
        }

        if (code.Any(digit => digit < 0 || digit > 9) ||
            guess.Any(digit => digit < 0 || digit > 9))
        {
            throw new ArgumentException(
                "All digits must be between 0 and 9.");
        }

        bool[] hitPositions = new bool[code.Length];
        int[] remainingDigits = new int[10];

        int hits = 0;
        int matches = 0;

        // First find digits in the correct positions.
        for (int i = 0; i < code.Length; i++)
        {
            if (guess[i] == code[i])
            {
                hits++;
                hitPositions[i] = true;
            }
            else
            {
                remainingDigits[code[i]]++;
            }
        }

        // Then find correct digits in the wrong positions.
        for (int i = 0; i < guess.Length; i++)
        {
            if (!hitPositions[i] &&
                remainingDigits[guess[i]] > 0)
            {
                matches++;
                remainingDigits[guess[i]]--;
            }
        }

        return new GuessResult(hits, matches, hitPositions);
    }
}
