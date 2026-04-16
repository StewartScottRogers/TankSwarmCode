# Chapter 2: Getting Started

[← Overview](ch01-overview.md) | [Table of Contents](TOC.md) | [Next: Architecture →](ch03-architecture.md)

---

## Prerequisites

| Requirement | Minimum Version |
|-------------|----------------|
| .NET SDK | 10.0 |
| Windows | Windows 10 or later (WinForms host) |
| Visual Studio / Rider | Any version that supports .NET 10 and `.slnx` solution files |

---

## Building the Project

### From Visual Studio

1. Open `TankSwarmCode.slnx`.
2. Set the startup project to **TankSwarmCode** (the WinForms host).
3. Select **Debug** or **Release** configuration.
4. Press **F5** to build and run.

### From the Command Line

```bash
# Debug run
dotnet run --project TankSwarmCode/TankSwarmCode.csproj

# Release build (produces a single-file executable)
dotnet publish TankSwarmCode/TankSwarmCode.csproj -c Release
```

The Release publish target is configured to produce a self-contained, single-file `win-x64` executable. The output lands in the standard `publish/` directory under the project.

---

## Running the Simulator

When the application starts you will see an empty arena and a toolbar/menu across the top.

### Step 1 — Add Tanks

Use the **Add Tanks** menu to select one or both pre-built swarms:

- **Red Swarm** — adds RedScout, RedAlpha, RedBravo, RedWolf, RedFox (5 tanks)
- **Blue Swarm** — adds BlueCommander, BluePatrol × 2, BlueSniper × 2, BlueWarden (6 tanks)

You can add both swarms for a full Red vs Blue battle, or add the same swarm twice for same-team mirror matches.

### Step 2 — Configure the Arena (optional)

The **Arena Configuration** panel lets you set the width and height of the arena. Changes take effect at the next round start.

### Step 3 — Start the Round

Click **Start**. The engine will:

1. Randomly place buildings (4–14 scaled to arena area).
2. Spawn each tank at a random, collision-free position.
3. Call each tank's `OnStart()` hook.
4. Begin the tick loop.

### Step 4 — Watch and Interact

- **Click a tank** to attach the info panel and monitor its state.
- **Right-click a tank** to pin/unpin focus.
- **Double-click** to lock the camera on a tank.
- The **Radio Log** panel scrolls swarm messages in real time, colour-coded by swarm.

### Step 5 — End of Round

The round ends when only one swarm has living members. Each survivor receives `OnRoundEnded(Won: true)`; dead tanks receive `OnRoundEnded(Won: false)`.

Click **Reset** to clear the arena and prepare for a new round, or **Start** again to replay with the same tank list.

---

## Speed Controls

| Control | Effect |
|---------|--------|
| Ticks/sec slider | Sets the simulation rate (1 to unlimited) |
| **Stop** button | Pauses the simulation without resetting |
| **Step** button | Advances exactly one tick — useful for debugging AI logic |

---

## Project Layout

```
TankSwarmCode/                    WinForms host
  Program.cs                      Entry point
  TankSwarmArena.cs               Main form (menus, controls, radio log)
  ArenaUserControl.cs             GDI+ renderer + engine host
  ArenaConfigurationUserControl   Arena dimension settings panel

TankSwarmCode.Arena/
  ArenaEngine.cs                  Tick loop, physics, collision detection
  TankRuntimeState.cs             Mutable per-tank state
  BulletRuntimeState.cs           Mutable per-bullet state
  ArenaContext.cs                 Read-only arena view exposed to tanks

TankSwarmCode.SwarmTank/
  SwarmTank.cs                    SwarmTankBase — subclass this to write AI

TankSwarmCode.SwarmTank.Interfaces/
  ISwarmTank.cs                   Tank contract
  IArenaContext.cs                Arena read-only interface
  ArenaConstants.cs               All physics constants
  Models/                         TankState, TankCommand, RadarContact, …
  Events/                         Event arg types
  Enums/                          TankRole, SwarmMessageType

TankSwarmCode.SwarmTanks.Red/     Red Swarm AI
TankSwarmCode.SwarmTanks.Blue/    Blue Swarm AI
Documentation/                    This documentation
```

---

[← Overview](ch01-overview.md) | [Table of Contents](TOC.md) | [Next: Architecture →](ch03-architecture.md)
