using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueEcmCortex : BlueCortexBase
{
    protected override TankConfiguration Config { get; } = new()
    {
        FormationSlot = 4,
        MaxFirePower = 1.5,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.Jam,
        RetreatEnergyThreshold = 35.0
    };
}
