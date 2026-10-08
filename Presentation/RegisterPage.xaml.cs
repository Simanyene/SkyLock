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

        private async void OnCreateAccountClicked(
            object? sender, EventArgs e)
        {
            if (isCreatingAccount)
                return;

            lblError.IsVisible = false;

            string gamerName = txtGamerName.Text?.Trim() ?? "";
            string email = txtEmail.Text?.Trim() ?? "";
            string password = txtPassword.Text ?? "";
            string confirmPassword = txtConfirmPassword.Text ?? "";

            if (gamerName.Length < 2 || gamerName.Length > 20)
            {
                ShowError("Your gamer name must contain 2 to 20 characters.");
                txtGamerName.Focus();
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
                    ShowError("An account with this email already exists on this device.");
                    return;
                }

                // Perform password hashing without blocking the screen.
                PlayerAccount account = await Task.Run(() =>
                {
                    string hash = PasswordService.CreateHash(
                        password, out string salt);

                    return new PlayerAccount
                    {
                        GamerName = gamerName,
                        Email = email,
                        PasswordHash = hash,
                        PasswordSalt = salt
                    };
                });

                await App.AccountService.AddAccountAsync(account);

                txtPassword.Text = "";
                txtConfirmPassword.Text = "";

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