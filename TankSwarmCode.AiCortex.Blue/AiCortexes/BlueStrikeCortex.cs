using TankSwarmCode.AiCortex.Blue.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.AiCortexes;

public sealed class BlueStrikeCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 3.0,
        PreferredRange = 250.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
