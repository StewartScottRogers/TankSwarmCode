using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public sealed class RedHammer : SwarmBrainBaseRed
{
    public RedHammer() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "RedHammer";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 3.0,
        PreferredRange = 200.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
