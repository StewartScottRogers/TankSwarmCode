using TankSwarmCode.SwarmTank.Enums;

namespace TankSwarmCode.SwarmTank.Models;

/// <summary>
/// Captures all actions a tank wishes to perform in the current tick.
/// Set via <c>SwarmTank.Set*()</c> methods; flushed by the engine once per tick.
/// All fields default to zero (no-op).
/// </summary>
public record TankCommand
{
    /// <summary>Desired body turn in degrees. Positive = right / clockwise.</summary>
    public double BodyTurnDegrees { get; init; }

    /// <summary>
    /// Desired distance to travel. Positive = forward, negative = reverse.
    /// Actual movement per tick is capped by Robocode acceleration rules.
    /// </summary>
    public double MoveDistance { get; init; }

    /// <summary>Desired gun turn in degrees relative to current gun heading. Positive = right.</summary>
    public double GunTurnDegrees { get; init; }

    /// <summary>Desired radar turn in degrees relative to current radar heading. Positive = right.</summary>
    public double RadarTurnDegrees { get; init; }

    /// <summary>
    /// Fire power in [0.1, 3.0]. Zero means do not fire this tick.
    /// </summary>
    public double FirePower { get; init; }

    /// <summary>Messages to broadcast to swarm allies this tick.</summary>
    public IReadOnlyList<SwarmMessage> BroadcastMessages { get; init; } = [];

    /// <summary>
    /// ECM mode to activate this tick. Defaults to <see cref="EcmMode.Off"/>.
    /// Active modes drain energy each tick; the engine silently ignores the request
    /// if the tank lacks sufficient energy.
    /// </summary>
    public EcmMode EcmMode { get; init; }
}
