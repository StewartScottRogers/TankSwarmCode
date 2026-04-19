using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

internal static class SwarmBrainBaseStrategyExtensions
{
    internal static bool IsEnemyEcmActive(this SwarmBrainBase brain)
        => brain.Arena.TickNumber - brain._enemyEcmAlertTick < 20;

    internal static void HandleEcm(this SwarmBrainBase brain)
    {
        if (brain.Config.HasEcm)
        {
            if (brain._activeStrategy == SwarmStrategy.ECMScreen)
                brain.SetEcm(brain.Config.OffensiveEcmMode);
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

    internal static void ExecuteStrategy(this SwarmBrainBase brain)
    {
        RadarContact? target = brain.GetStrategyTarget();

        switch (brain._activeStrategy)
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

    internal static RadarContact? GetStrategyTarget(this SwarmBrainBase brain)
    {
        if (!string.IsNullOrEmpty(brain._priorityTargetName)
            && brain.RadarMap.TryGetValue(brain._priorityTargetName, out RadarContact? named)
            && brain.Arena.TickNumber - named.Timestamp < 30)
        {
            return named;
        }
        return brain.GetFreshestEnemy(30);
    }

    internal static void ExecuteWolfpack(this SwarmBrainBase brain, RadarContact target)
    {
        brain.NavigateTo(target.Position, brain.Config.PreferredRange);
        brain.MaintainRadar(target.Position);
        brain.LinearPredictionFire(target);
    }

    internal static void ExecuteEncircle(this SwarmBrainBase brain, RadarContact target)
    {
        int aliveCount = brain.GetAliveAllyCount() + 1;
        int mySlot = brain.Config.FormationSlot % aliveCount;
        double orbitAngleDeg = mySlot * (360.0 / aliveCount);
        Vector2D orbitPoint = target.Position.PolarOffset(orbitAngleDeg, SwarmBrainBase.OrbitRadius);

        brain.NavigateTo(orbitPoint, 0);
        brain.MaintainRadar(target.Position);

        double dist = brain.State.Position.DistanceTo(target.Position);
        if (dist < 220)
            brain.LinearPredictionFire(target);
    }

    internal static void ExecutePincer(this SwarmBrainBase brain, RadarContact target)
    {
        double groupAngle = brain.Config.FormationSlot <= 1 ? 0.0 : 180.0;
        Vector2D approachPoint = target.Position.PolarOffset(groupAngle, 200.0);
        brain.NavigateTo(approachPoint, 50.0);
        brain.MaintainRadar(target.Position);
        brain.LinearPredictionFire(target);
    }

    internal static void ExecuteECMScreen(this SwarmBrainBase brain, RadarContact target)
    {
        if (brain.Config.HasEcm)
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

    internal static void ExecuteFallback(this SwarmBrainBase brain)
    {
        Vector2D rallyCorner = brain.FurthestArenaCorner();
        brain.NavigateTo(rallyCorner, 0);
        brain.MaintainRadar(null);
        // Do NOT fire — preserve energy
    }

    internal static void ExecuteScatter(this SwarmBrainBase brain)
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
        brain.SetTurnRadarRight(brain._radarSpin);
    }

    internal static void Scout(this SwarmBrainBase brain)
    {
        brain.SetTurnRadarRight(45);
        brain.SetAhead(100);
    }
}
