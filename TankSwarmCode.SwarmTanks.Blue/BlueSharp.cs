using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueSharp : SwarmTankBlueCortexCradle
{
    public BlueSharp() { SwarmId = 2; Role = TankRole.Support; }
    public override string Name => "BlueSharp";
    protected internal override TankConfiguration TankConfig { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 3.0,
        PreferredRange = 300.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
