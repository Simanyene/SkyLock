using System;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class ProfilePage : ContentPage
{
    private readonly int? playerAccountId;
    private readonly ImageButton[] avatarButtons;

    public ProfilePage() : this(null)
    {
    }

    public ProfilePage(int? accountId)
    {
        InitializeComponent();

        playerAccountId = accountId;

        avatarButtons = new[]
        {
            btnAvatarMaleOne,
            btnAvatarMaleTwo,
            btnAvatarFemaleOne,
            btnAvatarFemaleTwo,
            btnAvatarNeutral
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ShowSelectedAvatar();
    }

    private void ShowSelectedAvatar()
    {
        int selectedIndex =
            PlayerProfileService.GetAvatarIndex(playerAccountId);

        imgCurrentAvatar.Source =
            PlayerProfileService.GetAvatarImage(playerAccountId);

        for (int avatarIndex = 0;
             avatarIndex < avatarButtons.Length;
             avatarIndex++)
        {
            string state = avatarIndex == selectedIndex
                ? "Selected"
                : "Normal";

            VisualStateManager.GoToState(
                avatarButtons[avatarIndex],
                state);
        }
    }

    private void OnAvatarClicked(object? sender, EventArgs e)
    {
        if (sender is not ImageButton selectedButton)
            return;

        if (!int.TryParse(
                selectedButton.StyleId,
                out int selectedIndex))
        {
            return;
        }

        PlayerProfileService.SetAvatarIndex(
            playerAccountId,
            selectedIndex);

        ShowSelectedAvatar();

        lblProfileMessage.Text = "Your avatar has been saved.";
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}