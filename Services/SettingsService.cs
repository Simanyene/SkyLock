using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SkyLock.Models;

namespace SkyLock.Services;

public static class SettingsService
{
    private const string DifficultyKey = "Settings.DefaultDifficulty";
    private const string SoundKey = "Settings.SoundEffectsEnabled";
    private const string MusicKey = "Settings.MusicEnabled";
    private const string VoiceKey = "Settings.VoiceHintsEnabled";
    private const string DarkKey = "Settings.DarkModeEnabled";
    private const string LanguageKey = "Settings.Language";

    public static GameDifficulty DefaultDifficulty
    {
        get
        {
            if (Preferences.Default.ContainsKey(DifficultyKey))
            {
                string saved = Preferences.Default.Get(
                    DifficultyKey, "Easy");

                return GameDifficultyService.FromKey(saved);
            }

            // The old AppSettings class stored difficulty as a number.
            int oldValue = Preferences.Default.Get("difficulty", 0);

            return oldValue switch
            {
                1 => GameDifficulty.Medium,
                2 => GameDifficulty.Hard,
                _ => GameDifficulty.Easy
            };
        }
        set
        {
            // Also validates the supplied enum value.
            string key = GameDifficultyService.ToKey(value);
            Preferences.Default.Set(DifficultyKey, key);
        }
    }

    public static bool SoundEffectsEnabled
    {
        get => Preferences.Default.Get(
            SoundKey,
            Preferences.Default.Get("sound_on", true));

        set => Preferences.Default.Set(SoundKey, value);
    }

    public static bool MusicEnabled
    {
        get => Preferences.Default.Get(MusicKey, true);
        set => Preferences.Default.Set(MusicKey, value);
    }

    public static bool VoiceHintsEnabled
    {
        get => Preferences.Default.Get(
            VoiceKey,
            Preferences.Default.Get("voice_hints", false));

        set => Preferences.Default.Set(VoiceKey, value);
    }

    public static bool DarkModeEnabled
    {
        get => Preferences.Default.Get(
            DarkKey,
            Preferences.Default.Get("dark_theme", true));

        set => Preferences.Default.Set(DarkKey, value);
    }

    public static string Language
    {
        get
        {
            string saved = Preferences.Default.Get(
                LanguageKey,
                Preferences.Default.Get("language", "en"));

            return saved == "zu" ? "zu" : "en";
        }
        set
        {
            Preferences.Default.Set(
                LanguageKey,
                value == "zu" ? "zu" : "en");
        }
    }

    public static void ApplyTheme()
    {
        if (Application.Current is not null)
        {
            Application.Current.UserAppTheme =
                DarkModeEnabled ? AppTheme.Dark : AppTheme.Light;
        }
    }

    public static void ResetToDefaults()
    {
        DefaultDifficulty = GameDifficulty.Easy;
        SoundEffectsEnabled = true;
        MusicEnabled = true;
        VoiceHintsEnabled = false;
        DarkModeEnabled = true;

        LocalizationService.SetLanguage("en");
        SpeechService.Stop();
        ApplyTheme();
    }
}