using Microsoft.Maui.Controls;
using SkyLock.Services;
using System;
using System.Net.Mail;
using System.Xml;

namespace SkyLock.Presentation;

public partial class ForgotPasswordPage : ContentPage
{
    private bool isSending;

    public ForgotPasswordPage()
    {
        InitializeComponent();
        ApplyLocalizedText();
    }

    private void ApplyLocalizedText()
    {
        txtEmail.Placeholder = LocalizationService.T("email");
        btnSend.Text = LocalizationService.T("send_reset");
        btnBack.Text = LocalizationService.T("back_to_login");
    }

    private void OnInputChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        lblError.IsVisible = false;
    }

    private async void OnSendClicked(
        object? sender,
        EventArgs e)
    {
        if (isSending)
            return;

        lblError.IsVisible = false;

        string email = txtEmail.Text?.Trim() ?? "";

        // Validate the email address.
        if (!MailAddress.TryCreate(email, out var address)
            || address.Address != email)
        {
            ShowError("Please enter a valid email address.");
            txtEmail.Focus();
            return;
        }

        isSending = true;
        btnSend.IsEnabled = false;
        btnBack.IsEnabled = false;

        try
        {
            // Check whether the account exists.
            var account =
                await App.AccountService
                    .GetAccountByEmailAsync(email);

            if (account is null)
            {
                ShowError("No account was found with this email.");
                return;
            }

            // A real reset service still needs to be connected.
            ShowError(
                "Password recovery is currently unavailable. " +
                "Please try again later.");
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);

            ShowError(
                "Unable to process your request right now.");
        }
        finally
        {
            isSending = false;
            btnSend.IsEnabled = true;
            btnBack.IsEnabled = true;
        }
    }

    private async void OnBackClicked(
        object? sender,
        EventArgs e)
    {
        if (isSending)
            return;

        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        if (isSending)
            return true;

        return base.OnBackButtonPressed();
    }

    private void ShowError(string message)
    {
        lblError.Text = message;
        lblError.IsVisible = true;
    }
}
