# Chapter 3: Architecture Overview

[← Getting Started](ch02-getting-started.md) | [Table of Contents](TOC.md) | [Next: Physics Engine →](ch04-physics-engine.md)

---

## Design Philosophy

TankSwarmCode is split into narrow, purpose-built projects that keep the public API separate from the implementation:

- **AI authors** only reference `TankSwarmCode.SwarmTank` — they never touch the engine or renderer.
- **Engine changes** are isolated to `TankSwarmCode.Arena` and do not require AI code to be recompiled.
- **The renderer** is a separate WinForms layer that consumes read-only snapshots, so it cannot mutate simulation state.

---

## Project Dependency Graph

```
TankSwarmCode.Gui (WinForms host / renderer)
    │
    ├── TankSwarmCode.Arena
    │       ├── TankSwarmCode.Arena.Interfaces
    │       │       └── TankSwarmCode.SwarmTank
    │       └── TankSwarmCode.SwarmTank
    │
    ├── TankSwarmCode.SwarmTank
    │
    ├── TankSwarmCode.SwarmTanks.Red
    │       └── TankSwarmCode.SwarmTank
    │
    └── TankSwarmCode.SwarmTanks.Blue
            └── TankSwarmCode.SwarmTank

TankSwarmCode.Cli (headless runner)
    │
    ├── TankSwarmCode.Arena
    │       ├── TankSwarmCode.Arena.Interfaces
    │       │       └── TankSwarmCode.SwarmTank
    │       └── TankSwarmCode.SwarmTank
    └── TankSwarmCode.SwarmTank
```

Neither the Red nor Blue swarm projects reference the Arena project — they cannot call engine internals. The CLI loads swarm DLLs at runtime via reflection; it does not reference them at compile time.

---

## Layered Architecture

### Layer 1 — Public Interfaces & Models (`TankSwarmCode.SwarmTank`)

Everything AI authors see lives here:

- `ISwarmTank` — the tank contract the engine calls
- `IArenaContext` — read-only arena view (`ArenaWidth`, `ArenaHeight`, `TickNumber`, `LivingTankCount`, `Buildings`, `GetActiveBullets()`, `GetSwarmSize()`)
- `ArenaConstants` — all physics constants (`MaxVelocity`, turn rates, damage parameters, ECM costs)
- `Models/` — immutable data snapshots: `TankState`, `TankCommand`, `RadarContact`, `ScanResult`, `SwarmMessage`, `BulletState`, `BuildingDefinition`, `Vector2D`
- `Events/` — event argument types for every lifecycle hook
- `Enums/` — `TankRole`, `EcmMode`, `SwarmMessageType`

### Layer 2 — Tank Base Classes (`TankSwarmCode.SwarmTank`)

**`SwarmTankBase`** implements `ISwarmTank` and exposes the friendly AI API:

- Fluent setters: `SetAhead`, `SetBack`, `SetTurnRight`, `SetTurnLeft`, `SetTurnGunRight`, `SetTurnGunLeft`, `SetTurnRadarRight`, `SetTurnRadarLeft`, `SetFire`, `SetEcm`
- Maintained `RadarMap` (own scans merged with ally `RadarShare` broadcasts)
- `Broadcast()` helper to queue swarm messages
- Radar helpers: `GetFreshestEnemy()`, `GetFreshestContact()`
- Default no-op implementations of every virtual lifecycle method

**`SwarmTankCortexCradleBase`** extends `SwarmTankBase` with a full team-coordination brain used by all built-in tanks:

- Slot-based leader election (lowest `FormationSlot` among living allies leads each 40-tick epoch)
- Epoch strategy selection: Wolfpack, Encircle, Pincer, ECMScreen, Fallback, Scatter
- Coordinated volley scheduling via `VolleyFire` broadcast messages
- Ally health tracking via `AllyPing` heartbeats every 15 ticks
- Automatic ECM mode switching based on strategy and received `EcmAlert` messages
- Configured via `TankConfiguration` — subclasses only need to provide a config record

### Layer 3 — Physics Engine (`TankSwarmCode.Arena`)

`ArenaEngine` owns all mutable runtime state and drives the tick loop:

- `TankRuntimeState` — wraps an `ISwarmTank` with mutable position, velocity, energy, heading, ECM state, ghost positions, and pending command
- `BulletRuntimeState` — active bullet position, owner, and ricochet state
- `ArenaEngine` — the simulation loop (detailed in [Chapter 4](ch04-physics-engine.md))
- `ArenaContext` — snapshot-backed implementation of `IArenaContext`

### Layer 4 — Host Applications

**`TankSwarmCode.Gui` (WinForms host)**

- `TankSwarmArena` (main form) — menus, speed slider, start/stop/step/reset buttons, radio log panel
- `ArenaUserControl` — GDI+ double-buffered renderer; owns the `ArenaEngine` instance; drives the `System.Windows.Forms.Timer`-based tick loop; tracks per-tank damage/energy stats
- `ArenaConfigurationUserControl` — arena dimension configuration form
- `ScreenWakeLock` — prevents Windows display sleep during simulation

**`TankSwarmCode.Cli` (headless runner)**

- Single `Program.cs` entry point; no GUI dependency
- Loads swarm DLLs at runtime via `Assembly.LoadFrom` + reflection
- Runs one match or a parallel batch (`Parallel.For`); streams results as JSON, NDJSON, table, or CSV
- See [Chapter 15: Headless CLI Runner](ch15-cli.md) for full documentation

---

## Runtime Data Flow

```
Timer fires
    │
    ▼
ArenaUserControl.Tick()
    │
    ▼
ArenaEngine.Tick()          ← one simulation step
    │
    ├─ OnTick() (parallel)              → each tank writes its TankCommand
    ├─ FlushCommands() (sequential)     → commands collected
    ├─ ApplyMovement() (sequential)     → positions updated; wall & building collisions
    ├─ ApplyFiring() (sequential)       → new BulletRuntimeStates created
    ├─ ApplyEcm() (sequential)          → energy drained; ActiveEcm set; ghost positions updated
    ├─ MoveBullets() (parallel)         → bullet positions advanced; deflected bullets decay
    ├─ CheckBulletTankCollisions()      → energy deltas; lethal hits remove bullet; non-lethal deflect
    ├─ CheckTankTankCollisions()        → separation push; mutual 0.6 damage
    ├─ ProcessRadarScans() (parallel)   → ScannedTank events; ECM drop/corrupt/ghost injection; Painted events
    ├─ DeliverSwarmMessages()           → broadcasts routed to living, non-jamming allies
    ├─ UpdateTankStates() (parallel)    → fresh immutable TankState pushed to each ISwarmTank
    ├─ RebuildSnapshots()               → spent bullets removed; RicochetFlashes & ghost echoes published
    └─ CheckRoundEnd()                  → fires OnRoundEnded if one swarm remains
    │
    ▼
ArenaUserControl.Invalidate()   ← triggers a repaint
    │
    ▼
ArenaUserControl.OnPaint()      → reads immutable TankState/BulletState snapshots; renders frame
```

The renderer **never writes** to simulation state. All rendering is driven from immutable snapshots published at the end of the last tick.

---

## Thread Safety Model

| Phase | Execution |
|-------|-----------|
| `OnTick` callbacks | `Parallel.ForEach` — each tank writes only its own `TankCommand` |
| Movement, firing, ECM | Sequential — avoids concurrent energy/position mutation |
| Bullet movement | `Parallel.ForEach` — each bullet is independent |
| Collision resolution | Sequential — avoids concurrent energy mutation |
| Radar scans | `Parallel.ForEach` — each tank's scan is independent |
| Message delivery | Sequential — merges broadcasts into ally `RadarMap` |
| State sync | `Parallel.ForEach` — each tank receives its own fresh immutable snapshot |
| Rendering | UI thread reads immutable snapshots; no lock required |

---

[← Getting Started](ch02-getting-started.md) | [Table of Contents](TOC.md) | [Next: Physics Engine →](ch04-physics-engine.md)
