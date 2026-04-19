using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Close-range attacker (Slot 2). Shortest engagement range of the non-ECM tanks (180 px);
/// aggressive retreat threshold lets it press flanks in Pincer and Encircle formations.
/// </summary>
public sealed class BlueRush : SwarmBrainBase
{
    public BlueRush() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "BlueRush";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 2,
        MaxFirePower = 2.5,
        PreferredRange = 180.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
