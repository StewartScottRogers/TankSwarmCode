using TankSwarmCode.AiCortex.Blue.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.AiCortexes;

public sealed class BlueRushCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 2,
        MaxFirePower = 2.5,
        PreferredRange = 180.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
