using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SkyLock.Models;
using SQLite;

namespace SkyLock.Services;

public class PlayerAccountService
{
    private readonly SQLiteAsyncConnection database;
    private readonly Task initializationTask;

    public PlayerAccountService()
    {
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "skylock.db3");

        database = new SQLiteAsyncConnection(databasePath);
        initializationTask = database.CreateTableAsync<PlayerAccount>();
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        string cleaned = email.Trim();

        return MailAddress.TryCreate(cleaned, out var address)
            && address.Address.Equals(
                cleaned, StringComparison.OrdinalIgnoreCase)
            && address.Host.Contains('.');
    }

    public async Task<PlayerAccount?> GetAccountByEmailAsync(
        string email)
    {
        await initializationTask;

        string normalizedEmail = NormalizeEmail(email);

        return await database.Table<PlayerAccount>()
            .Where(account => account.Email == normalizedEmail)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await GetAccountByEmailAsync(email) is not null;
    }

    // Retained for callers that already construct a hashed account.
    public async Task<int> AddAccountAsync(PlayerAccount account)
    {
        ArgumentNullException.ThrowIfNull(account);

        await initializationTask;

        account.PilotName = account.PilotName.Trim();

        if (account.PilotName.Length < 3)
            throw new ArgumentException(
                "Pilot name must contain at least 3 characters.");

        if (!IsValidEmail(account.Email))
            throw new ArgumentException("Enter a valid email address.");

        if (string.IsNullOrWhiteSpace(account.PasswordHash) ||
            string.IsNullOrWhiteSpace(account.PasswordSalt))
        {
            throw new ArgumentException(
                "The account must have a password hash and salt.");
        }

        account.Email = NormalizeEmail(account.Email);

        return await database.InsertAsync(account);
    }

    // Returns null on success, otherwise a translation key.
    public async Task<string?> RegisterAsync(
        string pilotName,
        string email,
        string password)
    {
        pilotName = pilotName.Trim();

        if (pilotName.Length < 3)
            return "err_short_user";

        if (!IsValidEmail(email))
            return "err_bad_email";

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return "err_short_password";

        if (await EmailExistsAsync(email))
            return "err_email_exists";

        // Hashing runs away from the UI thread.
        var credentials = await Task.Run(() =>
        {
            string hash = PasswordHashService.CreateHash(
                password, out string salt);

            return (Hash: hash, Salt: salt);
        });

        var account = new PlayerAccount
        {
            PilotName = pilotName,
            Email = NormalizeEmail(email),
            PasswordHash = credentials.Hash,
            PasswordSalt = credentials.Salt
        };

        try
        {
            await AddAccountAsync(account);
        }
        catch (SQLiteException ex)
            when (ex.Result == SQLite3.Result.Constraint)
        {
            // Handles another insert using the same email.
            return "err_email_exists";
        }

        PlayerSessionService.SetCurrentPlayer(account);

        return null;
    }

    // Returns null on success, otherwise a translation key.
    public async Task<string?> SignInAsync(
        string email,
        string password)
    {
        if (!IsValidEmail(email))
            return "err_bad_email";

        PlayerAccount? account = await GetAccountByEmailAsync(email);

        if (account is null)
            return "err_bad_login";

        bool validPassword = await Task.Run(() =>
            PasswordHashService.VerifyPassword(
                password,
                account.PasswordHash,
                account.PasswordSalt));

        if (!validPassword)
            return "err_bad_login";

        PlayerSessionService.SetCurrentPlayer(account);

        return null;
    }

    public async Task<PlayerAccount?> GetAccountByIdAsync(int accountId)
    {
        await initializationTask;

        return await database.Table<PlayerAccount>()
            .Where(account => account.Id == accountId)
            .FirstOrDefaultAsync();
    }
}