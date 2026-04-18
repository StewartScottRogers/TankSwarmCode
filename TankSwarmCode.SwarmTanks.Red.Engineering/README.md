# Red Swarm Engineering

This shared project is the working area for building and tuning the Red Swarm AI. It contains no compiled code — add your implementation files to the main `TankSwarmCode.SwarmTanks.Red` class library and reference this project for shared resources.

---

## Quick Start

### 1. Subclass SwarmTankBase

Every tank inherits from `SwarmTankBase`. Reference the base library from your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\TankSwarmCode.SwarmTank\TankSwarmCode.SwarmTank.csproj" />
</ItemGroup>
```

Minimal tank skeleton:

```csharp
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Interfaces.Enums;
using TankSwarmCode.SwarmTank.Interfaces.Events;

public class RedScout : SwarmTankBase
{
    public RedScout() { SwarmId = 1; Role = TankRole.Scout; }
    public override string Name => "RedScout";

    public override void OnStart()      => SetTurnRadarRight(double.MaxValue);
    public override void OnTick(TickEventArgs e) { /* AI here */ }
}
```

### 2. Register in the GUI

In `TankSwarmArena.cs`, add a method and wire it to the **Add Tanks** menu:

```csharp
private void AddRedSwarm()
{
    _engine.AddTank(new RedScout());
    _engine.AddTank(new RedAttacker("RedAlpha"));
    // ...
}
```

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
| `BroadcastMessage(msg)` | Send a typed message to all allies |
| `NormalizeAngle(a)` | Clamp angle to ±180° — use before every turn command |

**State properties** (read in `OnTick`): `State.Position`, `State.Heading`, `State.GunHeading`, `State.RadarHeading`, `State.Energy`, `State.Velocity`

**Arena properties**: `Arena.ArenaWidth`, `Arena.ArenaHeight`, `Arena.TickNumber`

**Radar**: `State.RadarMap` — dictionary of known enemy contacts (updated by direct scans and ally `RadarShare` messages). Always check `contact.Timestamp` — discard contacts older than ~5 ticks.

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
