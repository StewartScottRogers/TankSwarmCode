namespace TankSwarmCode.SwarmTank.Telemetry;

/// <summary>Full black-box recording for one match.</summary>
public record MatchTelemetry
{
    public Guid MatchId { get; init; }
    public int? Seed { get; init; }
    public long TotalTicks { get; init; }
    public int? WinnerSwarmId { get; init; }
    public double ArenaWidth { get; init; }
    public double ArenaHeight { get; init; }
    public IReadOnlyList<TankTickRecord> Records { get; init; } = [];
}
