using System.Text.Json;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

internal static class SwarmBrainBaseLeadershipExtensions
{
    internal static string DetermineLeader(this SwarmBrainBase brain)
    {
        SwarmBrainBase.AllyEntry? best = null;
        string bestName = string.Empty;

        foreach (KeyValuePair<string, SwarmBrainBase.AllyEntry> kv in brain.AllyEntryMap)
        {
            if (kv.Value.LastSeen < brain.Arena.TickNumber - SwarmBrainBase.AllyStaleTicks)
                continue;

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

    internal static void RunEpochLogic(this SwarmBrainBase brain)
    {
        SwarmStrategy strategy = brain.SelectStrategy();
        brain.ActiveSwarmStrategy = strategy;
        brain.StrategyEpoch++;

        RadarContact? priorityTarget = null;
        long freshCutoff = brain.Arena.TickNumber - SwarmBrainBase.AllyStaleTicks;
        foreach (RadarContact c in brain.RadarMap.Values)
        {
            if (c.IsAlly || c.Timestamp < freshCutoff)
                continue;
            if (priorityTarget is null || c.Energy < priorityTarget.Energy)
                priorityTarget = c;
        }

        brain.PriorityTargetName = priorityTarget?.Name ?? string.Empty;

        string strategyJson = JsonSerializer.Serialize(
            new SwarmBrainBase.StrategyPayload(strategy.ToString(), brain.PriorityTargetName, brain.StrategyEpoch),
            SwarmBrainBase.JsonSerializerOptions);

        brain.Broadcast(new SwarmMessage
        {
            SenderName = brain.Name,
            Type = SwarmMessageType.StrategyCommand,
            TargetName = brain.PriorityTargetName,
            CustomData = strategyJson,
            Timestamp = brain.Arena.TickNumber
        });

        if ((strategy == SwarmStrategy.Wolfpack || strategy == SwarmStrategy.Pincer)
            && brain.GetAliveAllyCount() >= 2
            && priorityTarget != null
            && brain.State.Position.DistanceTo(priorityTarget.Position) <= SwarmBrainBase.VolleyRange
            && brain.Arena.TickNumber - brain.LastVolleyTick >= SwarmBrainBase.VolleyIntervalTicks)
        {
            long fireAtTick = brain.Arena.TickNumber + 20;
            brain.LastVolleyTick = brain.Arena.TickNumber;

            string volleyJson = JsonSerializer.Serialize(
                new SwarmBrainBase.VolleyPayload(fireAtTick), SwarmBrainBase.JsonSerializerOptions);

            brain.Broadcast(new SwarmMessage
            {
                SenderName = brain.Name,
                Type = SwarmMessageType.VolleyFire,
                TargetName = priorityTarget.Name,
                CustomData = volleyJson,
                Timestamp = brain.Arena.TickNumber
            });

            brain.ScheduledFireTick = fireAtTick;
        }
    }

    internal static SwarmStrategy SelectStrategy(this SwarmBrainBase brain)
    {
        long freshCutoff = brain.Arena.TickNumber - SwarmBrainBase.AllyStaleTicks;

        List<RadarContact> enemies = brain.RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .ToList();

        int allyCount = brain.GetAliveAllyCount() + 1; // +1 = self

        double totalEnergy = brain.State.Energy;
        foreach (SwarmBrainBase.AllyEntry entry in brain.AllyEntryMap.Values)
        {
            if (entry.LastSeen >= freshCutoff && brain.AllyEntryMap.ContainsKey(brain.Name) && !ReferenceEquals(entry, brain.AllyEntryMap[brain.Name]))
                totalEnergy += entry.Energy;
        }
        // Recompute properly: sum all alive ally energies + self
        double sumEnergy = brain.State.Energy;
        int countForAvg = 1;
        foreach (KeyValuePair<string, SwarmBrainBase.AllyEntry> kv in brain.AllyEntryMap)
        {
            if (kv.Key == brain.Name) continue;
            if (kv.Value.LastSeen >= freshCutoff)
            {
                sumEnergy += kv.Value.Energy;
                countForAvg++;
            }
        }
        double avgAllyEnergy = sumEnergy / countForAvg;

        if (brain.State.Energy < brain.TankConfig.RetreatEnergyThreshold && allyCount == 1)
            return SwarmStrategy.Scatter;

        if (avgAllyEnergy < 30.0)
            return SwarmStrategy.Fallback;

        if (enemies.Count == 0)
            return SwarmStrategy.Wolfpack;

        if (brain.IsEnemyEcmActive())
            return SwarmStrategy.ECMScreen;

        if (allyCount >= enemies.Count * 2 && allyCount >= 3)
            return SwarmStrategy.Encircle;

        if (allyCount >= 3 && enemies.Count <= 2)
            return SwarmStrategy.Pincer;

        return SwarmStrategy.Wolfpack;
    }

    internal static void BroadcastAllyPing(this SwarmBrainBase brain)
    {
        if (brain.Arena.TickNumber % SwarmBrainBase.AllyPingInterval != 0)
            return;

        brain.Broadcast(new SwarmMessage
        {
            Type = SwarmMessageType.AllyPing,
            SenderName = brain.Name,
            CustomData = $"{brain.TankConfig.FormationSlot}:{brain.State.Energy:F1}",
            Timestamp = brain.Arena.TickNumber
        });
    }

    internal static int GetAliveAllyCount(this SwarmBrainBase brain)
    {
        long freshCutoff = brain.Arena.TickNumber - SwarmBrainBase.AllyStaleTicks;
        return brain.AllyEntryMap.Values.Count(e => e.LastSeen >= freshCutoff) - 1; // subtract self
    }
}
