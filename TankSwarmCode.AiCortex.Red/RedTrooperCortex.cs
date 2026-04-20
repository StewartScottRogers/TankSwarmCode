using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedTrooperCortex : RedCortexBase
{
    public RedTrooperCortex(int slot) =>
        TankConfiguration = new TankConfiguration { FormationSlot = slot, MaxFirePower = 2.5, PreferredRange = 200.0, HasEcm = false };

    protected override TankConfiguration TankConfiguration { get; }
}
