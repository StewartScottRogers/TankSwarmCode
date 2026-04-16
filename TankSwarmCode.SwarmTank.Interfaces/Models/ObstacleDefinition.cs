namespace TankSwarmCode.SwarmTank.Interfaces.Models;

/// <summary>
/// An axis-aligned rectangular obstacle randomly placed in the arena each round.
/// Tanks cannot pass through, bullets are destroyed on contact, and radar cannot
/// see through obstacles.
/// </summary>
/// <param name="X">Left edge in pixels (arena coordinate space).</param>
/// <param name="Y">Top edge in pixels (arena coordinate space).</param>
/// <param name="Width">Width in pixels.</param>
/// <param name="Height">Height in pixels.</param>
public record ObstacleDefinition(double X, double Y, double Width, double Height);
