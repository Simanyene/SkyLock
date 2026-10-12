using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;

namespace SkyLock.Services;

public static class SpeechService
{
    private static CancellationTokenSource? speechCancellation;
    private static IEnumerable<Locale>? availableLocales;

    private static readonly string[] EnglishDigits =
    {
        "zero", "one", "two", "three", "four",
        "five", "six", "seven", "eight", "nine"
    };

    private static readonly string[] ZuluDigits =
    {
        "iqanda", "kunye", "kubili", "kuthathu", "kune",
        "isihlanu", "isithupha", "isikhombisa",
        "isishiyagalombili", "isishiyagalolunye"
    };

    public static void Announce(object? sender)
    {
        if (!SettingsService.VoiceHintsEnabled ||
            sender is not Button button)
        {
            return;
        }

        if (button.StyleId is string styleId &&
            styleId.StartsWith("digit", StringComparison.Ordinal) &&
            int.TryParse(styleId.AsSpan(5), out int digit) &&
            digit >= 0 &&
            digit <= 9)
        {
            string[] words = SettingsService.Language == "zu"
                ? ZuluDigits
                : EnglishDigits;

            Say(words[digit]);
            return;
        }

        Say(button.Text);
    }

    public static void Say(string? text, bool force = false)
    {
        _ = SayAsync(text, force);
    }

    public static async Task SayAsync(
        string? text,
        bool force = false)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        if (!force && !SettingsService.VoiceHintsEnabled)
            return;

        var cancellation = new CancellationTokenSource();

        CancellationTokenSource? previous =
            Interlocked.Exchange(
                ref speechCancellation, cancellation);

        CancelSafely(previous);

        try
        {
            availableLocales ??=
                await TextToSpeech.Default.GetLocalesAsync();

            cancellation.Token.ThrowIfCancellationRequested();

            string language = SettingsService.Language;

            Locale? locale = availableLocales.FirstOrDefault(item =>
                item.Language.StartsWith(
                    language,
                    StringComparison.OrdinalIgnoreCase));

            locale ??= availableLocales.FirstOrDefault(item =>
                item.Language.StartsWith(
                    "en",
                    StringComparison.OrdinalIgnoreCase));

            var options = new SpeechOptions();

            if (locale is not null)
                options.Locale = locale;

            await TextToSpeech.Default.SpeakAsync(
                text,
                options,
                cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // A newer message or Stop() interrupted this message.
        }
        catch (Exception ex)
        {
            // Speech availability must not stop gameplay.
            Debug.WriteLine($"Speech unavailable: {ex.Message}");
        }
        finally
        {
            Interlocked.CompareExchange(
                ref speechCancellation,
                null,
                cancellation);

            cancellation.Dispose();
        }
    }

    public static void Stop()
    {
        CancellationTokenSource? previous =
            Interlocked.Exchange(ref speechCancellation, null);

        CancelSafely(previous);
    }

    private static void CancelSafely(
        CancellationTokenSource? cancellation)
    {
        try
        {
            cancellation?.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The previous speech operation has already finished.
        }
    }
}