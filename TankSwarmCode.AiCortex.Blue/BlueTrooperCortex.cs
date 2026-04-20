using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueTrooperCortex : BlueCortexBase
{
    public BlueTrooperCortex(int slot) =>
        TankConfiguration = new TankConfiguration { FormationSlot = slot, MaxFirePower = 2.5, PreferredRange = 200.0, HasEcm = false };

    protected override TankConfiguration TankConfiguration { get; }
}
