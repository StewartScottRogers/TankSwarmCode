using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.Arena;

/// <summary>Mutable, engine-internal state for a single tank during a round.</summary>
internal sealed class TankRuntimeState
{
    internal TankRuntimeState(ISwarmTank tank)
    {
        Tank = tank;
    }

    internal ISwarmTank Tank { get; }

    internal double X { get; set; }
    internal double Y { get; set; }

    /// <summary>Body heading in degrees (0 = north, clockwise).</summary>
    internal double Heading { get; set; }
    internal double GunHeading { get; set; }
    internal double RadarHeading { get; set; }
    internal double PrevRadarHeading { get; set; }
    internal double Velocity { get; set; }
    internal double Energy { get; set; }
    internal bool IsAlive { get; set; }

    /// <summary>Tick on which this tank was destroyed; 0 while alive.</summary>
    internal long DestroyedAtTick { get; set; }

    /// <summary>The ECM mode active this tick, set by <see cref="ArenaEngine.ApplyEcm"/>.</summary>
    internal EcmMode ActiveEcm { get; set; }

    /// <summary>
    /// When non-null, overrides whatever ECM mode the tank AI requested this tick.
    /// Set externally (e.g., the inspector panel) to force a specific mode.
    /// <c>null</c> means the tank AI's own <see cref="ISwarmTank.SetEcm"/> call is used.
    /// </summary>
    internal EcmMode? EcmModeOverride { get; set; }

    /// <summary>
    /// Ghost echo positions projected by this tank when in <see cref="EcmMode.Spoof"/> mode.
    /// Each entry is (X, Y, Heading, Velocity) in arena space.
    /// Maintained and updated each tick by <see cref="ArenaEngine.UpdateGhostPositions"/>.
    /// </summary>
    internal (double X, double Y, double Heading, double Velocity)[] GhostPositions { get; set; } = [];

    /// <summary>Pushes mutable fields into the immutable <see cref="TankState"/> and calls <see cref="ISwarmTank.UpdateState"/>.</summary>
    internal void SyncToTank() => Tank.UpdateState(ToTankState());

    internal TankState ToTankState() => new()
    {
        Name = Tank.Name,
        SwarmId = Tank.SwarmId,
        Role = Tank.Role,
        Position = new Vector2D(X, Y),
        Heading = Heading,
        GunHeading = GunHeading,
        RadarHeading = RadarHeading,
        PrevRadarHeading = PrevRadarHeading,
        Velocity = Velocity,
        Energy = Energy,
        IsAlive = IsAlive,
        DestroyedAtTick = DestroyedAtTick,
        ActiveEcm = ActiveEcm
    };
}
