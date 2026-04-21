using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedArrowCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 2,
        MaxFirePower = 2.0,
        PreferredRange = 160.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
