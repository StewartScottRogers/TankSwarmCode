using System.Text.Json;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

public abstract class SwarmBrainBase : SwarmTankBase
{
    protected abstract TankConfig Config { get; }

    private record AllyEntry(int Slot, double Energy, long LastSeen);
    private record StrategyPayload(string Strategy, string TargetName, int Epoch);
    private record VolleyPayload(long FireAtTick);

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    private readonly Dictionary<string, AllyEntry> _allyMap = new();
    private SwarmStrategy _activeStrategy = SwarmStrategy.Wolfpack;
    private string _priorityTargetName = string.Empty;
    private int _strategyEpoch = -1;
    private long _scheduledFireTick = -1;
    private long _enemyEcmAlertTick = -999;
    private long _lastVolleyTick = -999;
    private double _radarSpin = 45.0;

    private const int LeadershipEpochTicks = 40;
    private const int AllyPingInterval = 15;
    private const int AllyStaleTicks = 30;
    private const double OrbitRadius = 180.0;
    private const double VolleyRange = 300.0;
    private const long VolleyIntervalTicks = 30;

    public override void OnTick(TickEventArgs e)
    {
        base.OnTick(e);

        // 1. Add self entry so DetermineLeader() can include us
        _allyMap[Name] = new AllyEntry(Config.FormationSlot, State.Energy, Arena.TickNumber);

        // 2. Broadcast ally ping
        BroadcastAllyPing();

        // 3. Cull stale ally entries
        long staleCutoff = Arena.TickNumber - AllyStaleTicks;
        foreach (string key in _allyMap.Keys.ToList())
        {
            if (_allyMap[key].LastSeen < staleCutoff)
                _allyMap.Remove(key);
        }

        // 4. Leader epoch logic
        string leaderName = DetermineLeader();
        if (leaderName == Name && (Arena.TickNumber % LeadershipEpochTicks == 0 || _strategyEpoch < 0))
        {
            RunEpochLogic();
        }

        // 5. Handle ECM
        HandleEcm();

        // 6. Scheduled volley fire
        if (_scheduledFireTick > 0 && Arena.TickNumber >= _scheduledFireTick)
        {
            RadarContact? volleyTarget = GetStrategyTarget();
            if (volleyTarget != null)
            {
                double power = Math.Min(Config.MaxFirePower, State.Energy * 0.1);
                if (power >= 0.1 && State.Energy >= 5)
                    SetFire(power);
            }
            _scheduledFireTick = -1;
        }

        // 7. Execute strategy
        ExecuteStrategy();
    }

    private string DetermineLeader()
    {
        // Leader = alive entry with lowest Slot; tie-break by name alphabetically
        AllyEntry? best = null;
        string bestName = string.Empty;

        foreach (KeyValuePair<string, AllyEntry> kv in _allyMap)
        {
            if (kv.Value.LastSeen < Arena.TickNumber - AllyStaleTicks)
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

    private void RunEpochLogic()
    {
        SwarmStrategy strategy = SelectStrategy();
        _activeStrategy = strategy;
        _strategyEpoch++;

        // Priority target = lowest energy enemy from RadarMap
        RadarContact? priorityTarget = null;
        long freshCutoff = Arena.TickNumber - AllyStaleTicks;
        foreach (RadarContact c in RadarMap.Values)
        {
            if (c.IsAlly || c.Timestamp < freshCutoff)
                continue;
            if (priorityTarget is null || c.Energy < priorityTarget.Energy)
                priorityTarget = c;
        }

        _priorityTargetName = priorityTarget?.Name ?? string.Empty;

        string strategyJson = JsonSerializer.Serialize(
            new StrategyPayload(strategy.ToString(), _priorityTargetName, _strategyEpoch),
            JsonOptions);

        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type = SwarmMessageType.StrategyCommand,
            TargetName = _priorityTargetName,
            CustomData = strategyJson,
            Timestamp = Arena.TickNumber
        });

        // Schedule volley fire if appropriate
        if ((strategy == SwarmStrategy.Wolfpack || strategy == SwarmStrategy.Pincer)
            && GetAliveAllyCount() >= 2
            && priorityTarget != null
            && State.Position.DistanceTo(priorityTarget.Position) <= VolleyRange
            && Arena.TickNumber - _lastVolleyTick >= VolleyIntervalTicks)
        {
            long fireAtTick = Arena.TickNumber + 20;
            _lastVolleyTick = Arena.TickNumber;

            string volleyJson = JsonSerializer.Serialize(new VolleyPayload(fireAtTick), JsonOptions);

            Broadcast(new SwarmMessage
            {
                SenderName = Name,
                Type = SwarmMessageType.VolleyFire,
                TargetName = priorityTarget.Name,
                CustomData = volleyJson,
                Timestamp = Arena.TickNumber
            });

            // Leader also schedules its own fire
            _scheduledFireTick = fireAtTick;
        }
    }

    private SwarmStrategy SelectStrategy()
    {
        long freshCutoff = Arena.TickNumber - AllyStaleTicks;

        List<RadarContact> enemies = RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .ToList();

        int allyCount = GetAliveAllyCount() + 1; // +1 = self

        double totalEnergy = State.Energy;
        foreach (AllyEntry entry in _allyMap.Values)
        {
            if (entry.LastSeen >= freshCutoff && _allyMap.ContainsKey(Name) && !ReferenceEquals(entry, _allyMap[Name]))
                totalEnergy += entry.Energy;
        }
        // Recompute properly: sum all alive ally energies + self
        double sumEnergy = State.Energy;
        int countForAvg = 1;
        foreach (KeyValuePair<string, AllyEntry> kv in _allyMap)
        {
            if (kv.Key == Name) continue;
            if (kv.Value.LastSeen >= freshCutoff)
            {
                sumEnergy += kv.Value.Energy;
                countForAvg++;
            }
        }
        double avgAllyEnergy = sumEnergy / countForAvg;

        if (State.Energy < Config.RetreatEnergyThreshold && allyCount == 1)
            return SwarmStrategy.Scatter;

        if (avgAllyEnergy < 30.0)
            return SwarmStrategy.Fallback;

        if (enemies.Count == 0)
            return SwarmStrategy.Wolfpack;

        if (IsEnemyEcmActive())
            return SwarmStrategy.ECMScreen;

        if (allyCount >= enemies.Count * 2 && allyCount >= 3)
            return SwarmStrategy.Encircle;

        if (allyCount >= 3 && enemies.Count <= 2)
            return SwarmStrategy.Pincer;

        return SwarmStrategy.Wolfpack;
    }

    private bool IsEnemyEcmActive() => Arena.TickNumber - _enemyEcmAlertTick < 20;

    private void HandleEcm()
    {
        if (Config.HasEcm)
        {
            if (_activeStrategy == SwarmStrategy.ECMScreen)
                SetEcm(Config.OffensiveEcmMode);
            else if (IsEnemyEcmActive())
                SetEcm(EcmMode.Burnthrough);
            else
                SetEcm(EcmMode.Off);
        }
        else
        {
            if (IsEnemyEcmActive())
                SetEcm(EcmMode.Burnthrough);
            else
                SetEcm(EcmMode.Off);
        }
    }

    private void ExecuteStrategy()
    {
        RadarContact? target = GetStrategyTarget();

        switch (_activeStrategy)
        {
            case SwarmStrategy.Wolfpack:
                if (target != null) ExecuteWolfpack(target); else Scout();
                break;
            case SwarmStrategy.Encircle:
                if (target != null) ExecuteEncircle(target); else Scout();
                break;
            case SwarmStrategy.Pincer:
                if (target != null) ExecutePincer(target); else Scout();
                break;
            case SwarmStrategy.ECMScreen:
                if (target != null) ExecuteECMScreen(target); else Scout();
                break;
            case SwarmStrategy.Fallback:
                ExecuteFallback();
                break;
            case SwarmStrategy.Scatter:
                ExecuteScatter();
                break;
        }
    }

    private RadarContact? GetStrategyTarget()
    {
        if (!string.IsNullOrEmpty(_priorityTargetName)
            && RadarMap.TryGetValue(_priorityTargetName, out RadarContact? named)
            && Arena.TickNumber - named.Timestamp < 30)
        {
            return named;
        }
        return GetFreshestEnemy(30);
    }

    private void ExecuteWolfpack(RadarContact target)
    {
        NavigateTo(target.Position, Config.PreferredRange);
        MaintainRadar(target.Position);
        LinearPredictionFire(target);
    }

    private void ExecuteEncircle(RadarContact target)
    {
        int aliveCount = GetAliveAllyCount() + 1;
        int mySlot = Config.FormationSlot % aliveCount;
        double orbitAngleDeg = mySlot * (360.0 / aliveCount);
        Vector2D orbitPoint = PolarOffset(target.Position, orbitAngleDeg, OrbitRadius);

        NavigateTo(orbitPoint, 0);
        MaintainRadar(target.Position);

        double dist = State.Position.DistanceTo(target.Position);
        if (dist < 220)
            LinearPredictionFire(target);
    }

    private void ExecutePincer(RadarContact target)
    {
        double groupAngle = Config.FormationSlot <= 1 ? 0.0 : 180.0;
        Vector2D approachPoint = PolarOffset(target.Position, groupAngle, 200.0);
        NavigateTo(approachPoint, 50.0);
        MaintainRadar(target.Position);
        LinearPredictionFire(target);
    }

    private void ExecuteECMScreen(RadarContact target)
    {
        if (Config.HasEcm)
        {
            // ECM already set by HandleEcm(); keep safe distance
            NavigateTo(target.Position, 120.0);
        }
        else
        {
            NavigateTo(target.Position, 130.0);
            LinearPredictionFire(target);
        }
        MaintainRadar(target.Position);
    }

    private void ExecuteFallback()
    {
        Vector2D rallyCorner = FurthestArenaCorner();
        NavigateTo(rallyCorner, 0);
        MaintainRadar(null);
        // Do NOT fire — preserve energy
    }

    private void ExecuteScatter()
    {
        RadarContact? enemy = GetFreshestEnemy(60);
        if (enemy != null)
        {
            double fleeHeading = (enemy.Position.BearingTo(State.Position) + 360) % 360;
            double bodyTurn = RelativeBearing(fleeHeading - State.Heading);
            SetTurnRight(Math.Clamp(bodyTurn, -ArenaConstants.MaxTurnRate, ArenaConstants.MaxTurnRate));
            SetAhead(200);
        }
        else
        {
            SetAhead(200);
        }
        SetTurnRadarRight(_radarSpin);
    }

    private void Scout()
    {
        SetTurnRadarRight(45);
        SetAhead(100);
    }

    private void NavigateTo(Vector2D dest, double stopDistance)
    {
        double distToDest = State.Position.DistanceTo(dest);
        if (distToDest <= stopDistance)
            return;

        double bearing = State.Position.BearingTo(dest);
        double bodyTurn = RelativeBearing(bearing - State.Heading);
        bodyTurn = Math.Clamp(bodyTurn, -ArenaConstants.MaxTurnRate, ArenaConstants.MaxTurnRate);
        SetTurnRight(bodyTurn);

        if (Math.Abs(bodyTurn) < 30)
            SetAhead(Math.Min(distToDest - stopDistance, 100));
        else
            SetAhead(40);
    }

    private void MaintainRadar(Vector2D? focusPoint)
    {
        if (focusPoint.HasValue)
        {
            double radarBearing = State.Position.BearingTo(focusPoint.Value);
            if (!double.IsFinite(radarBearing)) { SetTurnRadarRight(_radarSpin); return; }
            double radarDiff = RelativeBearing(radarBearing - State.RadarHeading);
            SetTurnRadarRight(Math.Clamp(radarDiff * 1.5, -45, 45));
        }
        else
        {
            SetTurnRadarRight(_radarSpin);
        }
    }

    private void LinearPredictionFire(RadarContact target)
    {
        double power = Math.Min(Config.MaxFirePower, State.Energy * 0.1);
        if (power < 0.1 || State.Energy < 5)
            return;

        double bulletSpeed = 20.0 - 3.0 * power;
        double dist = State.Position.DistanceTo(target.Position);
        double travelTime = dist / bulletSpeed;

        double predictedX = target.Position.X + target.VelocityVector.X * travelTime;
        double predictedY = target.Position.Y + target.VelocityVector.Y * travelTime;
        Vector2D predictedPos = new(predictedX, predictedY);

        double desiredGunBearing = State.Position.BearingTo(predictedPos);
        double gunDiff = RelativeBearing(desiredGunBearing - State.GunHeading);
        SetTurnGunRight(Math.Clamp(gunDiff, -ArenaConstants.MaxGunTurnRate, ArenaConstants.MaxGunTurnRate));

        if (Math.Abs(gunDiff) < 5.0)
            SetFire(power);
    }

    private void BroadcastAllyPing()
    {
        if (Arena.TickNumber % AllyPingInterval != 0)
            return;

        Broadcast(new SwarmMessage
        {
            Type = SwarmMessageType.AllyPing,
            SenderName = Name,
            CustomData = $"{Config.FormationSlot}:{State.Energy:F1}",
            Timestamp = Arena.TickNumber
        });
    }

    private static Vector2D PolarOffset(Vector2D center, double angleDeg, double radius)
    {
        double rad = angleDeg * Math.PI / 180.0;
        return new Vector2D(
            center.X + radius * Math.Sin(rad),
            center.Y - radius * Math.Cos(rad));
    }

    private static double RelativeBearing(double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }

    private Vector2D FurthestArenaCorner()
    {
        Vector2D[] corners =
        [
            new(50, 50),
            new(Arena.ArenaWidth - 50, 50),
            new(50, Arena.ArenaHeight - 50),
            new(Arena.ArenaWidth - 50, Arena.ArenaHeight - 50)
        ];

        long freshCutoff = Arena.TickNumber - AllyStaleTicks;
        List<RadarContact> freshEnemies = RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .ToList();

        if (freshEnemies.Count == 0)
            return new Vector2D(Arena.ArenaWidth / 2, Arena.ArenaHeight / 2);

        Vector2D best = corners[0];
        double bestDist = 0;
        foreach (Vector2D corner in corners)
        {
            double totalDist = freshEnemies.Sum(e => corner.DistanceTo(e.Position));
            if (totalDist > bestDist)
            {
                bestDist = totalDist;
                best = corner;
            }
        }
        return best;
    }

    private int GetAliveAllyCount()
    {
        long freshCutoff = Arena.TickNumber - AllyStaleTicks;
        return _allyMap.Values.Count(e => e.LastSeen >= freshCutoff) - 1; // subtract self
    }

    public override void OnSwarmMessage(SwarmMessageEventArgs e)
    {
        base.OnSwarmMessage(e); // handles RadarShare

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
