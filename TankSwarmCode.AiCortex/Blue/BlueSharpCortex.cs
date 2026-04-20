using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueSharpCortex : BlueCortexBase
{
    protected override TankConfiguration Config { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 3.0,
        PreferredRange = 300.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
