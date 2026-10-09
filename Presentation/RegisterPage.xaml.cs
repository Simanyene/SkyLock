using System.Net.Mail;
using SkyLock.Models;
using SkyLock.Services;

namespace SkyLock.Presentation
{
        public partial class RegisterPage : ContentPage
        {
            private bool isCreatingAccount;

            public RegisterPage()
            {
                InitializeComponent();
            }

            private void OnShowPasswordClicked(object? sender, EventArgs e)
            {
                // Reveal or hide the password.
                txtPassword.IsPassword = !txtPassword.IsPassword;

                btnShowPassword.Text =
                    txtPassword.IsPassword ? "Show" : "Hide";
            }

            private void OnShowConfirmPasswordClicked(
                object? sender, EventArgs e)
            {
                // Reveal or hide the confirmation password.
                txtConfirmPassword.IsPassword =
                    !txtConfirmPassword.IsPassword;

                btnShowConfirmPassword.Text =
                    txtConfirmPassword.IsPassword ? "Show" : "Hide";
            }

            private async void OnCreateAccountClicked(
                object? sender, EventArgs e)
            {
                if (isCreatingAccount)
                    return;

                lblError.IsVisible = false;

                string pilotName = txtPilotName.Text?.Trim() ?? "";
                string email = txtEmail.Text?.Trim() ?? "";
                string password = txtPassword.Text ?? "";
                string confirmPassword = txtConfirmPassword.Text ?? "";

                if (pilotName.Length < 2 || pilotName.Length > 20)
                {
                    ShowError("Your Pilot name must contain 2 to 20 characters.");
                    txtPilotName.Focus();
                    return;
                }

                if (!MailAddress.TryCreate(email, out var address)
                    || address.Address != email)
                {
                    ShowError("Please enter a valid email address.");
                    txtEmail.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(password)
                    || password.Length < 8 || password.Length > 128)
                {
                    ShowError("Your password must contain 8 to 128 characters.");
                    txtPassword.Focus();
                    return;
                }

                if (password != confirmPassword)
                {
                    ShowError("Your passwords do not match.");
                    txtConfirmPassword.Focus();
                    return;
                }

                isCreatingAccount = true;
                btnCreateAccount.IsEnabled = false;
                btnBackToSignIn.IsEnabled = false;
                btnCreateAccount.Text = "Creating Account...";

                try
                {
                    PlayerAccount? existingAccount =
                        await App.AccountService.GetAccountByEmailAsync(email);

                    if (existingAccount is not null)
                    {
                        ShowError(
                            "An account with this email already exists on this device.");
                        return;
                    }

                    // Hash the password without blocking the screen.
                    PlayerAccount account = await Task.Run(() =>
                    {
                        string hash = PasswordHashService.CreateHash(
                            password, out string salt);

                        return new PlayerAccount
                        {
                            PilotName = pilotName,
                            Email = email,
                            PasswordHash = hash,
                            PasswordSalt = salt
                        };
                    });

                    await App.AccountService.AddAccountAsync(account);

                    // Clear the passwords and reset their visibility.
                    txtPassword.Text = "";
                    txtConfirmPassword.Text = "";

                    txtPassword.IsPassword = true;
                    txtConfirmPassword.IsPassword = true;

                    btnShowPassword.Text = "Show";
                    btnShowConfirmPassword.Text = "Show";

                    await DisplayAlert(
                        "Account Created",
                        "Your account has been saved on this device. You can now return to Sign In.",
                        "OK");

                    await Navigation.PopModalAsync();
                }
                catch (Exception)
                {
                    ShowError(
                        "Registration could not be completed. Please try again.");
                }
                finally
                {
                    isCreatingAccount = false;
                    btnCreateAccount.IsEnabled = true;
                    btnBackToSignIn.IsEnabled = true;
                    btnCreateAccount.Text = "Create Account";
                }
            }

            private async void OnBackToSignInClicked(
                object? sender, EventArgs e)
            {
                if (isCreatingAccount)
                    return;

                await Navigation.PopModalAsync();
            }

            protected override bool OnBackButtonPressed()
            {
                // Keep the page open while saving the account.
                if (isCreatingAccount)
                    return true;

                return base.OnBackButtonPressed();
            }

            private void ShowError(string message)
            {
                lblError.Text = message;
                lblError.IsVisible = true;
            }
        }
    }