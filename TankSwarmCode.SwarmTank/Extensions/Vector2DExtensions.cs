using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank.Extensions;

public static class Vector2DExtensions
{
    public static Vector2D PolarOffset(this Vector2D center, double angleDeg, double radius)
    {
        double rad = angleDeg * Math.PI / 180.0;
        return new Vector2D(
            center.X + radius * Math.Sin(rad),
            center.Y - radius * Math.Cos(rad));
    }

    public static double RelativeBearing(this double angle)
    {
        while (angle > 180) angle -= 360;
        while (angle < -180) angle += 360;
        return angle;
    }
}
