using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

internal static class SwarmBrainBaseStrategyExtensions
{
    internal static bool IsEnemyEcmActive(this SwarmTankCortexCradleBase brain)
        => brain.Arena.TickNumber - brain.EnemyEcmAlertTick < 20;

    internal static void HandleEcm(this SwarmTankCortexCradleBase brain)
    {
        if (brain.TankConfig.HasEcm)
        {
            if (brain.ActiveSwarmStrategy == SwarmStrategy.ECMScreen)
                brain.SetEcm(brain.TankConfig.OffensiveEcmMode);
            else if (brain.IsEnemyEcmActive())
                brain.SetEcm(EcmMode.Burnthrough);
            else
                brain.SetEcm(EcmMode.Off);
        }
        else
        {
            if (brain.IsEnemyEcmActive())
                brain.SetEcm(EcmMode.Burnthrough);
            else
                brain.SetEcm(EcmMode.Off);
        }
    }

    internal static void ExecuteStrategy(this SwarmTankCortexCradleBase brain)
    {
        RadarContact? target = brain.GetStrategyTarget();

        switch (brain.ActiveSwarmStrategy)
        {
            case SwarmStrategy.Wolfpack:
                if (target != null) brain.ExecuteWolfpack(target); else brain.Scout();
                break;
            case SwarmStrategy.Encircle:
                if (target != null) brain.ExecuteEncircle(target); else brain.Scout();
                break;
            case SwarmStrategy.Pincer:
                if (target != null) brain.ExecutePincer(target); else brain.Scout();
                break;
            case SwarmStrategy.ECMScreen:
                if (target != null) brain.ExecuteECMScreen(target); else brain.Scout();
                break;
            case SwarmStrategy.Fallback:
                brain.ExecuteFallback();
                break;
            case SwarmStrategy.Scatter:
                brain.ExecuteScatter();
                break;
        }
    }

    internal static RadarContact? GetStrategyTarget(this SwarmTankCortexCradleBase brain)
    {
        if (!string.IsNullOrEmpty(brain.PriorityTargetName)
            && brain.RadarMap.TryGetValue(brain.PriorityTargetName, out RadarContact? named)
            && brain.Arena.TickNumber - named.Timestamp < 30)
        {
            return named;
        }
        return brain.GetFreshestEnemy(30);
    }

    internal static void ExecuteWolfpack(this SwarmTankCortexCradleBase brain, RadarContact target)
    {
        brain.NavigateTo(target.Position, brain.TankConfig.PreferredRange);
        brain.MaintainRadar(target.Position);
        brain.LinearPredictionFire(target);
    }

    internal static void ExecuteEncircle(this SwarmTankCortexCradleBase brain, RadarContact target)
    {
        int aliveCount = brain.GetAliveAllyCount() + 1;
        int mySlot = brain.TankConfig.FormationSlot % aliveCount;
        double orbitAngleDeg = mySlot * (360.0 / aliveCount);
        Vector2D orbitPoint = target.Position.PolarOffset(orbitAngleDeg, SwarmTankCortexCradleBase.OrbitRadius);

        brain.NavigateTo(orbitPoint, 0);
        brain.MaintainRadar(target.Position);

        double dist = brain.State.Position.DistanceTo(target.Position);
        if (dist < 220)
            brain.LinearPredictionFire(target);
    }

    internal static void ExecutePincer(this SwarmTankCortexCradleBase brain, RadarContact target)
    {
        double groupAngle = brain.TankConfig.FormationSlot <= 1 ? 0.0 : 180.0;
        Vector2D approachPoint = target.Position.PolarOffset(groupAngle, 200.0);
        brain.NavigateTo(approachPoint, 50.0);
        brain.MaintainRadar(target.Position);
        brain.LinearPredictionFire(target);
    }

    internal static void ExecuteECMScreen(this SwarmTankCortexCradleBase brain, RadarContact target)
    {
        if (brain.TankConfig.HasEcm)
        {
            // ECM already set by HandleEcm(); keep safe distance
            brain.NavigateTo(target.Position, 120.0);
        }
        else
        {
            brain.NavigateTo(target.Position, 130.0);
            brain.LinearPredictionFire(target);
        }
        brain.MaintainRadar(target.Position);
    }

    internal static void ExecuteFallback(this SwarmTankCortexCradleBase brain)
    {
        Vector2D rallyCorner = brain.FurthestArenaCorner();
        brain.NavigateTo(rallyCorner, 0);
        brain.MaintainRadar(null);
        // Do NOT fire — preserve energy
    }

    internal static void ExecuteScatter(this SwarmTankCortexCradleBase brain)
    {
        RadarContact? enemy = brain.GetFreshestEnemy(60);
        if (enemy != null)
        {
            double fleeHeading = (enemy.Position.BearingTo(brain.State.Position) + 360) % 360;
            double bodyTurn = (fleeHeading - brain.State.Heading).RelativeBearing();
            brain.SetTurnRight(Math.Clamp(bodyTurn, -ArenaConstants.MaxTurnRate, ArenaConstants.MaxTurnRate));
            brain.SetAhead(200);
        }
        else
        {
            brain.SetAhead(200);
        }
        brain.SetTurnRadarRight(brain.RadarSpin);
    }

    internal static void Scout(this SwarmTankCortexCradleBase brain)
    {
        brain.SetTurnRadarRight(45);
        brain.SetAhead(100);
    }
}
