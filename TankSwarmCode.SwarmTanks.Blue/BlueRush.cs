using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueRush : SwarmBrainBaseBlue
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
