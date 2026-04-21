using TankSwarmCode.AiCortex.Red.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red.AiCortexes;

public sealed class RedHammerCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 1.5,
        PreferredRange = 160.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
