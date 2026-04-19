using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Secondary attacker (formation slot 1). Slightly lower firepower than RedHammer but
/// a tighter preferred range (160 px), making it effective in close-quarters engagements.
/// </summary>
public sealed class RedBlade : SwarmBrainBase
{
    public RedBlade() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "RedBlade";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 2.5,
        PreferredRange = 160.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
