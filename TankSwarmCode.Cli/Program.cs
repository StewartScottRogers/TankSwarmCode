using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TankSwarmCode.Arena;
using TankSwarmCode.SwarmTank;

const int DefaultMaxTicks = 5000;
const double DefaultArenaWidth = 800;
const double DefaultArenaHeight = 600;

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
var cliArgs = Args.Parse(args);

// --list mode: discover tanks in a DLL
if (cliArgs.List is not null)
{
    var asm = Assembly.LoadFrom(Path.GetFullPath(cliArgs.List));
    var types = asm.GetTypes()
        .Where(t => !t.IsAbstract && t.IsClass && typeof(ISwarmTank).IsAssignableFrom(t))
        .OrderBy(t => t.Name)
        .ToList();

    Console.WriteLine($"Tanks in {Path.GetFileName(cliArgs.List)} ({types.Count} found):");
    foreach (var t in types)
    {
        bool usable = t.GetConstructor(Type.EmptyTypes) != null;
        Console.WriteLine($"  {t.Name,-28} {(usable ? "ok" : "skipped (no parameterless constructor)")}");
    }
    return 0;
}

if (cliArgs.Bot1 is null || cliArgs.Bot2 is null)
{
    Console.Error.WriteLine("""
        Usage:
          TankSwarmCode.Cli --bot1 <dll> --bot2 <dll> [options]
          TankSwarmCode.Cli --list <dll>

        Match options:
          --bot1 <dll>        Swarm 1 bot DLL
          --bot2 <dll>        Swarm 2 bot DLL
          --seed <int>        Random seed (incremented per match in batch)
          --max-ticks <int>   Max ticks per match (default: 5000)
          --batch <N>         Run N matches; streams NDJSON, one line per match
          --parallel <N>      Run up to N matches concurrently (default: 1)
          --on-timeout <rule> When max-ticks is reached: draw (default) | energy | survivors
                              energy:    winner = swarm with most total energy remaining
                              survivors: winner = swarm with most living tanks (tie = draw)

        Arena options:
          --width <double>    Arena width in pixels (default: 800)
          --height <double>   Arena height in pixels (default: 600)

        Output options:
          --summary           Append aggregate stats after batch
          --format <fmt>      json (default) | table | csv

        Discovery:
          --list <dll>        List all ISwarmTank types in a DLL
        """);
    return 1;
}

double arenaWidth       = cliArgs.Width  ?? DefaultArenaWidth;
double arenaHeight      = cliArgs.Height ?? DefaultArenaHeight;
int matchCount          = cliArgs.Batch  ?? 1;
int parallelism         = cliArgs.Parallel ?? 1;
bool isNdjson           = matchCount > 1;
bool tableFormat        = string.Equals(cliArgs.Format, "table", StringComparison.OrdinalIgnoreCase);
bool csvFormat          = string.Equals(cliArgs.Format, "csv",   StringComparison.OrdinalIgnoreCase);
string timeoutPolicy    = cliArgs.OnTimeout ?? "draw";

// Pre-allocate results array so parallel writes are index-safe
var resultsArr = new MatchResult[matchCount];

if (csvFormat)
    Console.WriteLine("match,winner_swarm_id,ticks,timed_out,swarm1_survivors,swarm2_survivors,swarm1_energy,swarm2_energy,first_kill_tick,seed");

var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = parallelism };
Parallel.For(0, matchCount, parallelOptions, idx =>
{
    int matchNum = idx + 1;
    int? seed = cliArgs.Seed.HasValue ? cliArgs.Seed.Value + idx : null;
    var result = RunMatch(cliArgs.Bot1, cliArgs.Bot2, seed, cliArgs.MaxTicks ?? DefaultMaxTicks, arenaWidth, arenaHeight, timeoutPolicy);
    resultsArr[idx] = result;

    if (!tableFormat && !csvFormat)
    {
        string line;
        if (isNdjson)
        {
            var obj = new { match = matchNum, result.winner_swarm_id, result.ticks, result.timed_out,
                            result.swarm1_survivors, result.swarm2_survivors,
                            result.swarm1_energy, result.swarm2_energy,
                            result.swarm1_survivor_names, result.swarm2_survivor_names,
                            result.first_kill_tick, seed, result.tanks };
            line = JsonSerializer.Serialize(obj, jsonOptions);
        }
        else
        {
            line = JsonSerializer.Serialize(result, jsonOptions);
        }
        // Atomic line write — prevents interleaved output when parallel > 1
        lock (Console.Out) Console.WriteLine(line);
    }

    if (csvFormat)
    {
        string csvLine = $"{matchNum},{result.winner_swarm_id?.ToString() ?? ""},{result.ticks},{result.timed_out},{result.swarm1_survivors},{result.swarm2_survivors},{result.swarm1_energy},{result.swarm2_energy},{result.first_kill_tick?.ToString() ?? ""},{seed?.ToString() ?? ""}";
        lock (Console.Out) Console.WriteLine(csvLine);
    }
});

var results = resultsArr.ToList();

// Table output (collected)
if (tableFormat)
    PrintTable(results, cliArgs.Bot1, cliArgs.Bot2, arenaWidth, arenaHeight, timeoutPolicy);

// Summary (JSON mode: opt-in via --summary; table mode: always)
if (cliArgs.Summary || tableFormat)
    PrintSummary(results, tableFormat, jsonOptions, ShortName(cliArgs.Bot1), ShortName(cliArgs.Bot2));

return 0;

// ── Match runner ────────────────────────────────────────────────────────────

static MatchResult RunMatch(string bot1Dll, string bot2Dll, int? seed, int maxTicks, double width, double height, string timeoutPolicy)
{
    var engine = new ArenaEngine(width, height, seed);

    LoadTanks(bot1Dll, swarmId: 1).ForEach(engine.AddTank);
    LoadTanks(bot2Dll, swarmId: 2).ForEach(engine.AddTank);

    engine.Start();

    // Track net damage absorbed per tank: sum of per-tick energy losses.
    // Understates slightly when a tank simultaneously takes damage AND gains
    // energy return from a hit (those two are netted in the same tick snapshot).
    var damageTaken  = engine.Tanks.ToDictionary(t => t.Name, _ => 0.0);
    var energyGained = engine.Tanks.ToDictionary(t => t.Name, _ => 0.0);
    var prevEnergy   = engine.Tanks.ToDictionary(t => t.Name, t => t.State.Energy);
    engine.TickCompleted += (_, _) =>
    {
        foreach (var t in engine.Tanks)
        {
            double delta = t.State.Energy - prevEnergy[t.Name];
            if (delta < 0) damageTaken[t.Name]  += -delta;
            else           energyGained[t.Name] +=  delta;
            prevEnergy[t.Name] = t.State.Energy;
        }
    };

    while (engine.IsRunning && engine.TickNumber < maxTicks)
        engine.Tick();

    bool timedOut = engine.IsRunning;

    var swarmsSurviving = engine.Tanks
        .Where(t => t.State.IsAlive)
        .Select(t => t.SwarmId)
        .Distinct()
        .ToList();

    int? winnerId = swarmsSurviving.Count == 1 ? swarmsSurviving[0] : null;

    // Apply timeout policy when the match hit max-ticks with multiple survivors
    if (timedOut && winnerId is null && timeoutPolicy != "draw")
    {
        var living = engine.Tanks.Where(t => t.State.IsAlive).ToList();
        if (timeoutPolicy == "energy")
        {
            double e1 = living.Where(t => t.SwarmId == 1).Sum(t => t.State.Energy);
            double e2 = living.Where(t => t.SwarmId == 2).Sum(t => t.State.Energy);
            if (e1 != e2) winnerId = e1 > e2 ? 1 : 2;
        }
        else if (timeoutPolicy == "survivors")
        {
            int c1 = living.Count(t => t.SwarmId == 1);
            int c2 = living.Count(t => t.SwarmId == 2);
            if (c1 != c2) winnerId = c1 > c2 ? 1 : 2;
        }
    }

    var tanks = engine.Tanks
        .Select(t => new TankResult(t.Name, t.SwarmId, t.Role.ToString(),
                                    t.State.IsAlive, Math.Round(t.State.Energy, 2),
                                    t.State.DestroyedAtTick,
                                    Math.Round(damageTaken[t.Name],  1),
                                    Math.Round(energyGained[t.Name], 1)))
        .ToList();

    int s1 = tanks.Count(t => t.swarm_id == 1 && t.survived);
    int s2 = tanks.Count(t => t.swarm_id == 2 && t.survived);
    var s1Names = tanks.Where(t => t.swarm_id == 1 && t.survived).Select(t => t.name).ToList();
    var s2Names = tanks.Where(t => t.swarm_id == 2 && t.survived).Select(t => t.name).ToList();
    double s1Energy = Math.Round(tanks.Where(t => t.swarm_id == 1 && t.survived).Sum(t => t.energy), 2);
    double s2Energy = Math.Round(tanks.Where(t => t.swarm_id == 2 && t.survived).Sum(t => t.energy), 2);
    long? firstKill = tanks
        .Where(t => t.destroyed_at_tick > 0)
        .Select(t => (long?)t.destroyed_at_tick)
        .Min();

    return new MatchResult(winnerId, engine.TickNumber, timedOut, s1, s2, s1Energy, s2Energy, s1Names, s2Names, firstKill, tanks);
}

static List<ISwarmTank> LoadTanks(string dllPath, int swarmId)
{
    var asm = Assembly.LoadFrom(Path.GetFullPath(dllPath));
    var tanks = asm.GetTypes()
        .Where(t => !t.IsAbstract && t.IsClass && typeof(ISwarmTank).IsAssignableFrom(t)
                    && t.GetConstructor(Type.EmptyTypes) != null)
        .Select(t => (ISwarmTank)Activator.CreateInstance(t)!)
        .ToList();

    foreach (var tank in tanks)
    {
        var prop = tank.GetType().GetProperty("SwarmId");
        if (prop?.CanWrite == true) prop.SetValue(tank, swarmId);
    }

    return tanks;
}

// ── Output helpers ──────────────────────────────────────────────────────────

static void PrintTable(List<MatchResult> results, string bot1, string bot2,
                        double arenaW = 800, double arenaH = 600, string timeoutPolicy = "draw")
{
    string b1 = ShortName(bot1);  // e.g. "Blue" from "TankSwarmCode.SwarmTanks.Blue"
    string b2 = ShortName(bot2);
    // Show 1 decimal for small energy values so "0 vs 0" can't hide a real difference
    static string E(double e) => e < 10 ? $"{e:F1}" : $"{e:F0}";

    // Context header — visible before scrolling through match rows
    if (results.Count > 1)
    {
        string toStr = timeoutPolicy != "draw" ? $"  |  --on-timeout {timeoutPolicy}" : "";
        string arenaStr = (arenaW != 800 || arenaH != 600) ? $"  |  arena {arenaW:F0}x{arenaH:F0}" : "";
        Console.WriteLine($"  {b1} vs {b2}  |  {results.Count} matches{toStr}{arenaStr}");

        // Quick result teaser for large batches so the punchline is visible before scrolling
        if (results.Count > 10)
        {
            int s1w = results.Count(r => r.winner_swarm_id == 1);
            int s2w = results.Count(r => r.winner_swarm_id == 2);
            int dw  = results.Count(r => r.winner_swarm_id == null);
            string drawStr = dw > 0 ? $"  |  draws: {dw}" : "";
            Console.WriteLine($"  {b1}: {s1w} wins ({100.0 * s1w / results.Count:F0}%)  |  {b2}: {s2w} wins ({100.0 * s2w / results.Count:F0}%){drawStr}");
        }
        Console.WriteLine();
    }

    if (results.Count == 1)
    {
        var r = results[0];
        string winner = r.winner_swarm_id == 1 ? $"{b1} (swarm 1)"
                      : r.winner_swarm_id == 2 ? $"{b2} (swarm 2)"
                      : r.swarm1_survivor_names.Count > 0 || r.swarm2_survivor_names.Count > 0
                      ? $"Draw: {string.Join(", ", r.swarm1_survivor_names)} vs {string.Join(", ", r.swarm2_survivor_names)}"
                      : "Draw";
        Console.WriteLine($"Winner: {winner}  |  Ticks: {r.ticks}{(r.timed_out ? " (timed out)" : "")}");
        Console.WriteLine($"Energy left: {b1} {r.swarm1_energy:F1}E  |  {b2} {r.swarm2_energy:F1}E");
        Console.WriteLine();
        Console.WriteLine($"  {"Tank",-20} {"Sw",2}  {"Role",-14} {"Surv",4}  {"Energy",7}  {"DmgTaken",9}  {"EGained",8}  {"Died@",6}");
        Console.WriteLine($"  {new string('-', 20)} {new string('-', 2)}  {new string('-', 14)} {new string('-', 4)}  {new string('-', 7)}  {new string('-', 9)}  {new string('-', 8)}  {new string('-', 6)}");
        foreach (var t in r.tanks)
            Console.WriteLine($"  {t.name,-20} {t.swarm_id,2}  {t.role,-14} {(t.survived ? "Yes" : "No"),4}  {t.energy,7:F1}  {t.damage_taken,9:F1}  {t.energy_gained,8:F1}  {(t.destroyed_at_tick > 0 ? t.destroyed_at_tick.ToString() : "-"),6}");

        // Death-order timeline
        Console.WriteLine();
        Console.WriteLine("  Death order:");
        var deaths = r.tanks
            .Where(t => !t.survived && t.destroyed_at_tick > 0)
            .OrderBy(t => t.destroyed_at_tick)
            .ToList();
        var survivors = r.tanks.Where(t => t.survived).OrderBy(t => t.swarm_id).ThenBy(t => t.name).ToList();
        int rank = 1;
        foreach (var t in deaths)
            Console.WriteLine($"    {rank++,2}. {t.name,-20} (swarm {t.swarm_id}) @ tick {t.destroyed_at_tick}");
        foreach (var t in survivors)
            Console.WriteLine($"     -  {t.name,-20} (swarm {t.swarm_id}) survived  {t.energy:F1}E");
    }
    else
    {
        Console.WriteLine($"  {"#",4}  {"Winner",-30}  {"Ticks",9}  {"S1-E",7}  {"S2-E",7}  {"Margin",7}  {"1stKill",8}");
        Console.WriteLine($"  {new string('-', 4)}  {new string('-', 30)}  {new string('-', 9)}  {new string('-', 7)}  {new string('-', 7)}  {new string('-', 7)}  {new string('-', 8)}");
        int sw1Size = results[0].tanks.Count(t => t.swarm_id == 1);
        int sw2Size = results[0].tanks.Count(t => t.swarm_id == 2);
        var sw1AllNames = results[0].tanks.Where(t => t.swarm_id == 1).Select(t => t.name).ToList();
        var sw2AllNames = results[0].tanks.Where(t => t.swarm_id == 2).Select(t => t.name).ToList();
        for (int i = 0; i < results.Count; i++)
        {
            var r = results[i];
            static string SurvLabel(string swarmName, int count, int swarmSize, double energy, List<string> names, List<string> allNames)
            {
                string eStr = E(energy);
                if (count == swarmSize)
                    return $"{swarmName} [full team, {eStr}E]";
                // When exactly 1 tank was lost, show the casualty name — more informative than listing N-1 survivors
                if (count == swarmSize - 1 && allNames.Count > 0)
                {
                    var lost = allNames.FirstOrDefault(n => !names.Contains(n));
                    if (lost != null)
                    {
                        string shortLost = lost.StartsWith(swarmName, StringComparison.OrdinalIgnoreCase) ? lost[swarmName.Length..] : lost;
                        return $"{swarmName} [-{shortLost}, {eStr}E]";
                    }
                }
                if (count <= 3 && names.Count > 0)
                {
                    // Strip swarm prefix (e.g. "Blue" from "BlueGuard") for compact display
                    var shortNames = names.Select(n => n.StartsWith(swarmName, StringComparison.OrdinalIgnoreCase)
                        ? n[swarmName.Length..] : n);
                    return $"{swarmName} [{string.Join("+", shortNames)}, {eStr}E]";
                }
                return $"{swarmName} ({count} left, {eStr}E)";
            }
            string winner = r.winner_swarm_id == 1
                ? SurvLabel(b1, r.swarm1_survivors, sw1Size, r.swarm1_energy, r.swarm1_survivor_names, sw1AllNames)
                : r.winner_swarm_id == 2
                ? SurvLabel(b2, r.swarm2_survivors, sw2Size, r.swarm2_energy, r.swarm2_survivor_names, sw2AllNames)
                : r.swarm1_survivor_names.Count > 0 || r.swarm2_survivor_names.Count > 0
                ? $"Draw: {string.Join(",", r.swarm1_survivor_names)} vs {string.Join(",", r.swarm2_survivor_names)}"
                : "Draw";
            string fk = r.first_kill_tick.HasValue ? r.first_kill_tick.Value.ToString() : "-";
            string toFlag = r.timed_out ? " TO" : "";
            string margin = r.timed_out ? $"+{E(Math.Abs(r.swarm1_energy - r.swarm2_energy))}E" : "";
            Console.WriteLine($"  {i+1,4}  {winner,-30}  {r.ticks,6}{toFlag,-3}  {E(r.swarm1_energy),7}  {E(r.swarm2_energy),7}  {margin,7}  {fk,8}");
        }
    }
}

static void PrintSummary(List<MatchResult> results, bool tableFormat,
                         JsonSerializerOptions jsonOptions, string bot1Name = "Swarm1", string bot2Name = "Swarm2")
{
    int total    = results.Count;
    int s1Wins   = results.Count(r => r.winner_swarm_id == 1);
    int s2Wins   = results.Count(r => r.winner_swarm_id == 2);
    int draws    = results.Count(r => r.winner_swarm_id == null);
    var decisiveTicks   = results.Where(r => !r.timed_out).Select(r => r.ticks).OrderBy(x => x).ToList();
    var s1DecisiveTicks = results.Where(r => !r.timed_out && r.winner_swarm_id == 1).Select(r => r.ticks).OrderBy(x => x).ToList();
    var s2DecisiveTicks = results.Where(r => !r.timed_out && r.winner_swarm_id == 2).Select(r => r.ticks).OrderBy(x => x).ToList();
    static long Median(List<long> sorted) => sorted[sorted.Count / 2];
    string decisiveTicksStr;
    if (decisiveTicks.Count > 0)
    {
        string perSwarm = "";
        if (s1DecisiveTicks.Count > 0 && s2DecisiveTicks.Count > 0)
        {
            long s1Med = Median(s1DecisiveTicks);
            long s2Med = Median(s2DecisiveTicks);
            string tempoNote = "";
            if (s1Med > 0 && s2Med > 0)
            {
                double ratio = (double)Math.Max(s1Med, s2Med) / Math.Min(s1Med, s2Med);
                if (ratio >= 1.3)
                {
                    string fasterName = s1Med < s2Med ? bot1Name : bot2Name;
                    tempoNote = $" ({fasterName} {ratio:F1}x faster)";
                }
            }
            perSwarm = $" — {bot1Name} {s1Med}t / {bot2Name} {s2Med}t{tempoNote}";
        }
        decisiveTicksStr = $"Decisive med: {Median(decisiveTicks)}t{perSwarm}";
    }
    else
    {
        decisiveTicksStr = "No decisive matches";
    }
    long? avgFirstKill = results
        .Where(r => r.first_kill_tick.HasValue)
        .Select(r => (long?)r.first_kill_tick!.Value)
        .ToList() is { Count: > 0 } fks
            ? (long)fks.Average(x => x!.Value)
            : null;

    if (tableFormat)
    {
        Console.WriteLine();
        Console.WriteLine($"  Matches: {total}  |  {bot1Name} wins: {s1Wins} ({Pct(s1Wins, total)})  |  {bot2Name} wins: {s2Wins} ({Pct(s2Wins, total)})  |  Draws: {draws} ({Pct(draws, total)})");

        // Win quality: avg energy margin for decisive wins + avg margin for TO wins
        var s1Margins   = results.Where(r => r.winner_swarm_id == 1 && !r.timed_out).Select(r => r.swarm1_energy - r.swarm2_energy).ToList();
        var s2Margins   = results.Where(r => r.winner_swarm_id == 2 && !r.timed_out).Select(r => r.swarm2_energy - r.swarm1_energy).ToList();
        var s1ToMargins = results.Where(r => r.winner_swarm_id == 1 && r.timed_out).Select(r => r.swarm1_energy - r.swarm2_energy).ToList();
        var s2ToMargins = results.Where(r => r.winner_swarm_id == 2 && r.timed_out).Select(r => r.swarm2_energy - r.swarm1_energy).ToList();
        if (s1Margins.Count > 0 || s2Margins.Count > 0 || s1ToMargins.Count > 0 || s2ToMargins.Count > 0)
        {
            string s1ToStr = s1ToMargins.Count > 0 ? $"{s1ToMargins.Count} TO +{s1ToMargins.Average():F0}E" : "0 TO";
            string s2ToStr = s2ToMargins.Count > 0 ? $"{s2ToMargins.Count} TO +{s2ToMargins.Average():F0}E" : "0 TO";
            int wq1Wins = s1Margins.Count + s1ToMargins.Count;
            int wq2Wins = s2Margins.Count + s2ToMargins.Count;
            // Show TO% when timeouts represent a meaningful share of a swarm's wins (> 20%)
            string s1ToPct = wq1Wins > 0 && s1ToMargins.Count > 0 && (double)s1ToMargins.Count / wq1Wins > 0.20
                ? $" ({100 * s1ToMargins.Count / wq1Wins}% via TO)" : "";
            string s2ToPct = wq2Wins > 0 && s2ToMargins.Count > 0 && (double)s2ToMargins.Count / wq2Wins > 0.20
                ? $" ({100 * s2ToMargins.Count / wq2Wins}% via TO)" : "";
            string s1q = s1Margins.Count > 0
                ? $"{bot1Name}: +{s1Margins.Average():F0}E decisive ({s1Margins.Count}), {s1ToStr}{s1ToPct}"
                : $"{bot1Name}: 0 decisive, {s1ToStr}{s1ToPct}";
            string s2q = s2Margins.Count > 0
                ? $"{bot2Name}: +{s2Margins.Average():F0}E decisive ({s2Margins.Count}), {s2ToStr}{s2ToPct}"
                : $"{bot2Name}: 0 decisive, {s2ToStr}{s2ToPct}";
            Console.WriteLine($"  Win quality: {s1q}  |  {s2q}");
        }
        // Median ticks-to-close: how long the match continued after first kill (non-timeout only)
        // Median is used because outlier long decisive matches skew the mean significantly
        var closeDelays = results
            .Where(r => !r.timed_out && r.first_kill_tick.HasValue)
            .Select(r => r.ticks - r.first_kill_tick!.Value)
            .OrderBy(x => x)
            .ToList();
        string closeStr = closeDelays.Count > 0
            ? $"  |  Median close after 1st kill: {closeDelays[closeDelays.Count / 2]}t"
            : "";
        // Per-victim avg first kill tick: reveals which swarm tends to strike earlier
        var fkByVictim = results
            .Where(r => r.first_kill_tick.HasValue)
            .Select(r =>
            {
                var earliest = r.tanks.Where(t => !t.survived).OrderBy(t => t.destroyed_at_tick).FirstOrDefault();
                return (victimSwarm: earliest?.swarm_id, tick: r.first_kill_tick!.Value);
            })
            .Where(x => x.victimSwarm.HasValue)
            .ToList();
        string fkSplitStr = "";
        if (avgFirstKill.HasValue && fkByVictim.Count > 0)
        {
            var v1 = fkByVictim.Where(x => x.victimSwarm == 1).Select(x => x.tick).ToList();
            var v2 = fkByVictim.Where(x => x.victimSwarm == 2).Select(x => x.tick).ToList();
            string v1s = v1.Count > 0 ? $"{bot1Name[0]}v:{v1.Average():F0}t" : "";
            string v2s = v2.Count > 0 ? $"{bot2Name[0]}v:{v2.Average():F0}t" : "";
            string both = string.Join("/", new[] { v1s, v2s }.Where(s => s.Length > 0));
            if (both.Length > 0) fkSplitStr = $" ({both})";
        }
        Console.WriteLine($"  {decisiveTicksStr}{(avgFirstKill.HasValue ? $"  |  Avg first kill: tick {avgFirstKill}{fkSplitStr}" : "")}{closeStr}");

        // Match speed distribution with per-winner breakdown
        static string WinSplit(List<MatchResult> rs, Func<MatchResult, bool> pred, string n1, string n2)
        {
            int s1 = rs.Count(r => pred(r) && r.winner_swarm_id == 1);
            int s2 = rs.Count(r => pred(r) && r.winner_swarm_id == 2);
            return s1 > 0 || s2 > 0 ? $" ({n1[0]}:{s1}/{n2[0]}:{s2})" : "";
        }
        int fast      = results.Count(r => r.ticks <= 200 && !r.timed_out);
        int medium    = results.Count(r => r.ticks > 200 && r.ticks <= 1000 && !r.timed_out);
        int slow      = results.Count(r => r.ticks > 1000 && !r.timed_out);
        int timedOutC = results.Count(r => r.timed_out);
        int coinFlips = results.Count(r => r.timed_out && Math.Abs(r.swarm1_energy - r.swarm2_energy) < 5.0);
        string cfNote = coinFlips > 0 ? $"  ({coinFlips} coin-flip <5E)" : "";
        string fastSplit   = WinSplit(results, r => r.ticks <= 200 && !r.timed_out, bot1Name, bot2Name);
        string medSplit    = WinSplit(results, r => r.ticks > 200 && r.ticks <= 1000 && !r.timed_out, bot1Name, bot2Name);
        string slowSplit   = slow > 0 ? WinSplit(results, r => r.ticks > 1000 && !r.timed_out, bot1Name, bot2Name) : "";
        string toSplit     = WinSplit(results, r => r.timed_out, bot1Name, bot2Name);
        int toDraw = results.Count(r => r.timed_out && r.winner_swarm_id == null);
        if (toDraw > 0)
            toSplit = toSplit.Length > 0 ? toSplit[..^1] + $"/D:{toDraw})" : $" (D:{toDraw})";
        Console.WriteLine($"  Speed: <=200t: {fast}{fastSplit}  201-1000t: {medium}{medSplit}  1001+t: {slow}{slowSplit}  timeout: {timedOutC}{toSplit}{cfNote}");

        // Pre-compute first-death name map (used for "First kill victim" breakdown and tank-level insights)
        var firstDeathByName = results
            .Select(r => r.tanks.Where(t => !t.survived && t.destroyed_at_tick > 0)
                                 .OrderBy(t => t.destroyed_at_tick)
                                 .FirstOrDefault()?.name)
            .Where(n => n != null)
            .GroupBy(n => n!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Pre-compute solo wins per tank: matches where the tank was the only survivor on the winning side
        // Track decisive vs timeout breakdown so Solo carry can surface "TO-only" vs mixed cases
        var soloWinsByTank = new Dictionary<string, (int total, int toWins)>();
        foreach (var r in results.Where(r => r.winner_swarm_id.HasValue))
        {
            var survivors = r.winner_swarm_id == 1 ? r.swarm1_survivor_names : r.swarm2_survivor_names;
            if (survivors.Count == 1)
            {
                var cur = soloWinsByTank.GetValueOrDefault(survivors[0], (total: 0, toWins: 0));
                soloWinsByTank[survivors[0]] = (cur.total + 1, cur.toWins + (r.timed_out ? 1 : 0));
            }
        }

        // First kill victim: which swarm took the first casualty
        var firstKillVictims = results
            .Select(r =>
            {
                var earliest = r.tanks.Where(t => t.destroyed_at_tick > 0).OrderBy(t => t.destroyed_at_tick).FirstOrDefault();
                return earliest?.swarm_id;
            })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
        if (firstKillVictims.Count > 0)
        {
            int fk1 = firstKillVictims.Count(id => id == 1);
            int fk2 = firstKillVictims.Count(id => id == 2);
            string fk1Pct = Pct(fk1, firstKillVictims.Count);
            string fk2Pct = Pct(fk2, firstKillVictims.Count);
            // Win rates when each side takes the first hit
            var matchesWithFk = results
                .Select(r =>
                {
                    var earliest = r.tanks.Where(t => t.destroyed_at_tick > 0).OrderBy(t => t.destroyed_at_tick).FirstOrDefault();
                    return (firstVictimSwarm: earliest?.swarm_id, r.winner_swarm_id);
                })
                .Where(x => x.firstVictimSwarm.HasValue)
                .ToList();
            int s1WinsWhenS1First = matchesWithFk.Count(x => x.firstVictimSwarm == 1 && x.winner_swarm_id == 1);
            int s1TotalWhenS1First = matchesWithFk.Count(x => x.firstVictimSwarm == 1);
            int s2WinsWhenS2First = matchesWithFk.Count(x => x.firstVictimSwarm == 2 && x.winner_swarm_id == 2);
            int s2TotalWhenS2First = matchesWithFk.Count(x => x.firstVictimSwarm == 2);
            string s1Rev = s1TotalWhenS1First > 0 ? $" [{bot1Name} still wins {Pct(s1WinsWhenS1First, s1TotalWhenS1First)}]" : "";
            string s2Rev = s2TotalWhenS2First > 0 ? $" [{bot2Name} still wins {Pct(s2WinsWhenS2First, s2TotalWhenS2First)}]" : "";
            Console.WriteLine($"  First kill victim: {bot1Name} in {fk1Pct} of matches{s1Rev}  |  {bot2Name} in {fk2Pct}{s2Rev}");
            // Per-tank first-kill breakdown, sorted descending — strip swarm prefix for readability
            string FkEntry(string name, string prefix) {
                int c = firstDeathByName.GetValueOrDefault(name, 0);
                string short_name = name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? name[prefix.Length..] : name;
                return $"{short_name}:{c}";
            }
            var sw1Names = results[0].tanks.Where(t => t.swarm_id == 1).Select(t => t.name).OrderBy(n => -firstDeathByName.GetValueOrDefault(n,0)).ToList();
            var sw2Names = results[0].tanks.Where(t => t.swarm_id == 2).Select(t => t.name).OrderBy(n => -firstDeathByName.GetValueOrDefault(n,0)).ToList();
            string b1fk = string.Join("  ", sw1Names.Select(n => FkEntry(n, bot1Name)));
            string b2fk = string.Join("  ", sw2Names.Select(n => FkEntry(n, bot2Name)));
            Console.WriteLine($"  First kills: {bot1Name}: {b1fk}  |  {bot2Name}: {b2fk}");
        }

        // Stalemate breakdown — show which tank pairs cause draws
        var drawResults = results.Where(r => r.winner_swarm_id == null).ToList();
        if (drawResults.Count > 0)
        {
            var pairs = drawResults
                .Select(r => string.Join(",", r.swarm1_survivor_names) + " vs " + string.Join(",", r.swarm2_survivor_names))
                .GroupBy(p => p)
                .OrderByDescending(g => g.Count())
                .Select(g => $"{g.Key} x{g.Count()}");
            Console.WriteLine($"  Stalemates: {string.Join("  |  ", pairs)}");
        }

        // Per-tank survival rates
        Console.WriteLine();
        // Pair each tank with its match outcome to compute win_survival_rate
        var tankStats = results
            .SelectMany(r => r.tanks.Select(t => (t, matchTicks: r.ticks, timedOut: r.timed_out, winnerId: r.winner_swarm_id)))
            .GroupBy(x => (x.t.name, x.t.swarm_id))
            .Select(g =>
            {
                int survived    = g.Count(x => x.t.survived);
                int winSurvived = g.Count(x => x.t.survived && x.winnerId == x.t.swarm_id);
                int swarmWins   = g.Count(x => x.winnerId == x.t.swarm_id);
                int matches     = g.Count();
                double avgE     = survived > 0 ? Math.Round(g.Where(x => x.t.survived).Average(x => x.t.energy), 1) : 0;
                double avgDmg   = Math.Round(g.Average(x => x.t.damage_taken),  1);
                double avgGain  = Math.Round(g.Average(x => x.t.energy_gained), 1);
                double avgAlive = g.Average(x => x.t.destroyed_at_tick > 0 ? x.t.destroyed_at_tick : (double)x.matchTicks);
                double avgDeath = g.Where(x => !x.t.survived && x.t.destroyed_at_tick > 0)
                                   .Select(x => (double)x.t.destroyed_at_tick)
                                   .DefaultIfEmpty(0).Average();
                // winRate: fraction of swarm wins where this tank was alive at end
                double winRate  = swarmWins > 0 ? Math.Round((double)winSurvived / swarmWins * 100, 0) : 0;
                // Use non-timeout matches for both avgAlive and rate — keeps numerator/denominator consistent
                var normalMatches = g.Where(x => !x.timedOut).ToList();
                double avgAliveNormal = normalMatches.Count > 0
                    ? normalMatches.Average(x => x.t.destroyed_at_tick > 0 ? x.t.destroyed_at_tick : (double)x.matchTicks)
                    : 0;
                double avgGainNormal = normalMatches.Count > 0 ? normalMatches.Average(x => x.t.energy_gained) : 0;
                double rate = avgAliveNormal > 0 ? Math.Round(avgGainNormal / avgAliveNormal * 100, 2) : 0;
                // Only count timeout survivals where the tank's swarm actually won — not "alive in defeat"
                int toSurvivals = g.Count(x => x.timedOut && x.t.survived && x.winnerId == x.t.swarm_id);
                string role = g.First().t.role;
                int firstDeaths = firstDeathByName.GetValueOrDefault(g.Key.name, 0);
                var sw = soloWinsByTank.GetValueOrDefault(g.Key.name, (total: 0, toWins: 0));
                int soloWins = sw.total;
                int soloWinsTO = sw.toWins;
                return (g.Key.name, g.Key.swarm_id, matches, survived, winSurvived, swarmWins, winRate, avgE, avgDmg, avgGain, rate, avgDeathTick: (long)avgDeath, avgAlive: (long)avgAlive, avgAliveNormal: (long)avgAliveNormal, toSurvivals, role, firstDeaths, soloWins, soloWinsTO);
            })
            .OrderBy(t => t.swarm_id).ThenByDescending(t => t.survived).ToList();

        // Header
        Console.WriteLine($"  {"Tank",-20} {"Surv",9}  {"WinSurv",13}  {"AvgAlive",8}  {"DmgTaken",9}  {"EGained",8}  {"Rate/100t",9}");
        Console.WriteLine($"  {new string('-', 20)} {new string('-', 9)}  {new string('-', 13)}  {new string('-', 8)}  {new string('-', 9)}  {new string('-', 8)}  {new string('-', 9)}");
        int prevSwarm = -1;
        foreach (var t in tankStats)
        {
            if (t.swarm_id != prevSwarm)
            {
                if (prevSwarm != -1) Console.WriteLine();
                string bname = t.swarm_id == 1 ? bot1Name : bot2Name;
                Console.WriteLine($"  Swarm {t.swarm_id} [{bname}] ({t.swarmWins} wins):");
                prevSwarm = t.swarm_id;
            }
            // decisiveSurv = survived in decisive wins only (winSurvived minus TO wins)
            int decisiveSurv = t.winSurvived - t.toSurvivals;
            int lossSurv = t.survived - t.winSurvived;
            string lossSuffix = lossSurv > 0 ? $"+{lossSurv}L" : "";
            string survPct = t.toSurvivals > 0
                ? $"{t.survived}/{t.matches} ({decisiveSurv}D+{t.toSurvivals}TO{lossSuffix})"
                : lossSurv > 0
                    ? $"{t.survived}/{t.matches} ({t.winSurvived}W+{lossSurv}L)"
                    : $"{t.survived}/{t.matches} ({100.0 * t.survived / t.matches:F0}%)";
            // WinSurv: fraction + avg energy when surviving (tells "barely alive" vs "dominant survivor")
            string eNote = t.survived > 0 ? $" ({t.avgE:F0}E)" : "";
            string winPct  = t.swarmWins > 0 ? $"{t.winSurvived}/{t.swarmWins}{eNote}" : "n/a";
            string aliveStr = t.toSurvivals > 0
                ? $"{t.avgAliveNormal}+{t.toSurvivals}TO"
                : t.avgAliveNormal.ToString();
            Console.WriteLine($"    {t.name,-20} {survPct,9}  {winPct,13}  {aliveStr,8}  {t.avgDmg,9:F1}  {t.avgGain,8:F1}  {t.rate,9:F2}");
        }

        // Insights: label each tank with up to 3 roles — MVP, Hider, Expendable
        Console.WriteLine();
        Console.WriteLine("  Insights:");
        foreach (int sw in new[] { 1, 2 })
        {
            var swarm = tankStats.Where(t => t.swarm_id == sw).ToList();
            if (swarm.Count == 0) continue;
            var swarmWithWins = swarm.Where(t => t.swarmWins > 0).ToList();
            if (swarmWithWins.Count == 0) continue;

            int topWinSurv = swarmWithWins.Max(t => t.winSurvived);
            // Co-MVP: within 1 win-survival of the leader, but leader must have survived in ≥5 wins
            var mvps       = swarmWithWins.Where(t => topWinSurv >= 5 && topWinSurv - t.winSurvived <= 1).ToList();
            if (mvps.Count == 0) mvps = swarmWithWins.Where(t => t.winSurvived == topWinSurv).ToList();
            var hider      = swarm.OrderBy(t => t.rate).First();
            // Top attacker: highest-rate non-ECM tank per swarm (computed before Expendable to suppress conflict)
            var topAttacker = swarm.Where(t => t.role != "EcmSpecialist").OrderByDescending(t => t.rate).FirstOrDefault();
            var expendables = swarmWithWins
                .Where(t => mvps.All(m => m.name != t.name) && t.winRate < 33 && (topAttacker == default || t.name != topAttacker.name))
                .ToList();

            // Build per-tank role tags
            var roles = new Dictionary<string, List<string>>();
            foreach (var t in swarm) roles[t.name] = new List<string>();
            string mvpLabel = mvps.Count > 1 ? "Co-MVP" : "MVP";
            int swarmWinCount = mvps[0].swarmWins;
            foreach (var m in mvps)
                roles[m.name].Add($"{mvpLabel} ({m.winSurvived}/{swarmWinCount})");
            // Hider: low-rate non-ECM tank (ECM label already implies hiding for specialists)
            if (hider.role != "EcmSpecialist" && hider.rate < 3.0)
                roles[hider.name].Add($"Hider (rate {hider.rate:F2})");
            // Label all tanks genuinely under-contributing (suppressed for Top attacker — they earn their deaths)
            foreach (var e in expendables)
                roles[e.name].Add($"Expendable ({e.winSurvived}/{swarmWinCount})");
            if (topAttacker != default && topAttacker.rate > 0)
            {
                roles[topAttacker.name].Add($"Top attacker (rate {topAttacker.rate:F2})");
                // Glass cannon: top attacker whose win-survival is still below the Expendable threshold
                if (topAttacker.winRate < 33)
                    roles[topAttacker.name].Add($"Glass cannon ({topAttacker.winSurvived}/{swarmWinCount})");
            }
            // Linchpin: swarm win rate collapses when this tank is dead (requires min 5 survivals for stable estimate)
            foreach (var t in swarm.Where(t => t.survived >= 5 && t.matches - t.survived >= 5))
            {
                int winsWithout  = t.swarmWins - t.winSurvived;
                int matchesWithout = t.matches - t.survived;
                double winRateDead  = 100.0 * winsWithout / matchesWithout;
                double winRateAlive = 100.0 * t.winSurvived / t.survived;
                if (winRateDead < 25 && winRateAlive - winRateDead > 55)
                    roles[t.name].Add($"Linchpin (alive: {winRateAlive:F0}%, dead: {winRateDead:F0}%)");
            }
            // All-in: tank that never survived a loss — survival and team victory are perfectly correlated
            foreach (var t in swarm.Where(t => t.survived >= 5 && t.survived == t.winSurvived))
                roles[t.name].Add($"All-in ({t.survived} surv, 0 loss)");
            // Survivor: survives 5+ defeats — tank persists even when its swarm loses
            int swarmLosses = swarm.Count > 0 ? swarm[0].matches - swarm[0].swarmWins : 0;
            foreach (var t in swarm.Where(t => t.survived - t.winSurvived >= 5))
            {
                int lossSurv = t.survived - t.winSurvived;
                roles[t.name].Add($"Survivor ({lossSurv}/{swarmLosses} losses)");
            }
            // Fragile: extremely low overall survival rate (< 10%) with min 20 matches — dies in almost every match
            foreach (var t in swarm.Where(t => t.matches >= 20 && (double)t.survived / t.matches < 0.10))
                roles[t.name].Add($"Fragile ({100.0 * t.survived / t.matches:F0}% surv)");
            // Primary target: first killed 1.5x more than swarm average (min 5 incidents for signal stability)
            double avgFkPerTank = swarm.Count > 0 ? (double)swarm.Sum(t => t.firstDeaths) / swarm.Count : 0;
            foreach (var t in swarm.Where(t => t.matches >= 20 && t.firstDeaths >= 5
                && avgFkPerTank > 0 && t.firstDeaths > avgFkPerTank * 1.5))
                roles[t.name].Add($"Primary target ({100.0 * t.firstDeaths / t.matches:F0}% first kill)");
            // Solo carry: single-handedly wins ≥20% of swarm wins and has ≥3 solo matches (min signal)
            foreach (var t in swarm.Where(t => t.soloWins >= 3 && t.swarmWins > 0
                && (double)t.soloWins / t.swarmWins >= 0.20))
            {
                string soloNote = t.soloWinsTO == t.soloWins ? " TO-only"
                    : t.soloWinsTO > 0 ? $" ({t.soloWins - t.soloWinsTO}D+{t.soloWinsTO}TO)" : "";
                roles[t.name].Add($"Solo carry ({t.soloWins}/{t.swarmWins} solo wins{soloNote})");
            }
            // Always surface ECM role explicitly — rate can be in the 3-5 range and miss both Hider and Fighter labels
            foreach (var t in swarm.Where(t => t.role == "EcmSpecialist"))
                roles[t.name].Add("ECM");

            var labeled = roles.Where(kv => kv.Value.Count > 0).ToList();
            if (labeled.Count == 0) continue;
            string bname2 = sw == 1 ? bot1Name : bot2Name;
            Console.WriteLine($"  Swarm {sw} [{bname2}] ({swarmWinCount} wins):");
            // Balanced: warn when 3+ tanks are near the top AND the leader isn't dominant (< 50% win-surv)
            // Prevents "Co-MVP" flooding the insights section without meaningful signal
            if (mvps.Count >= 3 && swarmWinCount > 0 && (double)topWinSurv / swarmWinCount < 0.50)
                Console.WriteLine($"    Balanced — {mvps.Count} co-carriers, no dominant single survivor (leader {topWinSurv}/{swarmWinCount})");
            foreach (var kv in labeled)
                Console.WriteLine($"    {kv.Key,-22}  {string.Join(", ", kv.Value)}");
        }

        // Winner combination frequency
        Console.WriteLine();
        Console.WriteLine("  Win combos:");
        foreach (int sw in new[] { 1, 2 })
        {
            string bname = sw == 1 ? bot1Name : bot2Name;
            string swarmPrefix = sw == 1 ? bot1Name : bot2Name;
            int swarmSize = tankStats.Count(t => t.swarm_id == sw);
            var swarmAllNames = tankStats.Where(t => t.swarm_id == sw).Select(t => t.name).ToList();
            var winMatches = results.Where(r => r.winner_swarm_id == sw).ToList();
            var combos = winMatches
                .Select(r =>
                {
                    var names = sw == 1 ? r.swarm1_survivor_names : r.swarm2_survivor_names;
                    static string Short(string n, string prefix) =>
                        n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? n[prefix.Length..] : n;
                    string key = names.Count == swarmSize
                        ? "(full team)"
                        // When exactly 1 tank was lost, show the casualty — consistent with match table [-Name] notation
                        : names.Count == swarmSize - 1
                            ? $"[-{Short(swarmAllNames.FirstOrDefault(n => !names.Contains(n)) ?? "?", swarmPrefix)}]"
                            : string.Join("+", names.OrderBy(n => n).Select(n => Short(n, swarmPrefix)));
                    return (key, r.timed_out);
                })
                .GroupBy(x => x.key)
                .Select(g => (key: g.Key, count: g.Count(), decisive: g.Count(x => !x.timed_out), to: g.Count(x => x.timed_out)))
                .OrderByDescending(g => g.count)
                .ToList();
            if (combos.Count == 0) continue;
            int swWins = combos.Sum(g => g.count);
            Console.WriteLine($"  Swarm {sw} [{bname}] ({swWins} wins):");
            int unique = 0;
            foreach (var g in combos)
            {
                if (g.count > 1)
                {
                    string dtStr = g.to > 0 ? $" [{g.decisive}D+{g.to}TO]" : $" [{g.decisive}D]";
                    string toOnly = g.decisive == 0 ? " ⚑TO-only" : "";
                    Console.WriteLine($"    {g.count,3}x  {g.key,-30}  ({100.0 * g.count / swWins:F0}%){dtStr}{toOnly}");
                }
                else
                    unique++;
            }
            if (unique > 0)
                Console.WriteLine($"          ({unique} unique one-off combo{(unique == 1 ? "" : "s")}, {100.0 * unique / swWins:F0}% of wins)");

        }
    }
    else
    {
        var stalematePairs = results
            .Where(r => r.winner_swarm_id == null)
            .Select(r => string.Join(",", r.swarm1_survivor_names) + " vs " + string.Join(",", r.swarm2_survivor_names))
            .GroupBy(p => p)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());

        var tankSummary = results
            .SelectMany(r => r.tanks.Select(t => (t, matchTicks: r.ticks, winnerId: r.winner_swarm_id)))
            .GroupBy(x => (x.t.name, x.t.swarm_id))
            .Select(g =>
            {
                int survived    = g.Count(x => x.t.survived);
                int winSurvived = g.Count(x => x.t.survived && x.winnerId == x.t.swarm_id);
                int swarmWins   = g.Count(x => x.winnerId == x.t.swarm_id);
                int matches     = g.Count();
                double avgE     = survived > 0 ? Math.Round(g.Where(x => x.t.survived).Average(x => x.t.energy), 1) : 0;
                double avgDmg   = Math.Round(g.Average(x => x.t.damage_taken),  1);
                double avgGain  = Math.Round(g.Average(x => x.t.energy_gained), 1);
                double avgAlive = g.Average(x => x.t.destroyed_at_tick > 0 ? x.t.destroyed_at_tick : (double)x.matchTicks);
                double rate     = Math.Round(avgAlive > 0 ? avgGain / avgAlive * 100 : 0, 2);
                double winRate  = swarmWins > 0 ? Math.Round((double)winSurvived / swarmWins, 3) : 0;
                long avgDeath   = (long)g.Where(x => !x.t.survived && x.t.destroyed_at_tick > 0)
                                         .Select(x => (double)x.t.destroyed_at_tick)
                                         .DefaultIfEmpty(0).Average();
                return new { g.Key.name, g.Key.swarm_id, matches, survived,
                             survive_rate = Round(survived, matches),
                             win_survival_rate = winRate,
                             avg_ticks_alive = (long)avgAlive,
                             avg_energy_when_alive = avgE, avg_damage_taken = avgDmg,
                             avg_energy_gained = avgGain, combat_rate_per_100t = rate,
                             avg_death_tick = avgDeath };
            })
            .OrderBy(t => t.swarm_id).ThenBy(t => t.name)
            .ToList();

        var summary = new
        {
            type             = "summary",
            matches          = total,
            swarm1_wins      = s1Wins,
            swarm2_wins      = s2Wins,
            draws,
            swarm1_win_rate  = Round(s1Wins, total),
            swarm2_win_rate  = Round(s2Wins, total),
            draw_rate        = Round(draws, total),
            avg_ticks        = Math.Round(results.Average(r => r.ticks), 1),
            avg_first_kill_tick = avgFirstKill,
            stalemate_pairs  = stalematePairs,
            tank_stats       = tankSummary
        };
        Console.WriteLine(JsonSerializer.Serialize(summary, jsonOptions));
    }

    static string Pct(int n, int d) => $"{100.0 * n / d:F0}%";
    static double Round(int n, int d) => Math.Round((double)n / d, 3);
}

static string ShortName(string dll)
{
    var s = Path.GetFileNameWithoutExtension(dll);
    return s.Contains('.') ? s[(s.LastIndexOf('.') + 1)..] : s;
}

// ── Records ──────────────────────────────────────────────────────────────────

record MatchResult(
    [property: JsonPropertyName("winner_swarm_id")] int? winner_swarm_id,
    long ticks,
    bool timed_out,
    int swarm1_survivors,
    int swarm2_survivors,
    double swarm1_energy,
    double swarm2_energy,
    List<string> swarm1_survivor_names,
    List<string> swarm2_survivor_names,
    long? first_kill_tick,
    List<TankResult> tanks);

record TankResult(string name, int swarm_id, string role, bool survived, double energy, long destroyed_at_tick, double damage_taken, double energy_gained);

// ── Arg parser ───────────────────────────────────────────────────────────────

class Args
{
    public string? Bot1     { get; private set; }
    public string? Bot2     { get; private set; }
    public int?    Seed     { get; private set; }
    public int?    MaxTicks { get; private set; }
    public int?    Batch    { get; private set; }
    public double? Width    { get; private set; }
    public double? Height   { get; private set; }
    public bool    Summary  { get; private set; }
    public string? Format    { get; private set; }
    public string? List      { get; private set; }
    public string? OnTimeout { get; private set; }
    public int?    Parallel  { get; private set; }

    public static Args Parse(string[] argv)
    {
        var a = new Args();
        for (int i = 0; i < argv.Length; i++)
        {
            switch (argv[i])
            {
                case "--bot1"      when i + 1 < argv.Length: a.Bot1     = argv[++i]; break;
                case "--bot2"      when i + 1 < argv.Length: a.Bot2     = argv[++i]; break;
                case "--seed"      when i + 1 < argv.Length: a.Seed     = int.Parse(argv[++i]); break;
                case "--max-ticks" when i + 1 < argv.Length: a.MaxTicks = int.Parse(argv[++i]); break;
                case "--batch"     when i + 1 < argv.Length: a.Batch    = int.Parse(argv[++i]); break;
                case "--width"     when i + 1 < argv.Length: a.Width    = double.Parse(argv[++i]); break;
                case "--height"    when i + 1 < argv.Length: a.Height   = double.Parse(argv[++i]); break;
                case "--format"    when i + 1 < argv.Length: a.Format   = argv[++i]; break;
                case "--list"       when i + 1 < argv.Length: a.List      = argv[++i]; break;
                case "--on-timeout" when i + 1 < argv.Length: a.OnTimeout = argv[++i]; break;
                case "--parallel"   when i + 1 < argv.Length: a.Parallel  = int.Parse(argv[++i]); break;
                case "--summary": a.Summary = true; break;
            }
        }
        return a;
    }
}
