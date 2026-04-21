# Red Swarm Engineering

This shared project is the working area for building and tuning the Red Swarm AI. It contains no compiled code — add implementation files to `TankSwarmCode.AiCortex.Red` (cortex logic) and `TankSwarmCode.SwarmTanks.Red` (tank shells).

---

## Tank Roster

| Tank | Slot | Role | MaxFirePower | PreferredRange | ECM |
|------|------|------|-------------|---------------|-----|
| RedArrow | 0 | Scout | 2.0 | 200 px | — |
| RedBlade | 1 | Attacker | 2.5 | 180 px | — |
| RedHammer | 2 | Attacker | 2.5 | 200 px | — |
| RedGhost | 3 | EcmSpecialist | 1.5 | 150 px | Spoof |
| RedTrooper | 4+ | Attacker | 2.0 | 200 px | — |

Slot numbers drive formation assignments and leadership election tiebreaking. Lower slot = higher leadership priority.

---

## Architecture: Two-File Pattern

Every tank is two files:

1. **Shell** (`TankSwarmCode.SwarmTanks.Red/`) — a thin `SwarmTankBase` subclass that implements `ITankContext` and delegates all lifecycle calls to a cortex via `CortexFactory`. **Never edit this file** — it is frozen by design.

2. **Cortex** (`TankSwarmCode.AiCortex.Red/`) — an `IAiCortex` subclassing `RedCortexBase`. This is the only file that changes between research iterations.

```
TankSwarmCode.SwarmTanks.Red/RedGhost.cs        ← frozen shell
TankSwarmCode.AiCortex.Red/RedGhostCortex.cs    ← AI logic (edit this)
```

---

## Quick Start — Adding a New Tank

### 1. Create the cortex

Add a new file in `TankSwarmCode.AiCortex.Red/`:

```csharp
using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.AiCortex.Red;

public sealed class RedViperCortex : RedCortexBase
{
    protected override TankConfiguration TankConfiguration { get; } = new()
    {
        FormationSlot = 5,
        MaxFirePower = 2.0,
        PreferredRange = 190.0,
        HasEcm = false,
        RetreatEnergyThreshold = 22.0
    };
}
```

### 2. Create the shell

Add a new file in `TankSwarmCode.SwarmTanks.Red/`:

```csharp
using TankSwarmCode.AiCortex.Red.Library;
using TankSwarmCode.SwarmTank;
using TankSwarmCode.SwarmTank.Enums;
using TankSwarmCode.SwarmTank.Events;
using TankSwarmCode.SwarmTank.Models;

namespace TankSwarmCode.SwarmTanks.Red;

public sealed class RedViper : SwarmTankBase, ITankContext
{
    private readonly IAiCortex _cortex = CortexFactory.For("RedViper");

    public RedViper() { SwarmId = 1; Role = TankRole.Attacker; }

    public override void OnStart()                               => _cortex.OnStart(this);
    public override void OnTick(TickEventArgs e)                 => _cortex.OnTick(this);
    public override void OnSwarmMessage(SwarmMessageEventArgs e) => _cortex.OnSwarmMessage(this, e);
    public override void OnRoundEnded(RoundEndedEventArgs e)     => _cortex.OnRoundEnded(this, e);

    string ITankContext.Name => Name;
    int ITankContext.SwarmId => SwarmId;
    TankRole ITankContext.Role { get => Role; set => Role = value; }
    TankState ITankContext.State => State;
    IArenaContext ITankContext.Arena => Arena;
    IReadOnlyDictionary<string, RadarContact> ITankContext.RadarMap => RadarMap;
    IReadOnlyDictionary<string, BuildingEcho> ITankContext.BuildingWallMap => BuildingWallMap;
    void ITankContext.SetAhead(double d)          => SetAhead(d);
    void ITankContext.SetBack(double d)           => SetBack(d);
    void ITankContext.SetTurnRight(double d)      => SetTurnRight(d);
    void ITankContext.SetTurnLeft(double d)       => SetTurnLeft(d);
    void ITankContext.SetTurnGunRight(double d)   => SetTurnGunRight(d);
    void ITankContext.SetTurnGunLeft(double d)    => SetTurnGunLeft(d);
    void ITankContext.SetTurnRadarRight(double d) => SetTurnRadarRight(d);
    void ITankContext.SetTurnRadarLeft(double d)  => SetTurnRadarLeft(d);
    void ITankContext.SetFire(double p)           => SetFire(p);
    void ITankContext.SetEcm(EcmMode m)           => SetEcm(m);
    void ITankContext.Broadcast(SwarmMessage msg) => Broadcast(msg);
}
```

### 3. Register in CortexFactory

In `TankSwarmCode.AiCortex.Red/Library/CortexFactory.cs`, add `"RedViper" => new RedViperCortex()`.

### 4. Add to the GUI

In `TankSwarmArena.cs`, call `arenaUserControl1.AddTank(new RedViper())` inside a Red Swarm click handler.

### 5. Test headless

```bash
dotnet build TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release

dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 100 --parallel 8 --format table
```

---

## Key API

| Method | Purpose |
|--------|---------|
| `ctx.SetAhead(dist)` / `ctx.SetBack(dist)` | Queue forward/backward movement |
| `ctx.SetTurnRight(deg)` / `ctx.SetTurnLeft(deg)` | Queue body turn |
| `ctx.SetTurnGunRight(deg)` / `ctx.SetTurnGunLeft(deg)` | Queue gun turn |
| `ctx.SetTurnRadarRight(deg)` / `ctx.SetTurnRadarLeft(deg)` | Queue radar turn |
| `ctx.SetFire(power)` | Fire bullet (power 0.1–3.0; higher = slower, more damage) |
| `ctx.SetEcm(EcmMode)` | Activate ECM for this tick (Jam / Spoof / JamAndSpoof / Burnthrough) |
| `ctx.Broadcast(msg)` | Send a typed message to all allies |

**State properties** (read in `OnTick`): `ctx.State.Position`, `.Heading`, `.GunHeading`, `.RadarHeading`, `.Energy`, `.Velocity`

**Arena:** `ctx.Arena.ArenaWidth`, `ctx.Arena.ArenaHeight`, `ctx.Arena.TickNumber`

**Radar:** `ctx.RadarMap` — merged picture from own scans + ally RadarShare messages. Discard contacts where `TickNumber - contact.Timestamp > 30`.

---

## Current Baselines

**200 matches, seed 1000, `--on-timeout energy`**

| Architecture | Red % | Notes |
|---|---|---|
| New arch (post-iter-84) baseline | 34% | RedGhost ECM not activating via cortex |
| + Proactive ECM (iter-1 on this branch) | ~50% | Spoof whenever enemies visible |

### Red Tank Battle Intel

| Tank | Role | Key traits |
|------|------|-----------|
| RedGhost | ECM | All-in; primary target for Blue (killed first ~14% of matches); Spoof-mode ECM |
| RedHammer | Attacker | MVP / Top attacker; highest Rate/100t |
| RedBlade | Attacker | Co-MVP; strong win-survival |
| RedArrow | Scout | Low combat rate; frequently survives losses |

### Enemy Battle Intel (observed, not from Blue source)

| Blue Tank | Observed behavior |
|-----------|-----------------|
| BlueSharp | MVP in ~80% of Blue wins; primary threat to kill |
| BlueGuard | Top attacker; highest damage to Red |
| BlueEcm | Linchpin when alive via kill-chain effects |

---

## Common Mistakes

| Mistake | Fix |
|---------|-----|
| Not normalising angle differences | Wrap with `NormalizeAngle()` before every `SetTurnGunRight` / `SetTurnRight` |
| Using `Arena.Width` / `Arena.Height` | Use `ctx.Arena.ArenaWidth` / `ctx.Arena.ArenaHeight` |
| Skipping `base.OnScannedTank(e)` | `RadarMap` never updates; no `RadarShare` to allies |
| Skipping `base.OnSwarmMessage(e)` | Ally radar contacts are never merged |
| Firing every tick regardless of alignment | Gate `SetFire` on gun alignment < 8° to avoid wasting energy |
| Setting ECM in `OnStart` | ECM must be set each tick; it resets to `Off` automatically |
| Rebuilding only one DLL before a benchmark | Stale DLLs contaminate comparison runs — always rebuild both |

---

## Documentation

| Chapter | Topic |
|---------|-------|
| [ch00 — Quick Reference](../Documentation/ch00-quick-reference.md) | CLI flags, insight labels, ECM modes, API cheat sheet |
| [ch05 — Tank AI Framework](../Documentation/ch05-tank-ai-framework.md) | Full lifecycle and event reference |
| [ch06 — Swarm Communication](../Documentation/ch06-swarm-communication.md) | Messaging and RadarShare |
| [ch07 — Radar System](../Documentation/ch07-radar-system.md) | RadarMap, contacts, staleness |
| [ch09 — Built-in Tanks](../Documentation/ch09-builtin-tanks.md) | Existing Red and Blue AI as reference |
| [ch10 — Building Your Own Tank](../Documentation/ch10-custom-tank.md) | Full walkthrough with code examples |
| [ch13 — ECM System](../Documentation/ch13-ecm-system.md) | Jam, Spoof, Burnthrough |
| [ch15 — Headless CLI](../Documentation/ch15-cli.md) | Batch runs, output formats, training workflows |
