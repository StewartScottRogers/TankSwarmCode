using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Red;

/// <summary>
/// Generic Red swarm fighter used when building arbitrary-size NvN teams.
/// Each instance gets a unique formation slot so the swarm brain assigns
/// distinct roles and orbit positions.
/// </summary>
public sealed class RedTrooper : SwarmBrainBase
{
    private readonly TankConfig _config;

    public RedTrooper(int slot)
    {
        SwarmId = 1;
        Role = TankRole.Attacker;
        _config = new TankConfig
        {
            FormationSlot = slot,
            MaxFirePower = 2.5,
            PreferredRange = 200.0,
            HasEcm = false,
            RetreatEnergyThreshold = 25.0
        };
    }

    public override string Name => $"Red{_config.FormationSlot}";
    protected override TankConfig Config => _config;
}
