using TankSwarmCode.SwarmTank.Events;
using TankSwarmCode.SwarmTank.Telemetry;

namespace TankSwarmCode.Arena;

/// <summary>
/// Attaches to an <see cref="ArenaEngine"/> and records a full tick-by-tick telemetry snapshot
/// for every tank. Call <see cref="Build"/> after the match ends to retrieve the recording.
/// </summary>
public sealed class BlackBoxRecorder
{
    private readonly ArenaEngine _engine;
    private readonly List<TankTickRecord> _records = [];

    public BlackBoxRecorder(ArenaEngine engine)
    {
        _engine = engine;
        engine.TickCompleted += OnTickCompleted;
    }

    private void OnTickCompleted(object? sender, TickEventArgs _)
    {
        long tick = _engine.TickNumber;
        foreach (TankRuntimeState rts in _engine.RuntimeTanks)
        {
            var cmd = rts.LastFlushedCommand;
            _records.Add(new TankTickRecord
            {
                Tick         = tick,
                TankName     = rts.Tank.Name,
                SwarmId      = rts.Tank.SwarmId,
                X            = rts.X,
                Y            = rts.Y,
                Heading      = rts.Heading,
                Velocity     = rts.Velocity,
                GunHeading   = rts.GunHeading,
                RadarHeading = rts.RadarHeading,
                Energy       = rts.Energy,
                IsAlive      = rts.IsAlive,
                ActiveEcm    = rts.ActiveEcm,
                CmdMove      = cmd.MoveDistance,
                CmdBodyTurn  = cmd.BodyTurnDegrees,
                CmdGunTurn   = cmd.GunTurnDegrees,
                CmdRadarTurn = cmd.RadarTurnDegrees,
                CmdFirePower = cmd.FirePower,
                CmdEcm       = cmd.EcmMode,
                RadarContacts = rts.Tank.RadarMap.Values.ToList(),
                Events        = rts.TickEvents.ToList(),
            });
            rts.TickEvents.Clear();
        }
    }

    public MatchTelemetry Build(int? seed, int? winnerSwarmId) => new()
    {
        MatchId       = Guid.NewGuid(),
        Seed          = seed,
        TotalTicks    = _engine.TickNumber,
        WinnerSwarmId = winnerSwarmId,
        ArenaWidth    = _engine.Width,
        ArenaHeight   = _engine.Height,
        Records       = _records.AsReadOnly(),
    };
}
