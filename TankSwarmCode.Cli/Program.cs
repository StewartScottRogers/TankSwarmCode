using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using TankSwarmCode.Arena;
using TankSwarmCode.SwarmTank.Interfaces;

const int DefaultMaxTicks = 5000;
const double ArenaWidth = 800;
const double ArenaHeight = 600;

var cliArgs = Args.Parse(args);

if (cliArgs.Bot1 is null || cliArgs.Bot2 is null)
{
    Console.Error.WriteLine("Usage: TankSwarmCode.Cli run --bot1 <dll> --bot2 <dll> [--seed <int>] [--max-ticks <int>] [--batch <N>]");
    return 1;
}

int matchCount = cliArgs.Batch ?? 1;
bool isNdjson = matchCount > 1;
var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

for (int i = 1; i <= matchCount; i++)
{
    int? seed = cliArgs.Seed.HasValue ? cliArgs.Seed.Value + i - 1 : null;
    var result = RunMatch(cliArgs.Bot1, cliArgs.Bot2, seed, cliArgs.MaxTicks ?? DefaultMaxTicks);

    if (isNdjson)
    {
        var line = new { match = i, result.winner_swarm_id, result.ticks, result.timed_out, seed, result.tanks };
        Console.WriteLine(JsonSerializer.Serialize(line, jsonOptions));
    }
    else
    {
        Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
    }
}

return 0;

static MatchResult RunMatch(string bot1Dll, string bot2Dll, int? seed, int maxTicks)
{
    var engine = new ArenaEngine(ArenaWidth, ArenaHeight, seed);

    LoadTanks(bot1Dll, swarmId: 1).ForEach(engine.AddTank);
    LoadTanks(bot2Dll, swarmId: 2).ForEach(engine.AddTank);

    int? winnerId = null;
    long endTick = 0;
    engine.RoundEnded += (_, e) => { endTick = e.TotalTicks; };

    engine.Start();

    while (engine.IsRunning && engine.TickNumber < maxTicks)
        engine.Tick();

    bool timedOut = engine.IsRunning;

    // Determine winner from survivors
    var swarmsSurviving = engine.Tanks
        .Where(t => t.State.IsAlive)
        .Select(t => t.SwarmId)
        .Distinct()
        .ToList();

    winnerId = swarmsSurviving.Count == 1 ? swarmsSurviving[0] : null;

    var tanks = engine.Tanks.Select(t => new TankResult(
        t.Name, t.SwarmId, t.State.IsAlive, t.State.Energy, t.State.DestroyedAtTick)).ToList();

    return new MatchResult(winnerId, engine.TickNumber, timedOut, tanks);
}

static List<ISwarmTank> LoadTanks(string dllPath, int swarmId)
{
    var assembly = Assembly.LoadFrom(Path.GetFullPath(dllPath));
    var tanks = assembly.GetTypes()
        .Where(t => !t.IsAbstract && t.IsClass && typeof(ISwarmTank).IsAssignableFrom(t))
        .Select(t => (ISwarmTank)Activator.CreateInstance(t)!)
        .ToList();

    foreach (var tank in tanks)
    {
        var prop = tank.GetType().GetProperty("SwarmId");
        if (prop?.CanWrite == true) prop.SetValue(tank, swarmId);
    }

    return tanks;
}

record MatchResult(
    [property: JsonPropertyName("winner_swarm_id")] int? winner_swarm_id,
    long ticks,
    bool timed_out,
    List<TankResult> tanks);

record TankResult(string name, int swarm_id, bool survived, double energy, long destroyed_at_tick);

class Args
{
    public string? Bot1 { get; private set; }
    public string? Bot2 { get; private set; }
    public int? Seed { get; private set; }
    public int? MaxTicks { get; private set; }
    public int? Batch { get; private set; }

    public static Args Parse(string[] argv)
    {
        var a = new Args();
        for (int i = 0; i < argv.Length; i++)
        {
            switch (argv[i])
            {
                case "--bot1" when i + 1 < argv.Length: a.Bot1 = argv[++i]; break;
                case "--bot2" when i + 1 < argv.Length: a.Bot2 = argv[++i]; break;
                case "--seed" when i + 1 < argv.Length: a.Seed = int.Parse(argv[++i]); break;
                case "--max-ticks" when i + 1 < argv.Length: a.MaxTicks = int.Parse(argv[++i]); break;
                case "--batch" when i + 1 < argv.Length: a.Batch = int.Parse(argv[++i]); break;
            }
        }
        return a;
    }
}
