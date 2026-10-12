using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace SkyLock.Models;

public class FlightRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Null means the game was played as a guest.
    public int? PlayerAccountId { get; set; }

    public string Difficulty { get; set; } = "Easy";

    public int DigitCount { get; set; }

    public int TimeLimitSeconds { get; set; }

    public int GuessCount { get; set; }

    public double TimeTakenSeconds { get; set; }

    public bool Won { get; set; }

    public DateTime PlayedAtUtc { get; set; } = DateTime.UtcNow;
}