using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public sealed class RedBlade : SwarmTankBrainRed
{
    public RedBlade() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "RedBlade";
    protected internal override TankConfig TankConfig { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 2.5,
        PreferredRange = 160.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
