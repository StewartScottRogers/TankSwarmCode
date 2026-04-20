namespace TankSwarmCode.SwarmTank.Models;

/// <summary>A single wall face detected by a radar echo — two world-space endpoints.</summary>
public record WallSegment(Vector2D Start, Vector2D End);
