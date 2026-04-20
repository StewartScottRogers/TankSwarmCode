namespace TankSwarmCode.SwarmTank.Interfaces.Models;

/// <summary>
/// Radar echo returned when a sweep hits a building. Contains only the wall faces
/// visible from the scanner — not the full building rectangle.
/// Shared automatically via <see cref="Enums.SwarmMessageType.BuildingEchoShare"/>.
/// </summary>
public record BuildingEcho
{
    /// <summary>The 1–2 wall faces that reflected the radar pulse toward the scanner.</summary>
    public IReadOnlyList<WallSegment> Walls { get; init; } = [];

    /// <summary>Bearing relative to the scanner's heading, in degrees. Negative = left, positive = right.</summary>
    public double Bearing { get; init; }

    /// <summary>Distance from the scanner to the nearest point on the reflecting wall, in pixels.</summary>
    public double Distance { get; init; }

    /// <summary>World-space position of the nearest hit point on the building surface.</summary>
    public Vector2D NearestPoint { get; init; }

    /// <summary>Name of the tank whose radar made the original scan.</summary>
    public string ScannedBy { get; init; } = string.Empty;

    /// <summary>Arena tick when this echo was recorded.</summary>
    public long Timestamp { get; init; }
}
