using TankSwarmCode.AiCortex.Red.AiCortexes.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red.AiCortexes;

public sealed class RedTrooperCortex : RedCortexBase
{
    public RedTrooperCortex(int slot) =>
        TankConfiguration = new TankConfiguration { FormationSlot = slot, MaxFirePower = 2.0, PreferredRange = 160.0, HasEcm = false, RetreatEnergyThreshold = 20.0 };

    protected override TankConfiguration TankConfiguration { get; }
}
