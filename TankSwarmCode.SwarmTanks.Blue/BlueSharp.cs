using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

public sealed class BlueSharp : SwarmBrainBaseBlue
{
    public BlueSharp() { SwarmId = 2; Role = TankRole.Support; }
    public override string Name => "BlueSharp";
    protected internal override TankConfig Config { get; } = new()
    {
        FormationSlot = 1,
        MaxFirePower = 3.0,
        PreferredRange = 300.0,
        HasEcm = false,
        RetreatEnergyThreshold = 20.0
    };
}
