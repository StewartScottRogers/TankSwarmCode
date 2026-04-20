using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedGhostCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 3,
        MaxFirePower = 0.1,
        PreferredRange = 150.0,
        HasEcm = true,
        OffensiveEcmMode = EcmMode.JamAndSpoof,
        RetreatEnergyThreshold = 40.0
    };
}
