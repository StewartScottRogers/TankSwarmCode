using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueStrike : SwarmTankBlueCortexCradle
{
    public BlueStrike() { SwarmId = 2; Role = TankRole.Attacker; }
    public override string Name => "BlueStrike";
    protected internal override TankConfiguration TankConfig { get; } = new()
    {
        FormationSlot = 0,
        MaxFirePower = 3.0,
        PreferredRange = 250.0,
        HasEcm = false,
        RetreatEnergyThreshold = 25.0
    };
}
