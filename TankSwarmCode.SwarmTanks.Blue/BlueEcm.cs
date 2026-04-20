using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueEcm : SwarmTankBlueCortexCradle
{
    public BlueEcm() { SwarmId = 2; Role = TankRole.EcmSpecialist; }
    public override string Name => "BlueEcm";
    protected internal override TankConfiguration TankConfig { get; } = new()
    {
        FormationSlot = 4,
        MaxFirePower = 1.5,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.Jam,
        RetreatEnergyThreshold = 35.0
    };
}
