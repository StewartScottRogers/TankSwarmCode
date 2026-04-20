using TankSwarmCode.SwarmTank.Enums;

namespace TankSwarmCode.SwarmTank.Models;

/// <summary>
/// Physical and tactical configuration for a swarm tank.
/// All tanks run the same <c>SwarmBrainBase</c> logic; only this record differs between them.
/// </summary>
public record TankConfiguration
{
    /// <summary>
    /// Position in the formation (0 = highest authority / default leader).
    /// Determines encircle angle slot and pincer group assignment.
    /// </summary>
    public int FormationSlot { get; init; }

    /// <summary>Maximum fire power this tank will use (0.1–3.0).</summary>
    public double MaxFirePower { get; init; } = 2.0;

    /// <summary>Preferred combat engagement range in pixels.</summary>
    public double PreferredRange { get; init; } = 200.0;

    /// <summary>Whether this tank is equipped with ECM hardware.</summary>
    public bool HasEcm { get; init; }

    /// <summary>ECM mode to activate when the swarm selects <see cref="SwarmStrategy.ECMScreen"/>.</summary>
    public EcmMode OffensiveEcmMode { get; init; } = EcmMode.Jam;

    /// <summary>Energy level at which this tank switches to <see cref="SwarmStrategy.Fallback"/> behaviour.</summary>
    public double RetreatEnergyThreshold { get; init; } = 25.0;
}
