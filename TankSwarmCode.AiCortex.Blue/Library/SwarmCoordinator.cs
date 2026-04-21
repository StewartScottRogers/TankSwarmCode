using System.Collections.Concurrent;
using System.Text.Json;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Events;
using TankSwarmCode.SwarmTank.Extensions;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.Library;

public sealed class SwarmCoordinator
{
    private static readonly ConcurrentDictionary<int, SwarmCoordinator> Registry = new();

    public static SwarmCoordinator ForTeam(int swarmId)
        => Registry.GetOrAdd(swarmId, _ => new SwarmCoordinator());

    internal readonly Dictionary<string, AllyEntry> AllyEntryMap = new();
    internal SwarmStrategy ActiveSwarmStrategy = SwarmStrategy.Wolfpack;
    internal string PriorityTargetName = string.Empty;
    internal int StrategyEpoch = -1;
    internal long EnemyEcmAlertTick = -999;
    internal long LastVolleyTick = -999;

    internal record AllyEntry(int Slot, double Energy, long LastSeen);
    private record StrategyPayload(string Strategy, string TargetName, int Epoch);
    private record VolleyPayload(long FireAtTick);

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    internal const int LeadershipEpochTicks = 40;
    internal const int AllyPingInterval = 15;
    internal const int AllyStaleTicks = 30;
    internal const double OrbitRadius = 180.0;
    internal const double VolleyRange = 300.0;
    internal const long VolleyIntervalTicks = 30;

    public void Reset()
    {
        AllyEntryMap.Clear();
        ActiveSwarmStrategy = SwarmStrategy.Wolfpack;
        PriorityTargetName = string.Empty;
        StrategyEpoch = -1;
        EnemyEcmAlertTick = -999;
        LastVolleyTick = -999;
    }

    internal void UpdateSelf(ITankContext ctx, TankConfiguration config)
        => AllyEntryMap[ctx.Name] = new AllyEntry(config.FormationSlot, ctx.State.Energy, ctx.Arena.TickNumber);

    internal void PruneStaleAllies(ITankContext ctx)
    {
        long staleCutoff = ctx.Arena.TickNumber - AllyStaleTicks;
        foreach (string key in AllyEntryMap.Keys.ToList())
            if (AllyEntryMap[key].LastSeen < staleCutoff)
                AllyEntryMap.Remove(key);
    }

    internal void BroadcastAllyPing(ITankContext ctx, TankConfiguration config)
    {
        if (ctx.Arena.TickNumber % AllyPingInterval != 0) return;
        ctx.Broadcast(new SwarmMessage
        {
            Type = SwarmMessageType.AllyPing,
            SenderName = ctx.Name,
            CustomData = $"{config.FormationSlot}:{ctx.State.Energy:F1}",
            Timestamp = ctx.Arena.TickNumber
        });
    }

    internal string DetermineLeader()
    {
        AllyEntry? best = null;
        string bestName = string.Empty;
        foreach (KeyValuePair<string, AllyEntry> kv in AllyEntryMap)
        {
            if (best is null
                || kv.Value.Slot < best.Slot
                || (kv.Value.Slot == best.Slot && string.Compare(kv.Key, bestName, StringComparison.Ordinal) < 0))
            {
                best = kv.Value;
                bestName = kv.Key;
            }
        }
        return bestName;
    }

    internal int GetAliveAllyCount() => AllyEntryMap.Count - 1;

    internal void HandleAllyPing(ITankContext ctx, SwarmMessageEventArgs e)
    {
        if (e.Message.SenderName == ctx.Name) return;
        string? data = e.Message.CustomData;
        if (data is null) return;
        int colonIdx = data.IndexOf(':');
        if (colonIdx < 0) return;
        if (!int.TryParse(data[..colonIdx], out int slot)) return;
        if (!double.TryParse(data[(colonIdx + 1)..], out double energy)) return;
        AllyEntryMap[e.Message.SenderName] = new AllyEntry(slot, energy, ctx.Arena.TickNumber);
    }

    internal void HandleStrategyCommand(SwarmMessageEventArgs e)
    {
        if (e.Message.CustomData is null) return;
        StrategyPayload? payload = JsonSerializer.Deserialize<StrategyPayload>(e.Message.CustomData, JsonOptions);
        if (payload is null || payload.Epoch <= StrategyEpoch) return;
        if (Enum.TryParse(payload.Strategy, ignoreCase: true, out SwarmStrategy parsed))
            ActiveSwarmStrategy = parsed;
        PriorityTargetName = payload.TargetName ?? string.Empty;
        StrategyEpoch = payload.Epoch;
    }

    internal long? HandleVolleyFire(ITankContext ctx, TankConfiguration config, SwarmMessageEventArgs e)
    {
        if (e.Message.CustomData is null) return null;
        VolleyPayload? payload = JsonSerializer.Deserialize<VolleyPayload>(e.Message.CustomData, JsonOptions);
        if (payload is null) return null;
        string targetName = e.Message.TargetName ?? string.Empty;
        if (!ctx.RadarMap.TryGetValue(targetName, out RadarContact? target)) return null;
        double power = Math.Min(config.MaxFirePower, ctx.State.Energy * 0.1);
        double bulletSpeed = 20.0 - 3.0 * Math.Max(power, 0.1);
        double dist = ctx.State.Position.DistanceTo(target.Position);
        return payload.FireAtTick - (long)Math.Ceiling(dist / bulletSpeed);
    }

    internal void HandleEcmAlert(ITankContext ctx)
        => EnemyEcmAlertTick = ctx.Arena.TickNumber;

    internal bool IsEnemyEcmActive(ITankContext ctx)
        => ctx.Arena.TickNumber - EnemyEcmAlertTick < 20;

    internal void HandleEcm(ITankContext ctx, TankConfiguration config)
    {
        EcmMode mode = EcmMode.Off;
        if (config.HasEcm && ActiveSwarmStrategy == SwarmStrategy.ECMScreen)
            mode = config.OffensiveEcmMode;
        else if (IsEnemyEcmActive(ctx))
            mode = EcmMode.Burnthrough;
        ctx.SetEcm(mode);
    }

    internal long? RunEpochLogic(ITankContext ctx, TankConfiguration config)
    {
        ActiveSwarmStrategy = SelectStrategy(ctx, config);
        StrategyEpoch++;

        long freshCutoff = ctx.Arena.TickNumber - AllyStaleTicks;
        RadarContact? priorityTarget = ctx.RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .OrderBy(c => c.Energy)
            .FirstOrDefault();
        PriorityTargetName = priorityTarget?.Name ?? string.Empty;

        ctx.Broadcast(new SwarmMessage
        {
            SenderName = ctx.Name,
            Type = SwarmMessageType.StrategyCommand,
            TargetName = PriorityTargetName,
            CustomData = JsonSerializer.Serialize(
                new StrategyPayload(ActiveSwarmStrategy.ToString(), PriorityTargetName, StrategyEpoch), JsonOptions),
            Timestamp = ctx.Arena.TickNumber
        });

        if ((ActiveSwarmStrategy == SwarmStrategy.Wolfpack || ActiveSwarmStrategy == SwarmStrategy.Pincer)
            && GetAliveAllyCount() >= 2
            && priorityTarget != null
            && ctx.State.Position.DistanceTo(priorityTarget.Position) <= VolleyRange
            && ctx.Arena.TickNumber - LastVolleyTick >= VolleyIntervalTicks)
        {
            long fireAt = ctx.Arena.TickNumber + 20;
            LastVolleyTick = ctx.Arena.TickNumber;

            ctx.Broadcast(new SwarmMessage
            {
                SenderName = ctx.Name,
                Type = SwarmMessageType.VolleyFire,
                TargetName = priorityTarget.Name,
                CustomData = JsonSerializer.Serialize(new VolleyPayload(fireAt), JsonOptions),
                Timestamp = ctx.Arena.TickNumber
            });

            return fireAt;
        }

        return null;
    }

    private SwarmStrategy SelectStrategy(ITankContext ctx, TankConfiguration config)
    {
        long freshCutoff = ctx.Arena.TickNumber - AllyStaleTicks;

        List<RadarContact> enemies = ctx.RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .ToList();

        double sumEnergy = ctx.State.Energy;
        int allyCount = 1;
        foreach (KeyValuePair<string, AllyEntry> kv in AllyEntryMap)
        {
            if (kv.Key == ctx.Name || kv.Value.LastSeen < freshCutoff) continue;
            sumEnergy += kv.Value.Energy;
            allyCount++;
        }

        if (ctx.State.Energy < config.RetreatEnergyThreshold && allyCount == 1)
            return SwarmStrategy.Scatter;

        if (sumEnergy / allyCount < 20.0)
            return SwarmStrategy.Fallback;

        if (enemies.Count == 0)
            return SwarmStrategy.Wolfpack;

        if (IsEnemyEcmActive(ctx))
            return SwarmStrategy.ECMScreen;

        if (allyCount >= enemies.Count * 2 && allyCount >= 3)
            return SwarmStrategy.Encircle;

        if (allyCount >= 3 && enemies.Count <= 2)
            return SwarmStrategy.Pincer;

        return SwarmStrategy.Wolfpack;
    }

    internal RadarContact? GetStrategyTarget(ITankContext ctx)
    {
        if (!string.IsNullOrEmpty(PriorityTargetName)
            && ctx.RadarMap.TryGetValue(PriorityTargetName, out RadarContact? named)
            && ctx.Arena.TickNumber - named.Timestamp < 30)
            return named;

        return ctx.RadarMap.Values
            .Where(c => !c.IsAlly && ctx.Arena.TickNumber - c.Timestamp < 30)
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefault();
    }

    internal void ExecuteStrategy(ITankContext ctx, TankConfiguration config, double radarSpin)
    {
        RadarContact? target = GetStrategyTarget(ctx);

        switch (ActiveSwarmStrategy)
        {
            case SwarmStrategy.Wolfpack:
                if (target != null) ExecuteWolfpack(ctx, config, target, radarSpin); else Scout(ctx);
                break;
            case SwarmStrategy.Encircle:
                if (target != null) ExecuteEncircle(ctx, config, target, radarSpin); else Scout(ctx);
                break;
            case SwarmStrategy.Pincer:
                if (target != null) ExecutePincer(ctx, config, target, radarSpin); else Scout(ctx);
                break;
            case SwarmStrategy.ECMScreen:
                if (target != null) ExecuteECMScreen(ctx, config, target, radarSpin); else Scout(ctx);
                break;
            case SwarmStrategy.Fallback:
                ExecuteFallback(ctx, radarSpin);
                break;
            case SwarmStrategy.Scatter:
                ExecuteScatter(ctx, radarSpin);
                break;
        }
    }

    private static void ExecuteWolfpack(ITankContext ctx, TankConfiguration config, RadarContact target, double radarSpin)
    {
        double age = ctx.Arena.TickNumber - target.Timestamp;
        Vector2D predictedPos = new(
            target.Position.X + target.VelocityVector.X * age,
            target.Position.Y + target.VelocityVector.Y * age);
        double approachAngle = config.FormationSlot == 6 ? 330.0
            : config.FormationSlot == 7 ? 270.0
            : config.FormationSlot == 8 ? 30.0
            : config.FormationSlot == 9 ? 90.0
            : config.FormationSlot * 60.0;
        Vector2D approachPoint = predictedPos.PolarOffset(approachAngle, config.PreferredRange);
        TankNavigation.NavigateTo(ctx, approachPoint, 0);
        TankNavigation.MaintainRadar(ctx, target.Position, radarSpin);
        TankNavigation.LinearPredictionFire(ctx, target, config.MaxFirePower);
    }

    private void ExecuteEncircle(ITankContext ctx, TankConfiguration config, RadarContact target, double radarSpin)
    {
        int aliveCount = GetAliveAllyCount() + 1;
        int mySlot = config.FormationSlot % aliveCount;
        double orbitAngleDeg = mySlot * (360.0 / aliveCount);
        Vector2D orbitPoint = target.Position.PolarOffset(orbitAngleDeg, OrbitRadius);

        TankNavigation.NavigateTo(ctx, orbitPoint, 0);
        TankNavigation.MaintainRadar(ctx, target.Position, radarSpin);

        if (ctx.State.Position.DistanceTo(target.Position) < 220)
            TankNavigation.LinearPredictionFire(ctx, target, config.MaxFirePower);
    }

    private static void ExecutePincer(ITankContext ctx, TankConfiguration config, RadarContact target, double radarSpin)
    {
        double groupAngle = config.FormationSlot <= 1 ? 0.0 : 180.0;
        Vector2D approachPoint = target.Position.PolarOffset(groupAngle, 200.0);
        TankNavigation.NavigateTo(ctx, approachPoint, 50.0);
        TankNavigation.MaintainRadar(ctx, target.Position, radarSpin);
        TankNavigation.LinearPredictionFire(ctx, target, config.MaxFirePower);
    }

    private static void ExecuteECMScreen(ITankContext ctx, TankConfiguration config, RadarContact target, double radarSpin)
    {
        double standoff = config.HasEcm ? 120.0 : 130.0;
        TankNavigation.NavigateTo(ctx, target.Position, standoff);
        if (!config.HasEcm)
            TankNavigation.LinearPredictionFire(ctx, target, config.MaxFirePower);
        TankNavigation.MaintainRadar(ctx, target.Position, radarSpin);
    }

    private static void ExecuteFallback(ITankContext ctx, double radarSpin)
    {
        TankNavigation.NavigateTo(ctx, TankNavigation.FurthestArenaCorner(ctx, AllyStaleTicks), 0);
        TankNavigation.MaintainRadar(ctx, null, radarSpin);
    }

    private static void ExecuteScatter(ITankContext ctx, double radarSpin)
    {
        RadarContact? enemy = ctx.RadarMap.Values
            .Where(c => !c.IsAlly && ctx.Arena.TickNumber - c.Timestamp < 60)
            .OrderByDescending(c => c.Timestamp)
            .FirstOrDefault();

        if (enemy != null)
        {
            double fleeHeading = (enemy.Position.BearingTo(ctx.State.Position) + 360) % 360;
            double bodyTurn = (fleeHeading - ctx.State.Heading).RelativeBearing();
            ctx.SetTurnRight(Math.Clamp(bodyTurn, -ArenaConstants.MaxTurnRate, ArenaConstants.MaxTurnRate));
        }
        ctx.SetAhead(200);
        ctx.SetTurnRadarRight(radarSpin);
    }

    private static void Scout(ITankContext ctx)
    {
        ctx.SetTurnRadarRight(45);
        ctx.SetAhead(100);
    }
}
