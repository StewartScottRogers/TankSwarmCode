using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex;

internal static class TankNavigation
{
    internal static void NavigateTo(ITankContext ctx, Vector2D dest, double stopDistance)
    {
        double distToDest = ctx.State.Position.DistanceTo(dest);
        if (distToDest <= stopDistance) return;

        Vector2D pos = ctx.State.Position;
        double W = ctx.Arena.ArenaWidth;
        double H = ctx.Arena.ArenaHeight;
        const double avoidDist = 80.0;

        double destBearing = pos.BearingTo(dest);
        double destRad = destBearing * (Math.PI / 180.0);
        double vx = Math.Sin(destRad);
        double vy = -Math.Cos(destRad);

        if (pos.X     < avoidDist) vx += (avoidDist - pos.X)       / avoidDist;
        if (W - pos.X < avoidDist) vx -= (avoidDist - (W - pos.X)) / avoidDist;
        if (pos.Y     < avoidDist) vy += (avoidDist - pos.Y)       / avoidDist;
        if (H - pos.Y < avoidDist) vy -= (avoidDist - (H - pos.Y)) / avoidDist;

        double finalBearing = Math.Atan2(vx, -vy) * (180.0 / Math.PI);
        double bodyTurn = (finalBearing - ctx.State.Heading).RelativeBearing();
        bodyTurn = Math.Clamp(bodyTurn, -ArenaConstants.MaxTurnRate, ArenaConstants.MaxTurnRate);
        ctx.SetTurnRight(bodyTurn);
        ctx.SetAhead(Math.Abs(bodyTurn) < 30 ? Math.Min(distToDest - stopDistance, 100) : 40);
    }

    internal static void MaintainRadar(ITankContext ctx, Vector2D? focusPoint, double radarSpin)
    {
        if (focusPoint.HasValue)
        {
            double radarBearing = ctx.State.Position.BearingTo(focusPoint.Value);
            if (!double.IsFinite(radarBearing)) { ctx.SetTurnRadarRight(radarSpin); return; }
            double radarDiff = (radarBearing - ctx.State.RadarHeading).RelativeBearing();
            ctx.SetTurnRadarRight(Math.Clamp(radarDiff * 1.5, -45, 45));
        }
        else
        {
            ctx.SetTurnRadarRight(radarSpin);
        }
    }

    internal static void LinearPredictionFire(ITankContext ctx, RadarContact target, double maxFirePower)
    {
        double power = Math.Min(maxFirePower, ctx.State.Energy * 0.1);
        if (power < 0.1 || ctx.State.Energy < 5) return;

        double bulletSpeed = 20.0 - 3.0 * power;
        double dist = ctx.State.Position.DistanceTo(target.Position);
        double travelTime = dist / bulletSpeed;

        Vector2D predictedPos = new(
            target.Position.X + target.VelocityVector.X * travelTime,
            target.Position.Y + target.VelocityVector.Y * travelTime);

        double desiredGunBearing = ctx.State.Position.BearingTo(predictedPos);
        double gunDiff = (desiredGunBearing - ctx.State.GunHeading).RelativeBearing();
        ctx.SetTurnGunRight(Math.Clamp(gunDiff, -ArenaConstants.MaxGunTurnRate, ArenaConstants.MaxGunTurnRate));

        if (Math.Abs(gunDiff) < 5.0 && !IsWallInLineOfFire(ctx, predictedPos))
            ctx.SetFire(power);
    }

    internal static bool IsWallInLineOfFire(ITankContext ctx, Vector2D target)
    {
        double ax = ctx.State.Position.X, ay = ctx.State.Position.Y;
        double bx = target.X, by = target.Y;

        foreach (BuildingEcho echo in ctx.BuildingWallMap.Values)
            foreach (WallSegment wall in echo.Walls)
                if (SegmentsIntersect(ax, ay, bx, by, wall.Start.X, wall.Start.Y, wall.End.X, wall.End.Y))
                    return true;
        return false;
    }

    internal static Vector2D FurthestArenaCorner(ITankContext ctx, long allyStaleTicks)
    {
        Vector2D[] corners =
        [
            new(50, 50),
            new(ctx.Arena.ArenaWidth - 50, 50),
            new(50, ctx.Arena.ArenaHeight - 50),
            new(ctx.Arena.ArenaWidth - 50, ctx.Arena.ArenaHeight - 50)
        ];

        long freshCutoff = ctx.Arena.TickNumber - allyStaleTicks;
        List<RadarContact> freshEnemies = ctx.RadarMap.Values
            .Where(c => !c.IsAlly && c.Timestamp >= freshCutoff)
            .ToList();

        if (freshEnemies.Count == 0)
            return new Vector2D(ctx.Arena.ArenaWidth / 2, ctx.Arena.ArenaHeight / 2);

        Vector2D best = corners[0];
        double bestDist = 0;
        foreach (Vector2D corner in corners)
        {
            double totalDist = freshEnemies.Sum(e => corner.DistanceTo(e.Position));
            if (totalDist > bestDist) { bestDist = totalDist; best = corner; }
        }
        return best;
    }

    private static bool SegmentsIntersect(double ax, double ay, double bx, double by,
                                          double cx, double cy, double dx, double dy)
    {
        double d1x = bx - ax, d1y = by - ay;
        double d2x = dx - cx, d2y = dy - cy;
        double cross = d1x * d2y - d1y * d2x;
        if (Math.Abs(cross) < 1e-10) return false;
        double t = ((cx - ax) * d2y - (cy - ay) * d2x) / cross;
        double u = ((cx - ax) * d1y - (cy - ay) * d1x) / cross;
        return t >= 0 && t <= 1 && u >= 0 && u <= 1;
    }
}
