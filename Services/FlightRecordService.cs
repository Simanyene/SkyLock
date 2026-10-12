using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
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
        string databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "skylock.db3");

        database = new SQLiteAsyncConnection(databasePath);
        initializationTask = database.CreateTableAsync<FlightRecord>();
    }

    public async Task<int> AddRecordAsync(FlightRecord flightRecord)
    {
        ArgumentNullException.ThrowIfNull(flightRecord);

        if (flightRecord.GuessCount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(flightRecord.GuessCount));

        if (!double.IsFinite(flightRecord.TimeTakenSeconds) ||
            flightRecord.TimeTakenSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(flightRecord.TimeTakenSeconds));
        }

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

            return await database.Table<FlightRecord>()
                .Where(record => record.PlayerAccountId == accountId)
                .OrderByDescending(record => record.PlayedAtUtc)
                .ToListAsync();
        }

        return await database.Table<FlightRecord>()
            .Where(record => record.PlayerAccountId == null)
            .OrderByDescending(record => record.PlayedAtUtc)
            .ToListAsync();
    }

    public Task<List<FlightRecord>> GetCurrentPlayerRecordsAsync()
    {
        return GetRecordsAsync(PlayerSessionService.CurrentAccountId);
    }

    public async Task<FlightStatistics> GetStatisticsAsync(
        int? playerAccountId)
    {
        List<FlightRecord> records =
            await GetRecordsAsync(playerAccountId);

        List<FlightRecord> wins =
            records.Where(record => record.Won).ToList();

        return new FlightStatistics(
            Played: records.Count,
            Won: wins.Count,
            Lost: records.Count - wins.Count,
            FastestSeconds: wins.Count > 0
                ? wins.Min(record => record.TimeTakenSeconds)
                : null,
            FewestGuesses: wins.Count > 0
                ? wins.Min(record => record.GuessCount)
                : null);
    }

    public Task<FlightStatistics> GetCurrentPlayerStatisticsAsync()
    {
        return GetStatisticsAsync(
            PlayerSessionService.CurrentAccountId);
    }
}