using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// ECM specialist (formation slot 3). Fires minimally (power 0.1) and instead disrupts
/// enemy radar using <see cref="EcmMode.JamAndSpoof"/> during ECMScreen strategy.
/// High retreat threshold (40 energy) keeps the ECM capability on the field longer.
/// </summary>
public sealed class RedGhost : SwarmBrainBase
{
    public RedGhost() { SwarmId = 1; Role = TankRole.EcmSpecialist; }
    public override string Name => "RedGhost";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 0.1,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.JamAndSpoof,
        RetreatEnergyThreshold = 40.0
    };
}
