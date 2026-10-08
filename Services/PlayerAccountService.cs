using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SkyLock.Models;
using SQLite;

    namespace SkyLock.Services
    {
        public class PlayerAccountService
    {
            private readonly SQLiteAsyncConnection database;
            private readonly Task initializationTask;

            public PlayerAccountService()
            {
                // Store the database in the app's data folder.
                string databasePath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    "skylock.db3");

                database = new SQLiteAsyncConnection(databasePath);

                // Create the accounts table if it does not exist.
                initializationTask =
                    database.CreateTableAsync<PlayerAccount>();
            }

            public async Task<int> AddAccountAsync(PlayerAccount account)
            {
                await initializationTask;

                // Store emails consistently for sign-in and duplicate checks.
                account.Email = account.Email.Trim().ToLowerInvariant();

                return await database.InsertAsync(account);
            }

            public async Task<PlayerAccount?> GetAccountByEmailAsync(
                string email)
            {
                await initializationTask;

                string normalizedEmail = email.Trim().ToLowerInvariant();

                return await database.Table<PlayerAccount>()
                    .Where(account => account.Email == normalizedEmail)
                    .FirstOrDefaultAsync();
            }
        }
    }