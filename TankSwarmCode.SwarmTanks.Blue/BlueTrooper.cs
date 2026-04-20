using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.SwarmTanks.Blue;

/// <summary>
/// Generic Blue swarm fighter used when building arbitrary-size NvN teams.
/// Each instance gets a unique formation slot so the swarm brain assigns
/// distinct roles and orbit positions.
/// </summary>
public sealed class BlueTrooper : SwarmBrainBaseBlue
{
    private readonly TankConfig _config;

    public BlueTrooper(int slot)
    {
        SwarmId = 2;
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

    public override string Name => $"Blue{_config.FormationSlot}";
    protected internal override TankConfig TankConfig => _config;
}
