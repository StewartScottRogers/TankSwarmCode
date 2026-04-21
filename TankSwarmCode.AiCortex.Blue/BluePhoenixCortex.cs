using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BluePhoenixCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 11,
        MaxFirePower = 2.5,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 0
    };
}
