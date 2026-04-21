using TankSwarmCode.AiCortex.Blue.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.AiCortexes;

public sealed class BlueGuardCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 3.0,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 30.0
    };
}
