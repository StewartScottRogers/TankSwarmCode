using TankSwarmCode.SwarmTank.Interfaces;
using TankSwarmCode.SwarmTank.Interfaces.Models;

namespace TankSwarmCode.Arena;

/// <summary>
/// Provides a tank's read-only window into the arena state.
/// One shared instance is passed to all tanks; data is live (backed by the engine).
/// </summary>
internal sealed class ArenaContext : IArenaContext
{
    private readonly ArenaEngine _engine;

    internal ArenaContext(ArenaEngine engine) => _engine = engine;

    public double ArenaWidth => _engine.Width;
    public double ArenaHeight => _engine.Height;
    public int LivingTankCount => _engine.LivingTankCount;
    public long TickNumber => _engine.TickNumber;

    public int GetSwarmSize(int swarmId) =>
        _engine.RuntimeTanks.Count(t => t.IsAlive && t.Tank.SwarmId == swarmId);

    public IReadOnlyList<BulletState> GetActiveBullets() =>
        _engine.RuntimeBullets
               .Where(b => b.Active)
               .Select(b => b.ToBulletState())
               .ToList();

    public IReadOnlyList<ObstacleDefinition> Obstacles => _engine.Obstacles;
}
