using System;
using Microsoft.Maui.Controls;
using SkyLock.Models;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class SettingsPage : ContentPage
{
    private bool isLoadingSettings;

    public SettingsPage() : this(null)
    {
    }

    public SettingsPage(int? accountId)
    {
        InitializeComponent();

        // Settings apply to the whole app.
        // accountId preserves compatibility with HomePage navigation.
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadSettings();
        ApplyTheme();
    }

    private void LoadSettings()
    {
        isLoadingSettings = true;

        try
        {
            pkrDefaultDifficulty.SelectedIndex =
                SettingsService.DefaultDifficulty switch
                {
                    GameDifficulty.Medium => 1,
                    GameDifficulty.Hard => 2,
                    _ => 0
                };

            swtSoundEffects.IsToggled =
                SettingsService.SoundEffectsEnabled;

            swtMusic.IsToggled =
                SettingsService.MusicEnabled;

            swtVoiceHints.IsToggled =
                SettingsService.VoiceHintsEnabled;

            swtDarkMode.IsToggled =
                SettingsService.DarkModeEnabled;
        }
        finally
        {
            isLoadingSettings = false;
        }
    }

    private void OnDifficultyChanged(object? sender, EventArgs e)
    {
        if (isLoadingSettings ||
            pkrDefaultDifficulty.SelectedIndex < 0)
        {
            return;
        }

        SettingsService.DefaultDifficulty =
            pkrDefaultDifficulty.SelectedIndex switch
            {
                1 => GameDifficulty.Medium,
                2 => GameDifficulty.Hard,
                _ => GameDifficulty.Easy
            };

        ShowSavedMessage();
    }

    private void OnSoundEffectsToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.SoundEffectsEnabled = e.Value;
        ShowSavedMessage();
    }

    private void OnMusicToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.MusicEnabled = e.Value;
        ShowSavedMessage();
    }

    private void OnVoiceHintsToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.VoiceHintsEnabled = e.Value;
        ShowSavedMessage();
    }

    private void OnDarkModeToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.DarkModeEnabled = e.Value;

        ApplyTheme();
        ShowSavedMessage();
    }

    private static void ApplyTheme()
    {
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme =
                SettingsService.DarkModeEnabled
                    ? AppTheme.Dark
                    : AppTheme.Light;
        }
    }

    private void ShowSavedMessage()
    {
        lblSettingsMessage.Text = "Settings saved.";
    }

    private async void OnResetSettingsClicked(
        object? sender,
        EventArgs e)
    {
        bool resetSettings = await DisplayAlert(
            "Restore settings",
            "Restore the default settings?",
            "Restore",
            "Cancel");

        if (!resetSettings)
            return;

        SettingsService.DefaultDifficulty = GameDifficulty.Easy;
        SettingsService.SoundEffectsEnabled = true;
        SettingsService.MusicEnabled = true;
        SettingsService.VoiceHintsEnabled = false;
        SettingsService.DarkModeEnabled = true;

        LoadSettings();
        ApplyTheme();

        lblSettingsMessage.Text = "Default settings restored.";
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}