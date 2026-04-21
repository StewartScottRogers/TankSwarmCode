using TankSwarmCode.AiCortex.Blue.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue.AiCortexes;

public sealed class BlueSharpCortex : BlueCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 3.0,
        PreferredRange = 300.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
