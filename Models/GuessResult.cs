using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyLock.Models;

public record GuessResult(
    int Hits,
    int Matches,
    bool[] HitPositions);