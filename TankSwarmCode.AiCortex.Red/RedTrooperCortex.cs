using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedTrooperCortex : RedCortexBase
{
    public RedTrooperCortex(int slot) =>
        Config = new TankConfiguration { FormationSlot = slot, MaxFirePower = 2.5, PreferredRange = 200.0, HasEcm = false };

    protected override TankConfiguration Config { get; }
}
