# Chapter 1: Project Overview

[← Table of Contents](TOC.md) | [Next: Getting Started →](ch02-getting-started.md)

---

## What is TankSwarmCode?

TankSwarmCode is a **real-time 2D tank battle simulator** built with C# and .NET 10. It takes inspiration from the classic Robocode programming game and extends the concept with team-based swarm tactics, shared radar intelligence, and a dynamic obstacle environment.

The simulator is intended as a platform for:

- **Game AI research** — experimenting with steering behaviors, target prioritization, and cooperative strategies
- **Swarm robotics study** — simulating decentralized coordination through broadcast messaging
- **Competitive programming** — implementing and tuning AI combatants to beat other swarms
- **Learning event-driven architecture** — the framework itself is a practical example of clean interface separation and parallel simulation

---

## Core Concepts

### The Arena

The arena is a bounded rectangular field (default 800 × 600 px). Each round, 4–14 randomly generated rectangular buildings are placed throughout the arena. Buildings are solid obstacles: they block tank movement, stop bullets, and occlude radar.

### Tanks

Each tank has:

- **Body** — moves forward/backward, turns at up to 10°/tick (faster at lower speeds)
- **Gun** — mounted on the body, rotates independently at up to 20°/tick
- **Radar** — mounted on the gun, rotates independently at up to 45°/tick
- **Energy** — starts at 100; depleted by firing, collisions, and wall contact; reaches 0 = destroyed

### Swarms

Tanks with the same `SwarmId` are allies. They can broadcast messages to each other (automatically received before the next tick) and share their radar contacts. A swarm wins the round when it is the last group with living members.

### Bullets

Bullets are fired with a power value in `[0.1, 3.0]`. Higher power means slower projectiles but more damage. The shooter recovers a portion of the firing cost when a bullet hits an enemy.

---

## Key Features

| Feature | Description |
|---------|-------------|
| Robocode physics | Velocity-dependent turning, acceleration/deceleration model |
| Team swarms | Tanks share a `SwarmId`; allies coordinate via messages |
| Shared radar picture | Every radar scan is automatically broadcast to allies |
| Dynamic buildings | Random rectangular obstacles block movement, bullets, and radar |
| Event-driven AI | Override virtual methods: `OnTick`, `OnScannedTank`, `OnHitByBullet`, etc. |
| Parallel simulation | Tick callbacks, bullet movement, radar, and state sync run in parallel |
| GDI+ rendering | Animated radar sweeps, explosion sequences, energy bars, message log |
| Interactive UI | Click tanks to inspect state; step-by-step debugging; adjustable speed |

---

## Solution at a Glance

```
TankSwarmCode.slnx
├── TankSwarmCode.SwarmTank.Interfaces   — public API (contracts, models, events, enums)
├── TankSwarmCode.Arena.Interfaces       — arena control contract
├── TankSwarmCode.SwarmTank              — SwarmTankBase abstract class
├── TankSwarmCode.Arena                  — ArenaEngine physics & runtime state
├── TankSwarmCode.SwarmTanks.Red         — Red Swarm AI implementations
├── TankSwarmCode.SwarmTanks.Blue        — Blue Swarm AI implementations
├── TankSwarmCode                        — WinForms host, renderer, main form
└── Documentation                        — this documentation
```

See [Chapter 3: Architecture Overview](ch03-architecture.md) for a deeper look at how these layers interact.

---

[← Table of Contents](TOC.md) | [Next: Getting Started →](ch02-getting-started.md)
