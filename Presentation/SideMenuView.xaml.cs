using Microsoft.Maui.Storage;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class SideMenuView : ContentView
{
    public event EventHandler<string>? MenuItemSelected;

    public event EventHandler? CloseRequested;

    public SideMenuView()
    {
        InitializeComponent();
    }

    public async Task LoadProfileAsync(int? playerAccountId)
    {
        string displayName = "Guest";

        int avatarIndex =
        PlayerProfileService.GetAvatarIndex(playerAccountId);

        if (playerAccountId.HasValue)
        {
            try
            {
                var account =
                    await App.AccountService.GetAccountByIdAsync(
                        playerAccountId.Value);

                if (account is not null)
                {
                    displayName = account.PilotName;
                }
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
            }
        }
        else
        {
            displayName =
                Preferences.Default.Get(
                    "Profile.GuestName",
                    "Guest");
        }

        imgAvatar.Source =
           PlayerProfileService.AvatarImages[avatarIndex];

        lblHello.Text =
            $"Welcome, {displayName}";
    }

    private void OnMenuItemClicked(
        object? sender,
        EventArgs e)
    {
        if (sender is Button menuButton)
        {
            MenuItemSelected?.Invoke(
                this,
                menuButton.StyleId);
        }
    }

    private void OnCloseClicked(
        object? sender,
        EventArgs e)
    {
        CloseRequested?.Invoke(
            this,
            EventArgs.Empty);
    }
}