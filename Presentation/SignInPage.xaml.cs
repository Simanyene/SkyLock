using System.Net.Mail;
using Microsoft.Maui.Storage;
using SkyLock.Models;
using SkyLock.Services;

namespace SkyLock.Presentation;

public partial class SignInPage : ContentPage
{
    private const string RememberedEmailKey = "RememberedEmail";
    private bool isSigningIn;

    public SignInPage()
    {
        InitializeComponent();

        string savedEmail =
            Preferences.Default.Get(RememberedEmailKey, "");

        txtEmail.Text = savedEmail;
        chkRememberEmail.IsChecked =
            !string.IsNullOrEmpty(savedEmail);
    }

    private void OnEmailCompleted(object? sender, EventArgs e)
    {
        txtPassword.Focus();
    }

    private void OnInputChanged(
        object? sender, TextChangedEventArgs e)
    {
        lblError.IsVisible = false;
    }

    private void OnShowPasswordClicked(
        object? sender, EventArgs e)
    {
        txtPassword.IsPassword = !txtPassword.IsPassword;

        btnShowPassword.Text = txtPassword.IsPassword
            ? "Show"
            : "Hide";
    }

    private async void OnSignInClicked(
        object? sender, EventArgs e)
    {
        if (isSigningIn)
            return;

        lblError.IsVisible = false;

        string email = txtEmail.Text?.Trim() ?? "";
        string password = txtPassword.Text ?? "";

        if (!MailAddress.TryCreate(email, out var address)
            || address.Address != email)
        {
            ShowError("Please enter a valid email address.");
            txtEmail.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(password)
            || password.Length < 8
            || password.Length > 128)
        {
            ShowError("Please enter your password of 8 to 128 characters.");
            txtPassword.Focus();
            return;
        }

        isSigningIn = true;
        Content.IsEnabled = false;

        Button? btnSignIn = sender as Button;

        if (btnSignIn is not null)
            btnSignIn.Text = "Signing In...";

        try
        {
            // Find the account saved on this device.
            PlayerAccount? account =
                await App.AccountService.GetAccountByEmailAsync(email);

            if (account is null)
            {
                ShowError("Incorrect email or password.");
                return;
            }

            // Check the password without blocking the screen.
            bool passwordIsCorrect = await Task.Run(() =>
                PasswordHashService.VerifyPassword(
                    password,
                    account.PasswordHash,
                    account.PasswordSalt));

            if (!passwordIsCorrect)
            {
                ShowError("Incorrect email or password.");
                return;
            }

            var currentWindow = Window;

            if (currentWindow is null)
            {
                ShowError("Unable to open Home. Please try again.");
                return;
            }

            // Remember the email only after successful sign-in.
            if (chkRememberEmail.IsChecked)
            {
                Preferences.Default.Set(
                    RememberedEmailKey, account.Email);
            }
            else
            {
                Preferences.Default.Remove(RememberedEmailKey);
            }

            // Pass the verified account's ID to Home.
            var homePage = new HomePage(account.Id);

            txtPassword.Text = "";

            currentWindow.Page = new NavigationPage(homePage);
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            ShowError("Sign-in could not be completed. Please try again.");
        }
        finally
        {
            isSigningIn = false;
            Content.IsEnabled = true;

            if (btnSignIn is not null)
                btnSignIn.Text = "Sign In";
        }
    }
    private async void OnForgotPasswordClicked(
    object? sender, EventArgs e)
    {
        if (isSigningIn)
            return;

        await Navigation.PushModalAsync(new ForgotPasswordPage());
    }

    private async void OnRegisterClicked(
        object? sender, EventArgs e)
    {
        if (isSigningIn)
            return;

        await Navigation.PushModalAsync(new RegisterPage());
    }

    private async void OnBackClicked(
        object? sender, EventArgs e)
    {
        if (isSigningIn)
            return;

        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        if (isSigningIn)
            return true;

        return base.OnBackButtonPressed();
    }

    private void ShowError(string message)
    {
        lblError.Text = message;
        lblError.IsVisible = true;
    }
}