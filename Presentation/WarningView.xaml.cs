using System;
using System.IO;
using Plugin.Maui.Audio;

namespace SkyLock.Presentation;

public partial class WarningView : ContentView
{
    private Stream? warningSoundStream;
    private IAudioPlayer? warningPlayer;

    public WarningView()
    {
        InitializeComponent();

        // Stop the sound when the view is removed.
        Unloaded += OnWarningUnloaded;
    }

    public void UpdateTime(int secondsLeft)
    {
        // Hide the warning and stop the sound outside the warning period.
        if (secondsLeft <= 0 || secondsLeft > 30)
        {
            brdWarning.IsVisible = false;
            StopWarningSound();
            return;
        }

        brdWarning.IsVisible = true;
        StartWarningSound();

        if (secondsLeft <= 10)
        {
            brdWarning.Style = (Style)Application.Current!
                .Resources["WarningCriticalBannerStyle"];

            lblWarningMessage.Text =
                $"Brace for impact!\n{secondsLeft}s until time runs out!";
        }
        else
        {
            brdWarning.Style = (Style)Application.Current!
                .Resources["WarningBannerStyle"];

            lblWarningMessage.Text =
                $"Warning: Altitude dropping!\n{secondsLeft}s remaining";
        }
    }

    private void StartWarningSound()
    {
        // Let the existing alarm continue without restarting each second.
        if (warningPlayer is not null)
            return;

        try
        {
            warningSoundStream = typeof(WarningView).Assembly
                .GetManifestResourceStream(
                    "SkyLock.Resources.Raw.warning_sound.wav");

            if (warningSoundStream is null)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Warning sound not found. Check its filename and Build Action.");

                return;
            }

            warningPlayer = AudioManager.Current
                .CreatePlayer(warningSoundStream);

            warningPlayer.Loop = true;
            warningPlayer.Volume = 1;
            warningPlayer.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Could not play warning sound: {ex.Message}");

            StopWarningSound();
        }
    }

    public void StopWarningSound()
    {
        warningPlayer?.Stop();
        warningPlayer?.Dispose();
        warningPlayer = null;

        warningSoundStream?.Dispose();
        warningSoundStream = null;
    }

    private void OnWarningUnloaded(object? sender, EventArgs e)
    {
        StopWarningSound();
    }
}