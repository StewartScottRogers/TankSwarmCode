using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedGhostCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 2.0,
        PreferredRange = 160.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };

}
