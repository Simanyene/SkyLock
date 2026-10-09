
using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Media;
using SkyLock.Models;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class SettingsPage : ContentPage
{
    private readonly int? playerAccountId;
    private bool isLoadingSettings;

    public SettingsPage() : this(
        PlayerSessionService.CurrentAccountId)
    {
    }

    public SettingsPage(int? accountId)
    {
        InitializeComponent();
        playerAccountId = accountId;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadSettings();
        UpdateLanguage();
    }

    // Load the saved settings.
    private void LoadSettings()
    {
        isLoadingSettings = true;

        try
        {
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

        ApplyTheme();
        HighlightSelections();
    }

    // Highlight selected difficulty and language.
    private void HighlightSelections()
    {
        HighlightButton(
            btnEasy,
            SettingsService.DefaultDifficulty ==
            GameDifficulty.Easy);

        HighlightButton(
            btnMedium,
            SettingsService.DefaultDifficulty ==
            GameDifficulty.Medium);

        HighlightButton(
            btnHard,
            SettingsService.DefaultDifficulty ==
            GameDifficulty.Hard);

        HighlightButton(
            btnEnglish,
            SettingsService.Language == "en");

        HighlightButton(
            btnZulu,
            SettingsService.Language == "zu");
    }

    // Use SkyLock's own colours.
    private static void HighlightButton(
        Button button,
        bool selected)
    {
        if (selected)
        {
            button.BackgroundColor =
                Color.FromArgb("#0067DB");

            button.BorderColor =
                Color.FromArgb("#39DEFF");

            button.BorderWidth = 2;

            button.TextColor = Colors.White;
            button.FontAttributes = FontAttributes.Bold;
        }
        else
        {
            button.BackgroundColor =
                Color.FromArgb("#073B5B");

            button.BorderColor =
                Color.FromArgb("#14B8EF");

            button.BorderWidth = 1;

            button.TextColor = Colors.White;
            button.FontAttributes = FontAttributes.None;
        }
    }

    // Difficulty buttons.
    private async void OnDifficultyClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        SettingsService.DefaultDifficulty =
            button.StyleId switch
            {
                "Medium" => GameDifficulty.Medium,
                "Hard" => GameDifficulty.Hard,
                _ => GameDifficulty.Easy
            };

        HighlightSelections();
        ShowSavedMessage();

        await SpeakSettingAsync(button.Text);
    }

    // Language buttons.
    private async void OnLanguageClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is not Button button)
            return;

        string language =
            button.StyleId == "zu" ? "zu" : "en";

        LocalizationService.SetLanguage(language);

        UpdateLanguage();
        HighlightSelections();
        ShowSavedMessage();

        await SpeakSettingAsync(button.Text);
    }

    // Translate the Settings Page immediately.
    private void UpdateLanguage()
    {
        bool isZulu = SettingsService.Language == "zu";

        lblSettingsTitle.Text =
            LocalizationService.T("settings").ToUpperInvariant();

        lblSettingsDescription.Text = isZulu
            ? "Lungisa izilungiselelo zendiza yakho."
            : "Personalise your flight.";

        btnBack.Text = LocalizationService.T("back");

        lblDifficultyHeading.Text =
            LocalizationService.T("difficulty").ToUpperInvariant();

        btnEasy.Text = LocalizationService.T("easy");
        btnMedium.Text = LocalizationService.T("medium");
        btnHard.Text = LocalizationService.T("hard");

        lblDifficultyHint.Text = isZulu
            ? "Khetha izinga lobunzima olithandayo."
            : "Choose your preferred difficulty.";

        lblLanguageHeading.Text =
            LocalizationService.T("language").ToUpperInvariant();

        lblAudioHeading.Text = isZulu
            ? "IZILUNGISELELO ZOMSINDO"
            : "AUDIO SETTINGS";

        lblSoundEffects.Text =
            LocalizationService.T("sound");

        lblMusic.Text =
            LocalizationService.T("music");

        lblVoiceHints.Text =
            LocalizationService.T("voice_hints");

        lblDarkMode.Text =
            LocalizationService.T("dark_theme");

        btnResetSettings.Text = isZulu
            ? "Buyisela Izilungiselelo Zokuqala"
            : "Restore Default Settings";

        lblSettingsMessage.Text = isZulu
            ? "Izinguquko zigcinwa ngokuzenzakalelayo."
            : "Changes are saved automatically.";
    }

    // Sound effects switch.
    private async void OnSoundEffectsToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.SoundEffectsEnabled = e.Value;
        ShowSavedMessage();

        await SpeakSettingAsync(
            lblSoundEffects.Text + " " +
            OnOff(e.Value));
    }

    // Background music switch.
    private async void OnMusicToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.MusicEnabled = e.Value;
        ShowSavedMessage();

        await SpeakSettingAsync(
            lblMusic.Text + " " +
            OnOff(e.Value));
    }

    // Voice hints switch.
    private async void OnVoiceHintsToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.VoiceHintsEnabled = e.Value;
        ShowSavedMessage();

        // Speak once when enabling voice hints.
        if (e.Value)
        {
            await SpeakSettingAsync(
                lblVoiceHints.Text + " " +
                OnOff(true));
        }
    }

    // Dark mode switch.
    private async void OnDarkModeToggled(
        object? sender,
        ToggledEventArgs e)
    {
        if (isLoadingSettings)
            return;

        SettingsService.DarkModeEnabled = e.Value;

        ApplyTheme();
        ShowSavedMessage();

        await SpeakSettingAsync(
            lblDarkMode.Text + " " +
            OnOff(e.Value));
    }

    private static void ApplyTheme()
    {
        if (Application.Current is null)
            return;

        Application.Current.UserAppTheme =
            SettingsService.DarkModeEnabled
                ? AppTheme.Dark
                : AppTheme.Light;
    }

    private static string OnOff(bool enabled)
    {
        return LocalizationService.T(
            enabled ? "on" : "off");
    }

    // Speak feedback only when voice hints are enabled.
    private static async Task SpeakSettingAsync(string message)
    {
        if (!SettingsService.VoiceHintsEnabled)
            return;

        try
        {
            await TextToSpeech.Default.SpeakAsync(message);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"Voice feedback unavailable: {ex.Message}");
        }
    }

    private void ShowSavedMessage()
    {
        lblSettingsMessage.Text =
            SettingsService.Language == "zu"
                ? "Izilungiselelo zigciniwe."
                : "Settings saved.";
    }

    // Restore all settings.
    private async void OnResetSettingsClicked(
        object? sender,
        EventArgs e)
    {
        bool isZulu = SettingsService.Language == "zu";

        bool confirmed = await DisplayAlert(
            isZulu
                ? "Buyisela Izilungiselelo"
                : "Restore Settings",
            isZulu
                ? "Uyafuna ukubuyisela izilungiselelo zokuqala?"
                : "Restore all default settings?",
            isZulu ? "Buyisela" : "Restore",
            isZulu ? "Khansela" : "Cancel");

        if (!confirmed)
            return;

        SettingsService.DefaultDifficulty =
            GameDifficulty.Easy;

        SettingsService.SoundEffectsEnabled = true;
        SettingsService.MusicEnabled = true;
        SettingsService.VoiceHintsEnabled = false;
        SettingsService.DarkModeEnabled = true;

        // Reset language to English.
        LocalizationService.SetLanguage("en");

        LoadSettings();
        UpdateLanguage();

        lblSettingsMessage.Text =
            "Default settings restored.";
    }

    // Return to Home.
    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
        else if (Window is not null)
        {
            Window.Page = new NavigationPage(
                new HomePage(playerAccountId));
        }
    }
}
