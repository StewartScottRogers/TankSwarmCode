using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedHammerCortex : RedCortexBase
{
    protected override TankConfiguration Config { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 3.0,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
