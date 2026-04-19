using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Scout (formation slot 2). Optimised for reconnaissance: longest preferred range (220 px)
/// and low firepower (1.5) so it prioritises staying alive and feeding radar data to allies.
/// </summary>
public sealed class RedArrow : SwarmBrainBase
{
    public RedArrow() { SwarmId = 1; Role = TankRole.Scout; }
    public override string Name => "RedArrow";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 2,
        MaxFirePower = 1.5,
        PreferredRange = 220.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
