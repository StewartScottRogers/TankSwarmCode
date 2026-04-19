using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.Arena;

/// <summary>
/// Detects tanks that are physically stuck — velocity zeroed by collision resolution
/// every tick despite the AI issuing movement commands — and applies escalating recovery:
///   Level 1 (30 stuck-ticks): random nudge.
///   Level 2 (60 stuck-ticks): reverse nudge along current heading.
///   Level 3 (100 stuck-ticks): BFS teleport to nearest free cell, then full reset.
/// Non-stuck tanks and tanks that are intentionally parked are never touched.
/// </summary>
internal static class TankRecovery
{
    private const int    CheckInterval      = 10;   // ticks between position samples
    private const double MinMovement        = 5.0;  // px net displacement required over CheckInterval to be "not stuck"
    private const int    SoftThreshold      = 30;   // stuck-ticks before level-1 nudge
    private const int    ReverseThreshold   = 60;   // stuck-ticks before level-2 nudge
    private const int    TeleportThreshold  = 100;  // stuck-ticks before level-3 BFS teleport

    internal static void Apply(
        TankRuntimeState rts,
        long tickNumber,
        IReadOnlyList<BuildingDefinition> buildings,
        IReadOnlyList<TankRuntimeState> allTanks,
        double arenaWidth,
        double arenaHeight,
        Random rng)
    {
        // Tank is intentionally parked — don't interfere
        if (!rts.WantedToMove)
        {
            Reset(rts, tickNumber);
            return;
        }

        // First call: seed reference position
        if (double.IsNaN(rts.StuckCheckX))
        {
            Reset(rts, tickNumber);
            return;
        }

        // Only sample every CheckInterval ticks
        if (tickNumber - rts.StuckCheckTick < CheckInterval) return;

        double dx    = rts.X - rts.StuckCheckX;
        double dy    = rts.Y - rts.StuckCheckY;
        double moved = Math.Sqrt(dx * dx + dy * dy);

        rts.StuckCheckX    = rts.X;
        rts.StuckCheckY    = rts.Y;
        rts.StuckCheckTick = tickNumber;

        if (moved >= MinMovement)
        {
            rts.StuckTicks = 0;
            rts.StuckLevel = 0;
            return;
        }

        rts.StuckTicks += CheckInterval;

        // Level 3: BFS teleport
        if (rts.StuckTicks >= TeleportThreshold)
        {
            Vector2D? free = FindNearestFreeCell(rts, buildings, allTanks, arenaWidth, arenaHeight);
            if (free.HasValue)
            {
                Console.WriteLine($"[UNSTICK] {rts.Tank.Name} t={tickNumber} stuck={rts.StuckTicks}t → teleport ({rts.X:F1},{rts.Y:F1})→({free.Value.X:F1},{free.Value.Y:F1})");
                rts.X = free.Value.X;
                rts.Y = free.Value.Y;
            }
            else
            {
                Console.WriteLine($"[UNSTICK] {rts.Tank.Name} t={tickNumber} stuck={rts.StuckTicks}t → teleport FAILED (no free cell found)");
            }
            rts.Velocity = 0;
            Reset(rts, tickNumber);
            return;
        }

        // Level 2: reverse nudge (fires once when threshold first crossed)
        if (rts.StuckTicks >= ReverseThreshold && rts.StuckLevel < 2)
        {
            rts.StuckLevel = 2;
            double headRad = rts.Heading * (Math.PI / 180.0);
            double dist    = 20.0 + rng.NextDouble() * 10.0;
            Nudge(rts, -Math.Sin(headRad) * dist, Math.Cos(headRad) * dist, buildings, arenaWidth, arenaHeight);
            Console.WriteLine($"[UNSTICK] {rts.Tank.Name} t={tickNumber} stuck={rts.StuckTicks}t → reverse nudge → ({rts.X:F1},{rts.Y:F1})");
            return;
        }

        // Level 1: random nudge (fires once when threshold first crossed)
        if (rts.StuckTicks >= SoftThreshold && rts.StuckLevel < 1)
        {
            rts.StuckLevel = 1;
            double angle = rng.NextDouble() * 2 * Math.PI;
            double dist  = 12.0 + rng.NextDouble() * 8.0;
            Nudge(rts, Math.Cos(angle) * dist, Math.Sin(angle) * dist, buildings, arenaWidth, arenaHeight);
            Console.WriteLine($"[UNSTICK] {rts.Tank.Name} t={tickNumber} stuck={rts.StuckTicks}t → soft nudge → ({rts.X:F1},{rts.Y:F1})");
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void Reset(TankRuntimeState rts, long tickNumber)
    {
        rts.StuckTicks     = 0;
        rts.StuckLevel     = 0;
        rts.StuckCheckX    = rts.X;
        rts.StuckCheckY    = rts.Y;
        rts.StuckCheckTick = tickNumber;
    }

    /// <summary>
    /// Moves the tank by (ddx, ddy) if the destination is free.
    /// Falls back to the original position if it would land inside a building.
    /// Zeroes velocity and updates the stuck reference point either way.
    /// </summary>
    private static void Nudge(
        TankRuntimeState rts,
        double ddx, double ddy,
        IReadOnlyList<BuildingDefinition> buildings,
        double arenaWidth, double arenaHeight)
    {
        double half = ArenaConstants.TankHalfSize;
        double nx   = Math.Clamp(rts.X + ddx, half, arenaWidth  - half);
        double ny   = Math.Clamp(rts.Y + ddy, half, arenaHeight - half);

        if (!OverlapsBuilding(nx, ny, buildings))
        {
            rts.X = nx;
            rts.Y = ny;
        }

        rts.Velocity    = 0;
        rts.StuckCheckX = rts.X;
        rts.StuckCheckY = rts.Y;
    }

    /// <summary>
    /// Searches outward from the tank's current position in concentric rings,
    /// returning the nearest (X,Y) that is inside arena bounds, clear of all
    /// buildings, and not overlapping any other living tank.
    /// Returns null if no free cell exists within 120 px.
    /// </summary>
    private static Vector2D? FindNearestFreeCell(
        TankRuntimeState rts,
        IReadOnlyList<BuildingDefinition> buildings,
        IReadOnlyList<TankRuntimeState> allTanks,
        double arenaWidth, double arenaHeight)
    {
        const int    stepPx    = 4;
        const int    maxRadius = 120;
        double half            = ArenaConstants.TankHalfSize;
        double minSepSq        = (half * 2 + 1) * (half * 2 + 1);

        for (int r = stepPx; r <= maxRadius; r += stepPx)
        {
            int samples = Math.Max(8, (int)(2 * Math.PI * r / stepPx));
            for (int s = 0; s < samples; s++)
            {
                double angle = 2 * Math.PI * s / samples;
                double cx    = Math.Clamp(rts.X + Math.Cos(angle) * r, half, arenaWidth  - half);
                double cy    = Math.Clamp(rts.Y + Math.Sin(angle) * r, half, arenaHeight - half);

                if (OverlapsBuilding(cx, cy, buildings)) continue;

                bool blocked = false;
                foreach (TankRuntimeState other in allTanks)
                {
                    if (other == rts || !other.IsAlive) continue;
                    double odx = cx - other.X;
                    double ody = cy - other.Y;
                    if (odx * odx + ody * ody < minSepSq) { blocked = true; break; }
                }

                if (!blocked) return new Vector2D(cx, cy);
            }
        }

        return null;
    }

    private static bool OverlapsBuilding(double x, double y, IReadOnlyList<BuildingDefinition> buildings)
    {
        double half = ArenaConstants.TankHalfSize;
        foreach (BuildingDefinition b in buildings)
        {
            if (x + half > b.X      && x - half < b.X + b.Width &&
                y + half > b.Y      && y - half < b.Y + b.Height)
                return true;
        }
        return false;
    }
}
