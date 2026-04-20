using System.Text.Json;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

public abstract class SwarmBrainBase : SwarmTankBase
{
    protected internal abstract TankConfig Config { get; }

    protected internal record AllyEntry(int Slot, double Energy, long LastSeen);
    protected internal record StrategyPayload(string Strategy, string TargetName, int Epoch);
    protected internal record VolleyPayload(long FireAtTick);

    protected internal static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    protected internal readonly Dictionary<string, AllyEntry> _allyMap = new();
    protected internal SwarmStrategy _activeStrategy = SwarmStrategy.Wolfpack;
    protected internal string _priorityTargetName = string.Empty;
    protected internal int _strategyEpoch = -1;
    protected internal long _scheduledFireTick = -1;
    protected internal long _enemyEcmAlertTick = -999;
    protected internal long _lastVolleyTick = -999;
    protected internal double _radarSpin = 45.0;

    protected internal const int LeadershipEpochTicks = 40;
    protected internal const int AllyPingInterval = 15;
    protected internal const int AllyStaleTicks = 30;
    protected internal const double OrbitRadius = 180.0;
    protected internal const double VolleyRange = 300.0;
    protected internal const long VolleyIntervalTicks = 30;
}
