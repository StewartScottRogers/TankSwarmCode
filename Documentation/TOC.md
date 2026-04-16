# TankSwarmCode Documentation

A Robocode-inspired 2D tank battle simulator built with C# and .NET 10.

---

## Table of Contents

| Chapter | Title | Description |
|---------|-------|-------------|
| [01](ch01-overview.md) | [Project Overview](ch01-overview.md) | What TankSwarmCode is, its goals, and key capabilities |
| [02](ch02-getting-started.md) | [Getting Started](ch02-getting-started.md) | Building, running, and navigating the application |
| [03](ch03-architecture.md) | [Architecture Overview](ch03-architecture.md) | Solution structure, project layers, and how subsystems connect |
| [04](ch04-physics-engine.md) | [Physics Engine](ch04-physics-engine.md) | Tick cycle, movement model, collision detection, bullet physics |
| [05](ch05-tank-ai-framework.md) | [Tank AI Framework](ch05-tank-ai-framework.md) | `SwarmTankBase` API, lifecycle hooks, and command model |
| [06](ch06-swarm-communication.md) | [Swarm Communication](ch06-swarm-communication.md) | Message types, broadcasting, and shared radar picture |
| [07](ch07-radar-system.md) | [Radar System](ch07-radar-system.md) | Arc sweep mechanics, line-of-sight blocking, and bearing math |
| [08](ch08-data-models.md) | [Data Models Reference](ch08-data-models.md) | All core types: `TankState`, `TankCommand`, `RadarContact`, etc. |
| [09](ch09-builtin-tanks.md) | [Built-in Tank AI Examples](ch09-builtin-tanks.md) | Red Swarm and Blue Swarm strategies dissected |
| [10](ch10-custom-tank.md) | [Building Your Own Tank](ch10-custom-tank.md) | Step-by-step guide to writing a custom AI tank |
| [11](ch11-rendering.md) | [Arena Rendering & UI](ch11-rendering.md) | GDI+ renderer, interactive features, and the main form |
| [12](ch12-configuration.md) | [Configuration & Constants](ch12-configuration.md) | All physics constants, rendering parameters, and tunable values |
| [13](ch13-ecm-system.md) | [ECM System](ch13-ecm-system.md) | Electronic Counter-Measures: jamming, spoofing, burnthrough, and the built-in ECM tanks |
