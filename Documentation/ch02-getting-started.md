# Chapter 2: Getting Started

[← Overview](ch01-overview.md) | [Table of Contents](TOC.md) | [Next: Architecture →](ch03-architecture.md)

---

## Prerequisites

| Requirement | Minimum Version |
|-------------|----------------|
| .NET SDK | 10.0 |
| Windows | Windows 10 or later (WinForms host) |
| Visual Studio / Rider | Any version supporting .NET 10 and `.slnx` solution files |

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

# Release build (single-file executable)
dotnet publish TankSwarmCode/TankSwarmCode.csproj -c Release
```

The Release publish target produces a self-contained, single-file `win-x64` executable in the standard `publish/` directory under the project.

---

## Running the Simulator

When the application starts you will see an empty arena and a toolbar/menu across the top.

### Step 1 — Add Tanks

Use the **Add Tanks** menu to select one or both pre-built swarms:

- **Red Swarm** — adds 6 tanks: RedScout, RedAlpha, RedBravo (RedAttacker × 2), RedWolf, RedFox (RedFlank × 2), and ECM-Jammer
- **Blue Swarm** — adds 7 tanks: BlueCommand, BluePatrol × 2, BlueSniper × 2, BlueWarden, and ECM-Operator

You can add both swarms for a full Red vs Blue battle, or add the same swarm twice for mirror matches.

### Step 2 — Configure the Arena (optional)

The **Arena Configuration** panel lets you set the width and height of the arena. Changes take effect at the next round start.

### Step 3 — Start the Round

Click **Start**. The engine will:

1. Randomly place buildings scaled to the arena area.
2. Spawn each tank at a random, collision-free position.
3. Call each tank's `OnStart()` hook.
4. Begin the tick loop.

### Step 4 — Watch and Interact

- **Left-click a tank** to attach the live info panel and monitor its state.
- **Right-click a tank** to pin/unpin focus on that tank.
- The **info panel** includes an **ECM override button** to force any ECM mode on a tank without editing AI code.
- The **Radio Log** panel scrolls swarm messages in real time, colour-coded by swarm.

### Step 5 — End of Round

The round ends when only one swarm has living members. Each survivor receives `OnRoundEnded(Won: true)`; dead tanks receive `OnRoundEnded(Won: false)`.

Click **Reset** to clear the arena and prepare for a new round, or **Start** again to replay with the same tank list.

---

## Speed Controls

| Control | Effect |
|---------|--------|
| Ticks/sec slider | Sets the simulation rate |
| **Stop** button | Pauses the simulation without resetting |
| **Step** button | Advances exactly one tick — useful for debugging AI logic |

---

## Headless CLI Runner

For AI training and benchmarking you can run matches without the GUI:

```bash
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj \
  --bot1 path/to/Red.dll --bot2 path/to/Blue.dll \
  --batch 100 --parallel 8 --format table
```

See [Chapter 15: Headless CLI Runner](ch15-cli.md) for the full flag reference, output formats (JSON, NDJSON, table, CSV), and example workflows.

---

## Project Layout

```
TankSwarmCode.Gui/                WinForms host
  Program.cs                      Entry point
  TankSwarmArena.cs               Main form (menus, controls, radio log)
  ArenaUserControl.cs             GDI+ renderer + engine host
  ArenaConfigurationUserControl   Arena dimension settings panel
  ScreenWakeLock.cs               Prevents display sleep during simulation

TankSwarmCode.Cli/
  Program.cs                      Headless match runner (JSON/NDJSON/table/CSV output)

TankSwarmCode.Arena/
  ArenaEngine.cs                  Tick loop, physics, collision detection
  TankRuntimeState.cs             Mutable per-tank state (engine-internal)
  BulletRuntimeState.cs           Mutable per-bullet state (engine-internal)
  ArenaContext.cs                 IArenaContext implementation

TankSwarmCode.SwarmTank/
  SwarmTank.cs                    SwarmTankBase — subclass this to write AI

TankSwarmCode.SwarmTank.Interfaces/
  ISwarmTank.cs                   Tank contract (engine ↔ AI boundary)
  IArenaContext.cs                Arena read-only interface exposed to tanks
  ArenaConstants.cs               All physics constants
  Models/                         TankState, TankCommand, RadarContact, BulletState, …
  Events/                         Event arg types for every lifecycle hook
  Enums/                          TankRole, EcmMode, SwarmMessageType

TankSwarmCode.SwarmTanks.Red/     Red Swarm AI (6 tanks)
TankSwarmCode.SwarmTanks.Blue/    Blue Swarm AI (7 tanks)
Documentation/                    This documentation
```

---

[← Overview](ch01-overview.md) | [Table of Contents](TOC.md) | [Next: Architecture →](ch03-architecture.md)
