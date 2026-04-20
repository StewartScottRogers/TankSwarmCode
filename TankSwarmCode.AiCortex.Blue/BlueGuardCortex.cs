using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueGuardCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 2.0,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 30.0
    };
}
