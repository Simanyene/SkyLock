
using System;
using Microsoft.Maui.Storage;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class ProfilePage : ContentPage
{
    private readonly int? playerAccountId;
    private readonly ImageButton[] avatarButtons;

    private int selectedAvatarIndex;
    private bool isSaving;

    // Constructor for guest players.
    public ProfilePage() : this(
        PlayerSessionService.CurrentAccountId)
    {
    }

    // Constructor for signed-in players.
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

    // Load the current player's information.
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        lblProfileMessage.Text = "";

        selectedAvatarIndex =
            PlayerProfileService.GetAvatarIndex(
                playerAccountId);

        ShowSelectedAvatar();

        try
        {
            if (playerAccountId.HasValue)
            {
                var account =
                    await App.AccountService
                        .GetAccountByIdAsync(
                            playerAccountId.Value);

                txtPilotName.Text =
                    account?.PilotName ?? "";
            }
            else
            {
                txtPilotName.Text =
                    Preferences.Default.Get(
                        "Profile.GuestName",
                        "Guest");
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            lblProfileMessage.Text =
                "Unable to load your profile.";
        }
    }

    // Display the avatar currently selected.
    private void ShowSelectedAvatar()
    {
        imgCurrentAvatar.Source =
            PlayerProfileService.AvatarImages[
                selectedAvatarIndex];

        for (int index = 0;
             index < avatarButtons.Length;
             index++)
        {
            string state =
                index == selectedAvatarIndex
                    ? "Selected"
                    : "Normal";

            VisualStateManager.GoToState(
                avatarButtons[index],
                state);
        }
    }

    // Change the selected avatar.
    private void OnAvatarClicked(
        object? sender,
        EventArgs e)
    {
        if (isSaving)
            return;

        if (sender is not ImageButton button)
            return;

        if (!int.TryParse(
            button.StyleId,
            out int avatarIndex))
        {
            return;
        }

        if (avatarIndex < 0 ||
            avatarIndex >= avatarButtons.Length)
        {
            return;
        }

        selectedAvatarIndex = avatarIndex;

        ShowSelectedAvatar();

        lblProfileMessage.Text =
            "Press Save Changes to update your profile.";
    }

    // Save the username and selected avatar.
    private async void OnSaveProfileClicked(
        object? sender,
        EventArgs e)
    {
        if (isSaving)
            return;

        string pilotName =
            txtPilotName.Text?.Trim() ?? "";

        // Check username length.
        if (pilotName.Length < 3)
        {
            lblProfileMessage.Text =
                "Username must contain at least 3 characters.";

            return;
        }

        if (pilotName.Length > 30)
        {
            lblProfileMessage.Text =
                "Username cannot exceed 30 characters.";

            return;
        }

        isSaving = true;

        btnSaveProfile.IsEnabled = false;

        lblProfileMessage.Text =
            "Saving profile...";

        try
        {
            if (playerAccountId.HasValue)
            {
                // Update registered player's SQLite account.
                bool updated =
                    await App.AccountService
                        .UpdatePilotNameAsync(
                            playerAccountId.Value,
                            pilotName);

                if (!updated)
                {
                    lblProfileMessage.Text =
                        "Account not found. Profile not saved.";

                    return;
                }
            }
            else
            {
                // Save guest username on this device.
                Preferences.Default.Set(
                    "Profile.GuestName",
                    pilotName);
            }

            // Save the selected avatar.
            PlayerProfileService.SetAvatarIndex(
                playerAccountId,
                selectedAvatarIndex);

            // Update the username in the current session.
            PlayerSessionService.UpdateCurrentName(
                playerAccountId,
                pilotName);

            txtPilotName.Text = pilotName;

            lblProfileMessage.Text =
                "Profile updated successfully!";

            await DisplayAlert(
                "Profile Saved",
                "Your username and avatar have been updated.",
                "OK");
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            lblProfileMessage.Text =
                "Unable to save profile. Please try again.";
        }
        finally
        {
            isSaving = false;

            btnSaveProfile.IsEnabled = true;
        }
    }

    // Return to the previous page.
    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        if (isSaving)
            return;

        if (Navigation.NavigationStack.Count > 1)
        {
            await Navigation.PopAsync();
        }
        else if (Window is not null)
        {
            Window.Page =
                new NavigationPage(
                    new HomePage(playerAccountId));
        }
    }
}
