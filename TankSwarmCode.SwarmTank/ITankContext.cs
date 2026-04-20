using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTank;

public interface ITankContext
{
    string Name { get; }
    int SwarmId { get; }
    TankRole Role { get; set; }
    TankState State { get; }
    IArenaContext Arena { get; }
    IReadOnlyDictionary<string, RadarContact> RadarMap { get; }
    IReadOnlyDictionary<string, BuildingEcho> BuildingWallMap { get; }

    void SetAhead(double distance);
    void SetBack(double distance);
    void SetTurnRight(double degrees);
    void SetTurnLeft(double degrees);
    void SetTurnGunRight(double degrees);
    void SetTurnGunLeft(double degrees);
    void SetTurnRadarRight(double degrees);
    void SetTurnRadarLeft(double degrees);
    void SetFire(double power);
    void SetEcm(EcmMode mode);
    void Broadcast(SwarmMessage message);
}
