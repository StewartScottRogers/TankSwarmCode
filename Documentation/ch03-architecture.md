# Chapter 3: Architecture Overview

[← Getting Started](ch02-getting-started.md) | [Table of Contents](TOC.md) | [Next: Physics Engine →](ch04-physics-engine.md)

---

## Design Philosophy

TankSwarmCode is split into narrow, purpose-built projects that keep the public API separate from the implementation. This means:

- **AI authors** only need to reference `TankSwarmCode.SwarmTank` and `TankSwarmCode.SwarmTank.Interfaces` — they never touch the engine or the renderer.
- **Engine changes** are isolated to `TankSwarmCode.Arena` and don't require AI code to be recompiled.
- **The renderer** is a separate WinForms layer that consumes read-only snapshots, so it cannot mutate simulation state.

---

## Project Dependency Graph

```
TankSwarmCode (WinForms host / renderer)
    │
    ├── TankSwarmCode.Arena
    │       │
    │       └── TankSwarmCode.Arena.Interfaces
    │       └── TankSwarmCode.SwarmTank.Interfaces
    │
    ├── TankSwarmCode.SwarmTank
    │       │
    │       └── TankSwarmCode.SwarmTank.Interfaces
    │
    ├── TankSwarmCode.SwarmTanks.Red
    │       └── TankSwarmCode.SwarmTank
    │
    └── TankSwarmCode.SwarmTanks.Blue
            └── TankSwarmCode.SwarmTank
```

Neither the Red nor Blue swarm projects reference the Arena project — they cannot call engine internals.

---

## Layered Architecture

### Layer 1 — Public Interfaces & Models (`TankSwarmCode.SwarmTank.Interfaces`)

Everything AI authors see lives here:

- `ISwarmTank` — the tank contract the engine calls
- `IArenaContext` — read-only arena view (dimensions, tank list, buildings, bullets)
- `ArenaConstants` — physics constants (max velocity, turn rates, damage formulas)
- `Models/` — immutable data snapshots: `TankState`, `TankCommand`, `RadarContact`, `SwarmMessage`, `BulletState`, `BuildingDefinition`, `Vector2D`
- `Events/` — event argument types for every lifecycle hook
- `Enums/` — `TankRole`, `SwarmMessageType`

### Layer 2 — Tank Base Class (`TankSwarmCode.SwarmTank`)

`SwarmTankBase` implements `ISwarmTank` and exposes the friendly API:

- Fluent setters (`SetAhead`, `SetFire`, `SetTurnGunRight`, …)
- Maintained `RadarMap` (own scans merged with ally broadcasts)
- `Broadcast()` helper to queue swarm messages
- Default no-op implementations of every virtual lifecycle method

### Layer 3 — Physics Engine (`TankSwarmCode.Arena`)

`ArenaEngine` owns the mutable runtime state and drives the tick loop:

- `TankRuntimeState` — wraps an `ISwarmTank` with mutable position, velocity, energy, heading, pending command, and buffered messages
- `BulletRuntimeState` — active bullet position and owner
- `ArenaEngine` — the main loop (detailed in [Chapter 4](ch04-physics-engine.md))
- `ArenaContext` — snapshot-backed implementation of `IArenaContext`

### Layer 4 — WinForms Host (`TankSwarmCode`)

- `TankSwarmArena` (main form) — menus, speed slider, start/stop/step/reset buttons, radio log panel
- `ArenaUserControl` — GDI+ double-buffered renderer, owns the `ArenaEngine` instance, drives the `System.Windows.Forms.Timer`-based tick loop at the configured rate
- `ArenaConfigurationUserControl` — arena dimension form

---

## Runtime Data Flow

```
Timer fires
    │
    ▼
ArenaUserControl.Tick()
    │
    ▼
ArenaEngine.Tick()      ← one simulation step
    │
    ├─ OnTick() for each tank (parallel)   → TankCommand written
    ├─ FlushCommands() (sequential)         → commands collected
    ├─ ApplyMovement() (sequential)         → positions updated, wall/building collisions
    ├─ ApplyFiring() (sequential)           → new BulletRuntimeStates created
    ├─ MoveBullets() (parallel)             → bullet positions advanced
    ├─ CheckBulletTankCollisions()          → energy deltas, deactivations
    ├─ CheckTankTankCollisions()            → separation push, mutual damage
    ├─ ProcessRadarScans() (parallel)       → ScannedTank events, RadarContact updates
    ├─ DeliverSwarmMessages() (sequential)  → broadcasts merged into allies' RadarMaps
    ├─ UpdateTankStates() (parallel)        → fresh TankState pushed to each ISwarmTank
    ├─ RebuildSnapshots()                   → spent bullets removed, cached lists rebuilt
    └─ CheckRoundEnd()                      → fires OnRoundEnded if appropriate
    │
    ▼
ArenaUserControl.Invalidate()   ← triggers a repaint
    │
    ▼
ArenaUserControl.OnPaint()      → reads immutable TankState/BulletState snapshots and renders
```

The renderer **never writes** to simulation state. All rendering is driven from the immutable snapshots that were published at the end of the last tick.

---

## Thread Safety Model

| Phase | Execution |
|-------|-----------|
| `OnTick` callbacks | `Parallel.ForEach` — each tank writes only its own `TankCommand` |
| Collision resolution | Sequential — avoids concurrent energy mutation |
| Radar scans | `Parallel.ForEach` — each tank's scan is independent |
| Message delivery | Sequential — merges broadcasts into ally `RadarMap` |
| State sync (`TankState` push) | `Parallel.ForEach` — each tank gets a fresh immutable snapshot |
| Rendering | UI thread reads immutable snapshots; no lock required |

---

[← Getting Started](ch02-getting-started.md) | [Table of Contents](TOC.md) | [Next: Physics Engine →](ch04-physics-engine.md)
