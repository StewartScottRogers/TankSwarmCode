using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Telemetry;

/// <summary>Complete state snapshot for one tank at one tick.</summary>
public record TankTickRecord
{
    public long Tick { get; init; }
    public string TankName { get; init; } = "";
    public int SwarmId { get; init; }

    // Position & movement
    public double X { get; init; }
    public double Y { get; init; }
    public double Heading { get; init; }
    public double Velocity { get; init; }
    public double GunHeading { get; init; }
    public double RadarHeading { get; init; }

    // Vitals
    public double Energy { get; init; }
    public bool IsAlive { get; init; }
    public EcmMode ActiveEcm { get; init; }

    // Command the AI issued this tick (captured before the engine applied it)
    public double CmdMove { get; init; }
    public double CmdBodyTurn { get; init; }
    public double CmdGunTurn { get; init; }
    public double CmdRadarTurn { get; init; }
    public double CmdFirePower { get; init; }
    public EcmMode CmdEcm { get; init; }

    // Radar picture this tank held at end of tick
    public IReadOnlyList<RadarContact> RadarContacts { get; init; } = [];

    // Events that occurred to this tank this tick
    public IReadOnlyList<TankEventRecord> Events { get; init; } = [];
}
