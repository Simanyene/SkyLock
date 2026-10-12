using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SkyLock.Models;

namespace SkyLock.Services;

public static class PlayerSessionService
{
    private const string LastEmailKey = "Player.LastEmail";

    public static int? CurrentAccountId { get; private set; }

    public static string CurrentName { get; private set; } = "Guest";

    public static bool IsGuest => CurrentAccountId is null;

    public static string DisplayName =>
        IsGuest
            ? LocalizationService.T("guest_name")
            : CurrentName;

    public static string? LastPlayerEmail
    {
        get
        {
            string email = Preferences.Default.Get(LastEmailKey, "");

            return string.IsNullOrWhiteSpace(email) ? null : email;
        }
    }

    public static void SetCurrentPlayer(PlayerAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        if (account.Id <= 0)
        {
            throw new ArgumentException(
                "Save the account before starting its session.");
        }

        CurrentAccountId = account.Id;
        CurrentName = account.PilotName;

        Preferences.Default.Set(LastEmailKey, account.Email);
    }

    public static void SignInGuest()
    {
        CurrentAccountId = null;
        CurrentName = "Guest";
    }

    public static void SignOut()
    {
        Preferences.Default.Remove(LastEmailKey);
        SignInGuest();

        GameSessionService.Reset();
        SpeechService.Stop();
    }


    public static void UpdateCurrentName(
        int? accountId,
        string newName)
    {
        // Only update the active player's session.
        if (CurrentAccountId != accountId)
            return;

        // Do not allow an empty username.
        if (string.IsNullOrWhiteSpace(newName))
            return;

        // Update the username in memory.
        CurrentName = newName.Trim();
    }

}