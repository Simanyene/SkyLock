using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyLock.Models;

public record FlightStatistics(
    int Played,
    int Won,
    int Lost,
    double? FastestSeconds,
    int? FewestGuesses);