using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedBladeCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 2.5,
        PreferredRange = 160.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
