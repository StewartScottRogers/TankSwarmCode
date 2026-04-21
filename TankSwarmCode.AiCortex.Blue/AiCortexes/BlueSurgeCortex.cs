using TankSwarmCode.AiCortex.Blue.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.AiCortexes;

public sealed class BlueSurgeCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 6,
        MaxFirePower = 2.5,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 0
    };
}
