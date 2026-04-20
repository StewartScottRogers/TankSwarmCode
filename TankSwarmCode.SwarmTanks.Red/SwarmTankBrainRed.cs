using System.Text.Json;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public abstract class SwarmTankBrainRed : SwarmTankBrainBase
{
    public override void OnTick(TickEventArgs e)
    {
        base.OnTick(e);

        AllyEntryMap[Name] = new AllyEntry(TankConfig.FormationSlot, State.Energy, Arena.TickNumber);

        this.BroadcastAllyPing();

        long staleCutoff = Arena.TickNumber - AllyStaleTicks;
        foreach (string key in AllyEntryMap.Keys.ToList())
        {
            if (AllyEntryMap[key].LastSeen < staleCutoff)
                AllyEntryMap.Remove(key);
        }

        string leaderName = this.DetermineLeader();
        if (leaderName == Name && (Arena.TickNumber % LeadershipEpochTicks == 0 || StrategyEpoch < 0))
            this.RunEpochLogic();

        this.HandleEcm();

        if (ScheduledFireTick > 0 && Arena.TickNumber >= ScheduledFireTick)
        {
            RadarContact? volleyTarget = this.GetStrategyTarget();
            if (volleyTarget != null)
            {
                double power = Math.Min(TankConfig.MaxFirePower, State.Energy * 0.1);
                if (power >= 0.1 && State.Energy >= 5 && !this.IsWallInLineOfFire(volleyTarget.Position))
                    SetFire(power);
            }
            ScheduledFireTick = -1;
        }

        this.ExecuteStrategy();
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        base.OnSwarmMessage(e);

        switch (e.Message.Type)
        {
            case SwarmMessageType.AllyPing:
                {
                    if (e.Message.SenderName == Name) break;
                    string? data = e.Message.CustomData;
                    if (data is null) break;
                    int colonIdx = data.IndexOf(':');
                    if (colonIdx < 0) break;
                    if (!int.TryParse(data[..colonIdx], out int slot)) break;
                    if (!double.TryParse(data[(colonIdx + 1)..], out double energy)) break;
                    AllyEntryMap[e.Message.SenderName] = new AllyEntry(slot, energy, Arena.TickNumber);
                    break;
                }

            case SwarmMessageType.StrategyCommand:
                {
                    if (e.Message.CustomData is null) break;
                    StrategyPayload? payload = JsonSerializer.Deserialize<StrategyPayload>(
                        e.Message.CustomData, JsonSerializerOptions);
                    if (payload is null) break;
                    if (payload.Epoch <= StrategyEpoch) break;
                    if (Enum.TryParse(payload.Strategy, ignoreCase: true, out SwarmStrategy parsed))
                        ActiveSwarmStrategy = parsed;
                    PriorityTargetName = payload.TargetName ?? string.Empty;
                    StrategyEpoch = payload.Epoch;
                    break;
                }

            case SwarmMessageType.VolleyFire:
                {
                    if (e.Message.CustomData is null) break;
                    VolleyPayload? payload = JsonSerializer.Deserialize<VolleyPayload>(
                        e.Message.CustomData, JsonSerializerOptions);
                    if (payload is null) break;

                    string targetName = e.Message.TargetName ?? string.Empty;
                    if (!RadarMap.TryGetValue(targetName, out RadarContact? target)) break;

                    double power = Math.Min(TankConfig.MaxFirePower, State.Energy * 0.1);
                    double bulletSpeed = 20.0 - 3.0 * Math.Max(power, 0.1);
                    double dist = State.Position.DistanceTo(target.Position);
                    long travelTime = (long)Math.Ceiling(dist / bulletSpeed);
                    ScheduledFireTick = payload.FireAtTick - travelTime;
                    break;
                }

            case SwarmMessageType.EcmAlert:
                EnemyEcmAlertTick = Arena.TickNumber;
                break;
        }
    }
}
