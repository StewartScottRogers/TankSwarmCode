using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedGhostCortex : RedCortexBase
{
    protected override TankConfiguration Config { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 0.1,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.JamAndSpoof,
        RetreatEnergyThreshold = 40.0
    };
}
