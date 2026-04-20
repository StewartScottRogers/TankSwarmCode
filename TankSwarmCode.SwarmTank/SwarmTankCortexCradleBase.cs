using System.Text.Json;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

public abstract class SwarmTankCortexCradleBase : SwarmTankBase
{
    protected internal abstract TankConfiguration TankConfig { get; }

    protected internal record AllyEntry(int Slot, double Energy, long LastSeen);
    protected internal record StrategyPayload(string Strategy, string TargetName, int Epoch);
    protected internal record VolleyPayload(long FireAtTick);

    protected internal static readonly JsonSerializerOptions JsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    protected internal readonly Dictionary<string, AllyEntry> AllyEntryMap = new();
    protected internal SwarmStrategy ActiveSwarmStrategy = SwarmStrategy.Wolfpack;
    protected internal string PriorityTargetName = string.Empty;
    protected internal int StrategyEpoch = -1;
    protected internal long ScheduledFireTick = -1;
    protected internal long EnemyEcmAlertTick = -999;
    protected internal long LastVolleyTick = -999;
    protected internal double RadarSpin = 45.0;

    protected internal const int LeadershipEpochTicks = 40;
    protected internal const int AllyPingInterval = 15;
    protected internal const int AllyStaleTicks = 30;
    protected internal const double OrbitRadius = 180.0;
    protected internal const double VolleyRange = 300.0;
    protected internal const long VolleyIntervalTicks = 30;
}
