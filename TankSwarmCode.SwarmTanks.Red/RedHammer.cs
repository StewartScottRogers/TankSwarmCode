using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Primary attacker and default swarm leader (formation slot 0).
/// Highest firepower (3.0) at mid range. Leads strategy broadcasts and volley coordination.
/// </summary>
public sealed class RedHammer : SwarmBrainBase
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
