using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueStrikeCortex : BlueCortexBase
{
    protected override TankConfiguration Config { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 3.0,
        PreferredRange = 250.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
