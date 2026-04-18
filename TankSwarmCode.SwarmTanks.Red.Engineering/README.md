# Red Swarm Engineering

This shared project is the working area for building and tuning the Red Swarm AI. It contains no compiled code — add your implementation files to the main `TankSwarmCode.SwarmTanks.Red` class library and reference this project for shared resources.

---

## Quick Start

### 1. Choose a Base Class

All current Red tanks use `SwarmBrainBase` for the full coordination brain. To add a new tank with identical behaviour, subclass `SwarmBrainBase` and provide a `TankConfig`:

```csharp
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Models;

public sealed class RedViper : SwarmBrainBase
{
    public RedViper() { SwarmId = 1; Role = TankRole.Attacker; }
    public override string Name => "RedViper";
    protected override TankConfig Config { get; } = new()
    {
        FormationSlot = 5,       // higher slot = lower leadership priority
        MaxFirePower = 2.0,
        PreferredRange = 190.0,
        HasEcm = false,
        RetreatEnergyThreshold = 22.0
    };
}
```

For full AI control (no shared brain), subclass `SwarmTankBase` directly — see [ch10 — Building Your Own Tank](../Documentation/ch10-custom-tank.md).

### 2. Register in the GUI

In `TankSwarmArena.cs`, call `arenaUserControl1.AddTank(new RedViper())` inside one of the existing Red Swarm click handlers, or wire it to a new menu item.

### 3. Test Headless

```bash
dotnet build TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release

dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 100 --parallel 8 --format table
```

---

## Key API

| Method | Purpose |
|--------|---------|
| `SetAhead(dist)` / `SetBack(dist)` | Queue forward/backward movement |
| `SetTurnRight(deg)` / `SetTurnLeft(deg)` | Queue body turn |
| `SetTurnGunRight(deg)` / `SetTurnGunLeft(deg)` | Queue gun turn |
| `SetTurnRadarRight(deg)` / `SetTurnRadarLeft(deg)` | Queue radar turn |
| `SetFire(power)` | Fire bullet (power 0.1–3.0; higher = slower, more damage) |
| `SetEcm(EcmMode)` | Activate ECM for this tick (Jam / Spoof / JamAndSpoof / Burnthrough) |
| `Broadcast(msg)` | Send a typed message to all allies |
| `NormalizeAngle(a)` | Clamp angle to ±180° — use before every turn command |

**State properties** (read in `OnTick`): `State.Position`, `State.Heading`, `State.GunHeading`, `State.RadarHeading`, `State.Energy`, `State.Velocity`

**Arena properties**: `Arena.ArenaWidth`, `Arena.ArenaHeight`, `Arena.TickNumber`

**Radar**: `RadarMap` — dictionary of known enemy contacts (updated by direct scans and ally `RadarShare` messages). Always check `contact.Timestamp` — discard contacts older than ~5 ticks.

---

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Not normalising angle differences | Wrap with `NormalizeAngle()` before every `SetTurnGunRight` / `SetTurnRight` |
| Using `Arena.Width` / `Arena.Height` | Use `Arena.ArenaWidth` / `Arena.ArenaHeight` |
| Skipping `base.OnScannedTank(e)` | `RadarMap` never updates; no `RadarShare` to allies |
| Skipping `base.OnSwarmMessage(e)` | Ally radar contacts are never merged |
| Firing every tick regardless of alignment | Gate `SetFire` on gun alignment < 8° to avoid wasting energy |

---

## Documentation

| Chapter | Topic |
|---------|-------|
| [ch01 — Overview](../Documentation/ch01-overview.md) | What TankSwarmCode is |
| [ch05 — Tank AI Framework](../Documentation/ch05-tank-ai-framework.md) | Full lifecycle and event reference |
| [ch06 — Swarm Communication](../Documentation/ch06-swarm-communication.md) | Messaging and RadarShare |
| [ch07 — Radar System](../Documentation/ch07-radar-system.md) | RadarMap, contacts, staleness |
| [ch09 — Built-in Tanks](../Documentation/ch09-builtin-tanks.md) | Existing Red and Blue AI as reference |
| [ch10 — Building Your Own Tank](../Documentation/ch10-custom-tank.md) | Full walkthrough with code examples |
| [ch13 — ECM System](../Documentation/ch13-ecm-system.md) | Jam, Spoof, Burnthrough |
| [ch15 — Headless CLI](../Documentation/ch15-cli.md) | Batch runs, output formats, training workflows |
