using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;


namespace SkyLock.Services;

public static class PlayerProfileService
{
    public static IReadOnlyList<string> AvatarImages { get; } =
        Array.AsReadOnly(new[]
        {
            "avatar_male1.svg",
            "avatar_male2.svg",
            "avatar_female1.svg",
            "avatar_female2.svg",
            "avatar_neutral.svg"
        });

    private static string GetAvatarKey(int? accountId)
    {
        return accountId.HasValue
            ? $"Player.Avatar.{accountId.Value}"
            : "Player.Avatar.Guest";
    }

    public static int GetAvatarIndex(int? accountId)
    {
        int index = Preferences.Default.Get(
            GetAvatarKey(accountId),
            4);

        return Math.Clamp(index, 0, AvatarImages.Count - 1);
    }

    public static void SetAvatarIndex(
        int? accountId,
        int avatarIndex)
    {
        int index = Math.Clamp(
            avatarIndex,
            0,
            AvatarImages.Count - 1);

        Preferences.Default.Set(
            GetAvatarKey(accountId),
            index);
    }

    public static string GetAvatarImage(int? accountId)
    {
        return AvatarImages[GetAvatarIndex(accountId)];
    }

    public static int AvatarIndex
    {
        get => GetAvatarIndex(
            PlayerSessionService.CurrentAccountId);

        set => SetAvatarIndex(
            PlayerSessionService.CurrentAccountId,
            value);
    }

    public static string AvatarImage =>
        GetAvatarImage(PlayerSessionService.CurrentAccountId);
}