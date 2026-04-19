using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTank;

internal static class SwarmBrainBaseNavigationExtensions
{
    internal static void NavigateTo(this SwarmBrainBase brain, Vector2D dest, double stopDistance)
    {
        double distToDest = brain.State.Position.DistanceTo(dest);
        if (distToDest <= stopDistance)
            return;

        double bearing = brain.State.Position.BearingTo(dest);
        double bodyTurn = (bearing - brain.State.Heading).RelativeBearing();
        bodyTurn = Math.Clamp(bodyTurn, -ArenaConstants.MaxTurnRate, ArenaConstants.MaxTurnRate);
        brain.SetTurnRight(bodyTurn);

        if (Math.Abs(bodyTurn) < 30)
            brain.SetAhead(Math.Min(distToDest - stopDistance, 100));
        else
            brain.SetAhead(40);
    }

    internal static void MaintainRadar(this SwarmBrainBase brain, Vector2D? focusPoint)
    {
        if (focusPoint.HasValue)
        {
            double radarBearing = brain.State.Position.BearingTo(focusPoint.Value);
            if (!double.IsFinite(radarBearing)) { brain.SetTurnRadarRight(brain._radarSpin); return; }
            double radarDiff = (radarBearing - brain.State.RadarHeading).RelativeBearing();
            brain.SetTurnRadarRight(Math.Clamp(radarDiff * 1.5, -45, 45));
        }
        else
        {
            brain.SetTurnRadarRight(brain._radarSpin);
        }
    }

    internal static void LinearPredictionFire(this SwarmBrainBase brain, RadarContact target)
    {
        double power = Math.Min(brain.Config.MaxFirePower, brain.State.Energy * 0.1);
        if (power < 0.1 || brain.State.Energy < 5)
            return;

        double bulletSpeed = 20.0 - 3.0 * power;
        double dist = brain.State.Position.DistanceTo(target.Position);
        double travelTime = dist / bulletSpeed;

        double predictedX = target.Position.X + target.VelocityVector.X * travelTime;
        double predictedY = target.Position.Y + target.VelocityVector.Y * travelTime;
        Vector2D predictedPos = new(predictedX, predictedY);

        double desiredGunBearing = brain.State.Position.BearingTo(predictedPos);
        double gunDiff = (desiredGunBearing - brain.State.GunHeading).RelativeBearing();
        brain.SetTurnGunRight(Math.Clamp(gunDiff, -ArenaConstants.MaxGunTurnRate, ArenaConstants.MaxGunTurnRate));

        if (Math.Abs(gunDiff) < 5.0)
            brain.SetFire(power);
    }

    internal static Vector2D FurthestArenaCorner(this SwarmBrainBase brain)
    {
        Vector2D[] corners =
        [
            new(50, 50),
            new(brain.Arena.ArenaWidth - 50, 50),
            new(50, brain.Arena.ArenaHeight - 50),
            new(brain.Arena.ArenaWidth - 50, brain.Arena.ArenaHeight - 50)
        ];

        long freshCutoff = brain.Arena.TickNumber - SwarmBrainBase.AllyStaleTicks;
        List<RadarContact> freshEnemies = brain.RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .ToList();

        if (freshEnemies.Count == 0)
            return new Vector2D(brain.Arena.ArenaWidth / 2, brain.Arena.ArenaHeight / 2);

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
}
