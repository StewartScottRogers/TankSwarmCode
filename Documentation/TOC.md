# TankSwarmCode Documentation

A Robocode-inspired 2D tank battle simulator built with C# and .NET 10.

---

## Reading Paths

**Building a custom tank AI**
→ [ch02 Getting Started](ch02-getting-started.md) → [ch05 Tank AI Framework](ch05-tank-ai-framework.md) → [ch06 Swarm Communication](ch06-swarm-communication.md) → [ch10 Custom Tank](ch10-custom-tank.md)

**Running Karpathy Loop research**
→ [ch00 Quick Reference](ch00-quick-reference.md) → [ch15 CLI Runner](ch15-cli.md) → [AutoResearch.md](../AutoResearch.md)

**Understanding the system architecture**
→ [ch01 Overview](ch01-overview.md) → [ch03 Architecture](ch03-architecture.md) → [ch04 Physics](ch04-physics-engine.md) → [ch09 Built-in Tanks](ch09-builtin-tanks.md)

**Looking up a specific system**
→ [ch00 Quick Reference](ch00-quick-reference.md) covers API, CLI flags, ECM modes, insight thresholds, and config constants on one page

---

## Table of Contents

| Chapter | Title | Description |
|---------|-------|-------------|
| [00](ch00-quick-reference.md) | [Quick Reference](ch00-quick-reference.md) | CLI flags, insight labels, ECM modes, API, config constants — all on one page |
| [01](ch01-overview.md) | [Project Overview](ch01-overview.md) | What TankSwarmCode is, its goals, and key capabilities |
| [02](ch02-getting-started.md) | [Getting Started](ch02-getting-started.md) | Building, running, and navigating the application |
| [03](ch03-architecture.md) | [Architecture Overview](ch03-architecture.md) | Solution structure, project layers, and how subsystems connect |
| [04](ch04-physics-engine.md) | [Physics Engine](ch04-physics-engine.md) | Tick cycle, movement model, collision detection, bullet physics |
| [05](ch05-tank-ai-framework.md) | [Tank AI Framework](ch05-tank-ai-framework.md) | `SwarmTankBase` API, lifecycle hooks, and command model |
| [06](ch06-swarm-communication.md) | [Swarm Communication](ch06-swarm-communication.md) | Message types, broadcasting, and shared radar picture |
| [07](ch07-radar-system.md) | [Radar System](ch07-radar-system.md) | Arc sweep mechanics, line-of-sight blocking, and bearing math |
| [08](ch08-data-models.md) | [Data Models Reference](ch08-data-models.md) | All core types: `TankState`, `TankCommand`, `RadarContact`, `IArenaContext`, enums |
| [09](ch09-builtin-tanks.md) | [Built-in Tank AI Examples](ch09-builtin-tanks.md) | Red Swarm (6 tanks) and Blue Swarm (7 tanks) strategies dissected |
| [10](ch10-custom-tank.md) | [Building Your Own Tank](ch10-custom-tank.md) | Step-by-step guide to writing a custom AI tank |
| [11](ch11-rendering.md) | [Arena Rendering & UI](ch11-rendering.md) | GDI+ renderer, ECM auras, interactive features, and the main form |
| [12](ch12-configuration.md) | [Configuration & Constants](ch12-configuration.md) | All physics constants, ECM constants, and rendering parameters |
| [13](ch13-ecm-system.md) | [ECM System](ch13-ecm-system.md) | Electronic Counter-Measures: jamming, spoofing, burnthrough, and the built-in ECM tanks |
| [14](ch14-tank-energy.md) | [Tank Energy](ch14-tank-energy.md) | Energy as a resource: starting value, all drains and gains, death condition, and power/speed trade-offs |
| [15](ch15-cli.md) | [Headless CLI Runner](ch15-cli.md) | `TankSwarmCode.Cli`: flags, output formats (JSON/NDJSON/table/CSV), parallel batches, and the `--list` discovery command |
