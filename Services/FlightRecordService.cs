using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using SkyLock.Models;
using SQLite;

namespace SkyLock.Services;

public class FlightRecordService
{
    private readonly SQLiteAsyncConnection database;
    private readonly Task initializationTask;

    public FlightRecordService()
    {
        // Use the same database file as player accounts.
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "skylock.db3");

        database = new SQLiteAsyncConnection(databasePath);

        // Create the flight records table if it does not exist.
        initializationTask =
            database.CreateTableAsync<FlightRecord>();
    }

    public async Task<int> AddRecordAsync(FlightRecord flightRecord)
    {
        await initializationTask;

        return await database.InsertAsync(flightRecord);
    }

    public async Task<List<FlightRecord>> GetRecordsAsync(
        int? playerAccountId)
    {
        await initializationTask;

        if (playerAccountId.HasValue)
        {
            int accountId = playerAccountId.Value;

            // Return only this account's results.
            return await database.Table<FlightRecord>()
                .Where(record => record.PlayerAccountId == accountId)
                .OrderByDescending(record => record.PlayedAtUtc)
                .ToListAsync();
        }

        // Guest results are kept separately from account results.
        return await database.Table<FlightRecord>()
            .Where(record => record.PlayerAccountId == null)
            .OrderByDescending(record => record.PlayedAtUtc)
            .ToListAsync();
    }
}