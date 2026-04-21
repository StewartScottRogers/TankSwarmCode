using TankSwarmCode.AiCortex.Blue.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Blue;

public sealed class BlueSlotCortex : BlueCortexBase
{
    public BlueSlotCortex(int slot) =>
        TankConfiguration = new TankConfiguration { FormationSlot = slot, MaxFirePower = 1.0, PreferredRange = 160.0, HasEcm = false, RetreatEnergyThreshold = 0 };

    protected override TankConfiguration TankConfiguration { get; }
}
