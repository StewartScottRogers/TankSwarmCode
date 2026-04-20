using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public sealed class RedArrow : SwarmTankBrainRed
{
    public RedArrow() { SwarmId = 1; Role = TankRole.Scout; }
    public override string Name => "RedArrow";
    protected internal override TankConfig TankConfig { get; } = new()
    {
        FormationSlot = 2,
        MaxFirePower = 1.5,
        PreferredRange = 220.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
