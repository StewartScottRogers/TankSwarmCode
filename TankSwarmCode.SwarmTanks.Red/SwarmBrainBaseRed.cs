using System.Text.Json;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public abstract class SwarmBrainBaseRed : SwarmBrainBase
{
    public override void OnTick(TickEventArgs e)
    {
        base.OnTick(e);

        _allyMap[Name] = new AllyEntry(Config.FormationSlot, State.Energy, Arena.TickNumber);

        this.BroadcastAllyPing();

        long staleCutoff = Arena.TickNumber - AllyStaleTicks;
        foreach (string key in _allyMap.Keys.ToList())
        {
            if (_allyMap[key].LastSeen < staleCutoff)
                _allyMap.Remove(key);
        }

        string leaderName = this.DetermineLeader();
        if (leaderName == Name && (Arena.TickNumber % LeadershipEpochTicks == 0 || _strategyEpoch < 0))
            this.RunEpochLogic();

        this.HandleEcm();

        if (_scheduledFireTick > 0 && Arena.TickNumber >= _scheduledFireTick)
        {
            RadarContact? volleyTarget = this.GetStrategyTarget();
            if (volleyTarget != null)
            {
                double power = Math.Min(Config.MaxFirePower, State.Energy * 0.1);
                if (power >= 0.1 && State.Energy >= 5 && !this.IsWallInLineOfFire(volleyTarget.Position))
                    SetFire(power);
            }
            _scheduledFireTick = -1;
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
                    _allyMap[e.Message.SenderName] = new AllyEntry(slot, energy, Arena.TickNumber);
                    break;
                }

            case SwarmMessageType.StrategyCommand:
                {
                    if (e.Message.CustomData is null) break;
                    StrategyPayload? payload = JsonSerializer.Deserialize<StrategyPayload>(
                        e.Message.CustomData, JsonOptions);
                    if (payload is null) break;
                    if (payload.Epoch <= _strategyEpoch) break;
                    if (Enum.TryParse(payload.Strategy, ignoreCase: true, out SwarmStrategy parsed))
                        _activeStrategy = parsed;
                    _priorityTargetName = payload.TargetName ?? string.Empty;
                    _strategyEpoch = payload.Epoch;
                    break;
                }

            case SwarmMessageType.VolleyFire:
                {
                    if (e.Message.CustomData is null) break;
                    VolleyPayload? payload = JsonSerializer.Deserialize<VolleyPayload>(
                        e.Message.CustomData, JsonOptions);
                    if (payload is null) break;

                    string targetName = e.Message.TargetName ?? string.Empty;
                    if (!RadarMap.TryGetValue(targetName, out RadarContact? target)) break;

                    double power = Math.Min(Config.MaxFirePower, State.Energy * 0.1);
                    double bulletSpeed = 20.0 - 3.0 * Math.Max(power, 0.1);
                    double dist = State.Position.DistanceTo(target.Position);
                    long travelTime = (long)Math.Ceiling(dist / bulletSpeed);
                    _scheduledFireTick = payload.FireAtTick - travelTime;
                    break;
                }

            case SwarmMessageType.EcmAlert:
                _enemyEcmAlertTick = Arena.TickNumber;
                break;
        }
    }
}
