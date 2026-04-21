using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueLancerCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 9,
        MaxFirePower = 2.5,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 0
    };
}
