namespace TankSwarmCode.SwarmTank.Models;

/// <summary>Immutable 2-D position or direction vector using double precision.</summary>
public readonly record struct Vector2D(double X, double Y)
{
    public static Vector2D Zero => new(0, 0);

    public double Length => Math.Sqrt(X * X + Y * Y);

    /// <summary>Returns the distance to <paramref name="other"/>.</summary>
    public double DistanceTo(Vector2D other) =>
        Math.Sqrt(Math.Pow(other.X - X, 2) + Math.Pow(other.Y - Y, 2));

    /// <summary>
    /// Returns the absolute bearing in degrees (0 = north / up, clockwise)
    /// from this position to <paramref name="other"/>.
    /// </summary>
    public double BearingTo(Vector2D other)
    {
        double dx = other.X - X;
        double dy = other.Y - Y;
        double angle = Math.Atan2(dx, -dy) * (180.0 / Math.PI);
        return (angle + 360) % 360;
    }

    public override string ToString() => $"({X:F1}, {Y:F1})";
}
