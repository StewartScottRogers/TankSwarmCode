# Chapter 1: Project Overview

[← Table of Contents](TOC.md) | [Next: Getting Started →](ch02-getting-started.md)

---

## What is TankSwarmCode?

TankSwarmCode is a **real-time 2D tank battle simulator** built with C# and .NET 10. It takes inspiration from the classic Robocode programming game and extends the concept with team-based swarm tactics, shared radar intelligence, electronic counter-measures, and a dynamic obstacle environment.

The simulator is intended as a platform for:

- **Game AI research** — experimenting with steering behaviours, target prioritisation, and cooperative strategies
- **Swarm robotics study** — simulating decentralised coordination through broadcast messaging
- **Competitive programming** — implementing and tuning AI combatants to beat other swarms
- **Learning event-driven architecture** — the framework is a practical example of clean interface separation and parallel simulation

---

## Core Concepts

### The Arena

The arena is a bounded rectangular field (default 800 × 600 px). Each round, randomly generated rectangular buildings are placed throughout the arena. Buildings are solid obstacles: they block tank movement, stop bullets, and occlude radar line-of-sight.

### Tanks

Each tank has:

- **Body** — moves forward/backward, turns at up to 10°/tick (slower at higher speeds)
- **Gun** — mounted on the body, rotates independently at up to 20°/tick
- **Radar** — mounted on the gun, rotates independently at up to 45°/tick
- **Energy** — starts at 100; depleted by firing, collisions, wall contact, and ECM costs; reaches 0 = destroyed

### Swarms

Tanks with the same `SwarmId` are allies. They broadcast messages to each other (automatically received before the next tick) and share their radar contacts. A swarm wins the round when it is the last group with living members.

### Bullets

Bullets are fired with a power value in `[0.1, 3.0]`. Higher power means slower projectiles but more damage. The shooter recovers a portion of the firing cost when a bullet connects with an enemy.

### Electronic Counter-Measures (ECM)

Any tank can activate an ECM mode each tick at an energy cost. ECM modes interfere with the radar pipeline — jamming corrupts or drops enemy scans, spoofing injects phantom contacts into enemy RadarMaps, and burnthrough pierces enemy interference. See [Chapter 13: ECM System](ch13-ecm-system.md).

---

## Key Features

| Feature | Description |
|---------|-------------|
| Robocode physics | Velocity-dependent turning, asymmetric acceleration/deceleration |
| Team swarms | Tanks share a `SwarmId`; allies coordinate via structured messages |
| Shared radar picture | Every radar scan is automatically broadcast to allies via `RadarShare` |
| Dynamic buildings | Random rectangular obstacles block movement, bullets, and radar |
| Event-driven AI | Override virtual methods: `OnTick`, `OnScannedTank`, `OnPainted`, `OnHitByBullet`, etc. |
| ECM warfare | Jam, Spoof, JamAndSpoof, and Burnthrough modes with energy costs and probabilistic effects |
| Bullet ricochet | Non-lethal hits deflect bullets instead of absorbing them; sparks fade visually |
| Parallel simulation | Tick callbacks, bullet movement, radar scans, and state sync run in parallel |
| GDI+ rendering | Animated radar sweeps, ECM auras, ghost echoes, explosion sequences, energy bars |
| Interactive UI | Click tanks to inspect state; force ECM modes via UI; step-by-step debugging |
| Headless CLI runner | Run matches without a GUI; stream JSON/NDJSON/table/CSV; parallel batches for AI training |

---

## Solution at a Glance

```
TankSwarmCode.slnx
├── TankSwarmCode.SwarmTank.Interfaces   — public API (contracts, models, events, enums)
├── TankSwarmCode.Arena.Interfaces       — arena control contract (IArena)
├── TankSwarmCode.SwarmTank              — SwarmTankBase abstract class
├── TankSwarmCode.Arena                  — ArenaEngine physics & runtime state
├── TankSwarmCode.SwarmTanks.Red         — Red Swarm AI (6 tanks)
├── TankSwarmCode.SwarmTanks.Blue        — Blue Swarm AI (7 tanks)
├── TankSwarmCode.Gui                    — WinForms host, renderer, main form
├── TankSwarmCode.Cli                    — headless JSON/NDJSON/table/CSV match runner
└── Documentation                        — this documentation
```

See [Chapter 3: Architecture Overview](ch03-architecture.md) for how these layers interact.

---

[← Table of Contents](TOC.md) | [Next: Getting Started →](ch02-getting-started.md)
