# Chapter 13: ECM System

[← Configuration & Constants](ch12-configuration.md) | [Table of Contents](TOC.md) | [Next: Tank Energy →](ch14-tank-energy.md)

---

## Overview

Electronic Counter-Measures (ECM) is a per-tick energy expenditure that degrades the **radar pipeline** described in [Chapter 7](ch07-radar-system.md). Every tank can activate an ECM mode each tick by calling `SetEcm(EcmMode)` from `OnTick`. The engine applies the energy cost and injects the effects in the ECM phase (before radar scans are delivered).

ECM creates a strategic layer on top of raw firepower: a tank that cannot be reliably scanned is very hard to hit with linear-prediction fire, and a battlefield littered with ghost contacts causes well-coded attackers to waste shots on phantoms.

---

## ECM and Radar: Core Rules

Jamming has three hard consequences that apply to **both `Jam` and `JamAndSpoof`**:

| Rule | Detail |
|------|--------|
| **Own radar is offline** | No `OnScannedTank` fires; no `RadarMap` updates from own scans |
| **Cannot fire** | Firing is suppressed; jamming disables valid targeting |
| **Radio blocked (both directions)** | The jammer cannot send or receive swarm messages |

When jamming is turned off, the radar resumes automatically on the next tick.

---

## The Five Modes

### Off (default)

No ECM active. No energy cost. Tanks that never call `SetEcm` behave as if ECM does not exist.

---

### Jam

```csharp
SetEcm(EcmMode.Jam);   // 0.5 energy/tick
```

Floods the local EM spectrum with noise.

- **Own radar**: offline.
- **Firing**: blocked.
- **Radio**: blocked in both directions.
- **Enemy scans of this tank**:

| Outcome | Without Burnthrough | With Burnthrough |
|---------|---------------------|-----------------|
| **Scan dropped** — `OnScannedTank` never fires; `OnPainted` not fired | 50 % | 8 % |
| **Scan corrupted** — fires with randomised position, heading, velocity, energy; `OnPainted` **is** fired | 30 % | 8 % |
| **Scan normal** — accurate data; `OnPainted` fired | 20 % | 84 % |

A dropped scan makes the jamming tank completely invisible that tick. A corrupted scan is worse than nothing — the scanner fires on a false position.

**Trade-off**: Jam is the most powerful single-tick disruption, but disabling your own radar, firing, and radio is a significant tactical cost. Use in bursts when actively hunted, not as a default posture.

---

### Spoof

```csharp
SetEcm(EcmMode.Spoof);   // 0.8 energy/tick
```

Projects two "ghost" radar contacts at positions that drift independently around the spoofing tank.

**Ghost drift**: Each ghost starts at a random offset (25–155 px) from the spoofer and moves at a random velocity (up to ±5 px/tick). Every tick there is a 4 % chance each ghost changes direction and speed independently. Ghost names are `"Ghost-" + spoofer_name[0..4]` and have randomised energy (45–80).

Each tick the engine checks whether any ghost position falls within an enemy's sweep arc (with clear line-of-sight). If so, a fake `OnScannedTank` is injected. Ghost contacts do **not** trigger `OnPainted`.

Unlike `Jam`, Spoof does **not** disable the tank's own radar, firing, or radio.

**Burnthrough interaction**: A scanner running Burnthrough has a 70 % chance of recognising and discarding each ghost before it reaches `OnScannedTank`.

**Trade-off**: Spoof costs the most energy per tick (0.8) but the effect spreads silently through the `RadarShare` chain — every ally of the scanner will merge the ghost contact into their own `RadarMap`. One spoof tank can corrupt the entire opposing team's intelligence picture.

**Detecting spoofing**: Inspect contact names. Any name starting with `"Ghost-"` is a phantom; discard it and broadcast `EcmAlert` to your swarm.

---

### Burnthrough

```csharp
SetEcm(EcmMode.Burnthrough);   // 0.3 energy/tick
```

Focuses radar output to cut through enemy interference. For this tank's own scans:

- Jam drop chance: 50 % → **8 %**
- Jam corrupt chance: 30 % → **8 %**
- Ghost echoes from Spoof tanks discarded with **70 % probability** before reaching `OnScannedTank`

Burnthrough does **not** affect allies — each tank must activate its own. Use `EcmAlert` messages to coordinate.

**Trade-off**: Cheap (0.3/tick) and largely neutralises Jam. Against a Spoof tank with no Burnthrough counter, 30 % of ghosts still slip through — but far more manageable than 100 %.

---

### JamAndSpoof

```csharp
SetEcm(EcmMode.JamAndSpoof);   // 1.3 energy/tick (0.5 Jam + 0.8 Spoof)
```

Simultaneously runs both Jam and Spoof at full effect.

- **Own radar**: offline (same as Jam).
- **Firing**: blocked (same as Jam).
- **Radio**: blocked in both directions (same as Jam).
- **Enemy scans**: Jam drop/corrupt chances applied.
- **Ghost projections**: two drifting ghosts injected into enemy sweeps.

The highest-cost ECM option. A `JamAndSpoof` tank consumes 130 energy per 100 ticks — exceeding any other single mode. Intended for dedicated electronic-warfare tanks with no cannon, or emergency total-blackout scenarios.

---

## The `OnPainted` Event

When any tank's radar successfully sweeps over another tank (non-dropped scan), the **scanned tank** receives `OnPainted`:

```csharp
public virtual void OnPainted(PaintedEventArgs e) { }
```

| Field | Description |
|-------|-------------|
| `e.PainterName` | Name of the scanning tank |
| `e.PainterSwarmId` | Swarm of the scanning tank |
| `e.PainterPosition` | Arena position of the scanning tank at the moment of the scan |

### Default behaviour

The base class automatically broadcasts a `Painted` swarm message to all allies:

```csharp
Broadcast(new SwarmMessage
{
    SenderName = Name,
    Type       = SwarmMessageType.Painted,
    TargetName = e.PainterName,
    Position   = e.PainterPosition,
    Timestamp  = Arena.TickNumber
});
```

The entire swarm instantly learns the painter's position whenever any member is swept — at no cost to the AI author.

### Jamming interaction

If the painted tank is currently jamming, `OnPainted` still fires (the radar beam hits regardless of the painted tank's ECM), but the auto-broadcast is silently dropped because jamming blocks outgoing radio.

### Override examples

```csharp
// Evasion + keep auto-broadcast
public override void OnPainted(PaintedEventArgs e)
{
    base.OnPainted(e);   // auto-broadcasts [PAINTED] to allies
    SetTurnRight(45);
    SetBack(30);
}

// Custom broadcast with extra data (range)
public override void OnPainted(PaintedEventArgs e)
{
    // Don't call base — provide custom payload instead
    Broadcast(new SwarmMessage
    {
        SenderName = Name,
        Type       = SwarmMessageType.Painted,
        TargetName = e.PainterName,
        Position   = e.PainterPosition,
        CustomData = $"range:{(int)State.Position.DistanceTo(e.PainterPosition)}",
        Timestamp  = Arena.TickNumber
    });
}
```

---

## ECM Countermeasure Chain

The intended ECM meta-game between the built-in swarms:

```
RedGhost runs JamAndSpoof (ECMScreen strategy)
    → Red tanks close to 130 px under cover
    → Blue RadarMap fills with Ghost-RedGh contacts
    → Blue tanks running linear prediction fire on ghosts and miss

BlueEcm scans a Ghost contact (name starts with "Ghost-")
    → broadcasts EcmAlert to Blue swarm
    → SwarmBrainBase responds: all Blue tanks switch to Burnthrough

Blue swarm now filters 70% of ghost contacts per sweep

BlueEcm switches to offensive Jam (when strategy calls for it)
    → RedGhost is forced to switch from JamAndSpoof to Jam (or Burnthrough)
    → ghost projection stops; Red loses deception advantage

Additionally, every time an enemy radar paints any Blue tank:
    → OnPainted fires on the painted tank
    → [PAINTED] message auto-broadcast to Blue swarm
    → All Blue tanks know the enemy scanner's position
    → Blue attacker can fire on the scanner without its own radar lock
```

---

## Adding ECM to Your Own Tank

Any tank can use ECM — it is not restricted to the built-in specialists.

```csharp
public override void OnTick(TickEventArgs e)
{
    if (State.Energy > 50 && _underFire)
        SetEcm(EcmMode.Jam);
    else if (State.Energy > 30)
        SetEcm(EcmMode.Burnthrough);
    else
        SetEcm(EcmMode.Off);
    // Normal movement and firing logic below…
}

public override void OnHitByBullet(HitByBulletEventArgs e) => _underFire = true;
```

To react to ally ECM alerts:

```csharp
public override void OnSwarmMessage(SwarmMessageEventArgs e)
{
    base.OnSwarmMessage(e);   // always call base to merge RadarShare

    if (e.Message.Type == SwarmMessageType.EcmAlert)
        _burnthroughTicks = 40;
}
```

To detect and discard ghost contacts:

```csharp
public override void OnScannedTank(ScannedTankEventArgs e)
{
    if (e.Result.Name.StartsWith("Ghost-", StringComparison.Ordinal))
    {
        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type       = SwarmMessageType.EcmAlert,
            CustomData = "Ghost contact — activating burnthrough",
            Timestamp  = Arena.TickNumber
        });
        return;   // don't call base — prevents ghost entering RadarMap
    }

    base.OnScannedTank(e);
}
```

---

## Energy Budget

ECM competes directly with firing for energy. See [Chapter 14: Tank Energy](ch14-tank-energy.md) for the full energy model.

| Mode | Cost/100 ticks | Approx. equivalent power-1 shots |
|------|---------------|----------------------------------|
| Burnthrough | 30 | ~10 |
| Jam | 50 | ~17 |
| Spoof | 80 | ~27 |
| JamAndSpoof | 130 | ~43 |

Running Jam or JamAndSpoof continuously is extremely expensive and also disables your radar and radio. Use offensive jamming in bursts.

---

## Inspector Panel Override

Any tank's ECM mode can be forced from the UI without modifying its AI code. Left-click a tank and use the **ECM cycle button** at the bottom of the info panel:

```
Auto (AI)  →  OFF  →  JAM  →  SPOOF  →  JAM+SPOOF  →  ECCM  →  Auto (AI)  →  …
```

When an override is active, the button is highlighted purple with bold text. The override persists after the panel is closed until you cycle back to **Auto (AI)** or reset the arena. This is useful for testing ECM interactions without writing any code.

---

## Constants Reference

All ECM constants are in `ArenaConstants` (`TankSwarmCode.SwarmTank/ArenaConstants.cs`):

| Constant | Value | Description |
|----------|-------|-------------|
| `EcmJamCostPerTick` | 0.5 | Energy/tick for Jam |
| `EcmSpoofCostPerTick` | 0.8 | Energy/tick for Spoof |
| `EcmBurnthroughCostPerTick` | 0.3 | Energy/tick for Burnthrough |
| `EcmJamDropChance` | 0.50 | Jam drop probability (no Burnthrough) |
| `EcmJamCorruptChance` | 0.30 | Jam corrupt probability (no Burnthrough) |
| `EcmBurnthroughDropChance` | 0.08 | Drop chance with Burnthrough active |
| `EcmBurnthroughCorruptChance` | 0.08 | Corrupt chance with Burnthrough active |
| `EcmBurnthroughGhostFilterChance` | 0.70 | Ghost discard probability with Burnthrough |
| `EcmSpoofRadius` | 130.0 px | Maximum initial ghost offset from spoofer |
| `EcmSpoofGhostCount` | 2 | Ghost contacts per Spoof/JamAndSpoof tank |

---

[← Configuration & Constants](ch12-configuration.md) | [Table of Contents](TOC.md) | [Next: Tank Energy →](ch14-tank-energy.md)
