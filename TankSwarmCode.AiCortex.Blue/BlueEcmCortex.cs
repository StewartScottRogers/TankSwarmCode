using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueEcmCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 4,
        MaxFirePower = 2.0,
        PreferredRange = 160.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.Jam,
        RetreatEnergyThreshold = 35.0
    };
}
