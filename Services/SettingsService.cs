using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace SkyLock.Services;

public static class SettingsService
{
    public static string DefaultDifficulty
    {
        get => Preferences.Default.Get(
            "Settings.DefaultDifficulty", "Easy");

        set => Preferences.Default.Set(
            "Settings.DefaultDifficulty", value);
    }

    public static bool SoundEffectsEnabled
    {
        get => Preferences.Default.Get(
            "Settings.SoundEffectsEnabled", true);

        set => Preferences.Default.Set(
            "Settings.SoundEffectsEnabled", value);
    }

    public static bool MusicEnabled
    {
        get => Preferences.Default.Get(
            "Settings.MusicEnabled", true);

        set => Preferences.Default.Set(
            "Settings.MusicEnabled", value);
    }

    public static bool VoiceHintsEnabled
    {
        get => Preferences.Default.Get(
            "Settings.VoiceHintsEnabled", false);

        set => Preferences.Default.Set(
            "Settings.VoiceHintsEnabled", value);
    }

    public static bool DarkModeEnabled
    {
        get => Preferences.Default.Get(
            "Settings.DarkModeEnabled", true);

        set => Preferences.Default.Set(
            "Settings.DarkModeEnabled", value);
    }
}