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

    private void OnInputChanged(object? sender, TextChangedEventArgs e)
    {
        lblError.IsVisible = false;
    }

    private async void OnSendClicked(object? sender, EventArgs e)
    {
        if (isSending)
            return;

        lblError.IsVisible = false;

        string email = txtEmail.Text?.Trim() ?? "";

        if (!MailAddress.TryCreate(email, out var address)
            || address.Address != email)
        {
            ShowError(LocalizationService.T("err_bad_email"));
            txtEmail.Focus();
            return;
        }

        isSending = true;
        Content.IsEnabled = false;

        try
        {
            var account = await App.AccountService.GetAccountByEmailAsync(email);

            if (account is null)
            {
                ShowError(LocalizationService.T("reset_not_found"));
                return;
            }

            // Password recovery is not connected yet, so tell the player honestly.
            ShowError(LocalizationService.T("reset_unavailable"));
        }
        catch (Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(exception);
            ShowError(LocalizationService.T("reset_unavailable"));
        }
        finally
        {
            isSending = false;
            Content.IsEnabled = true;
        }
    }

    private async void OnBackClicked(object? sender, EventArgs e)
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