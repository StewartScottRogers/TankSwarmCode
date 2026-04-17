# Chapter 13: ECM System

[← Configuration & Constants](ch12-configuration.md) | [Table of Contents](TOC.md)

---

## Overview

Electronic Counter-Measures (ECM) is a per-tick energy expenditure that degrades the **radar pipeline** described in [Chapter 7](ch07-radar-system.md). Every tank can activate an ECM mode each tick by calling `SetEcm(EcmMode)` from `OnTick`. The engine applies the energy cost and injects the effects before radar scans are delivered.

ECM creates a new strategic layer on top of raw firepower: a tank that cannot be reliably scanned is very hard to hit with linear-prediction fire, and a battlefield littered with ghost contacts causes even well-coded attackers to waste shots on phantoms.

---

## ECM and Radar: Core Rules

Jamming has three hard consequences that apply to **both `Jam` and `JamAndSpoof`**:

| Rule | Detail |
|------|--------|
| **Own radar is offline** | `ProcessRadarScan` skips the jammer entirely — no `OnScannedTank` fires, no `RadarMap` updates |
| **Cannot fire** | Firing is suppressed; no valid radar data means no valid targeting |
| **Radio blocked (both directions)** | The jammer cannot send or receive swarm messages; `RadarShare`, `[PAINTED]`, and all other message types are silently dropped |

When jamming is turned off, the radar resumes automatically on the next tick with no state to restore.

---

## The Five Modes

### Off (default)

No ECM active. No energy cost. This is the default state — tanks that never call `SetEcm` behave exactly as before the ECM system was added.

---

### Jam

```csharp
SetEcm(EcmMode.Jam);   // 0.5 energy/tick
```

**What it does**: Floods the local EM spectrum with noise.

- **Own radar**: offline this tick.
- **Firing**: blocked (no radar data).
- **Radio**: blocked in both directions.
- **Enemy scans of this tank**: when an enemy radar sweeps over the jammer, the engine rolls before delivering `OnScannedTank`:

| Outcome | Probability (no counter) | Probability (enemy has Burnthrough) |
|---------|--------------------------|--------------------------------------|
| **Scan dropped** — `OnScannedTank` never fires | 50 % | 8 % |
| **Scan corrupted** — event fires with randomised position, heading, velocity, energy | 30 % | 8 % |
| **Scan normal** — event fires with accurate data | 20 % | 84 % |

A dropped scan makes the jamming tank completely invisible that tick. A corrupted scan is worse than nothing — the scanner fires on a false position.

**Tradeoff**: Jam is the most powerful single-tick disruption, but disabling your own radar and radio is a significant tactical cost. Use it when you are being actively hunted, not as a default posture.

---

### Spoof

```csharp
SetEcm(EcmMode.Spoof);   // 0.8 energy/tick
```

**What it does**: Projects two "ghost" radar contacts at positions that drift independently around the spoofing tank. Each tick the engine checks whether any ghost position falls within an enemy's sweep arc (and has clear line-of-sight). If so, the engine injects a fake `OnScannedTank` event for that enemy.

Ghost contacts look identical to real scans except:

- The `Name` field is `"Ghost-XXXX"` (first four characters of the spoofing tank's name).
- The `Energy` value is randomised in the range 45–80.

Unlike `Jam`, Spoof does **not** disable the tank's own radar or radio. The tank can continue scanning, shooting, and communicating normally while projecting deception.

**Ghost drift behaviour**: Each ghost starts at a random offset (20–130 px) from the spoofer and moves at a random velocity (up to ±5 px/tick). Every tick there is a 4 % chance each ghost changes direction and speed independently, keeping the motion convincingly organic.

**Detecting spoofing**: An ECCM-aware AI can inspect contact names. Any name starting with `"Ghost-"` is a phantom; the AI should discard it and broadcast `EcmAlert` to its swarm.

**Burnthrough interaction**: A scanner running Burnthrough has a 70 % chance of recognising and discarding each ghost contact before it reaches `OnScannedTank`.

**Tradeoff**: Spoof costs the most energy per tick (0.8) but the effect persists silently across the enemy swarm through the `RadarShare` broadcast chain — every ally of the scanner will merge the ghost contact into their own `RadarMap`. One spoof tank can corrupt the entire opposing team's intelligence picture.

---

### Burnthrough

```csharp
SetEcm(EcmMode.Burnthrough);   // 0.3 energy/tick
```

**What it does**: Focuses radar output to cut through enemy interference. For this tank's own radar scans:

- Jam drop chance falls from 50 % to **8 %**.
- Jam corrupt chance falls from 30 % to **8 %**.
- Ghost echoes from Spoof tanks are discarded with **70 % probability** before reaching `OnScannedTank`.

Burnthrough does **not** affect allies — each tank must activate its own Burnthrough. The `EcmAlert` message is the mechanism for coordinating this.

**Tradeoff**: Burnthrough is cheap (0.3/tick) and largely neutralises Jam. Against a pure Spoof tank with no Burnthrough response, ghost contacts still slip through 30 % of the time — but this is far more manageable than 100 %.

---

### JamAndSpoof

```csharp
SetEcm(EcmMode.JamAndSpoof);   // 1.3 energy/tick (0.5 Jam + 0.8 Spoof)
```

**What it does**: Simultaneously runs both `Jam` and `Spoof` at full effect.

- **Own radar**: offline (same as `Jam`).
- **Firing**: blocked (same as `Jam`).
- **Radio**: blocked in both directions (same as `Jam`).
- **Enemy scans of this tank**: jam drop/corrupt chances applied (same as `Jam`).
- **Ghost projections**: two drifting ghost echoes injected into enemy sweeps (same as `Spoof`).

The combined mode is the highest-cost ECM option and is intended for dedicated electronic-warfare tanks with no cannon, or for emergency total blackout scenarios. A `JamAndSpoof` tank consumes 130 energy per 100 ticks — more than any other single mode.

**Aura**: The renderer draws both the static-dot Jam burst and the orbiting ghost-hull Spoof indicator simultaneously.

---

## The `OnPainted` Event

When any tank's radar successfully sweeps over another tank (non-dropped scan), the **scanned tank** receives an `OnPainted` callback:

```csharp
protected virtual void OnPainted(PaintedEventArgs e)
```

`PaintedEventArgs` fields:

| Field | Description |
|-------|-------------|
| `PainterName` | Name of the tank whose radar painted this tank |
| `PainterSwarmId` | Swarm the painter belongs to |
| `PainterPosition` | Arena position of the painter at the moment of the scan |

### Default behaviour

The base class implementation automatically broadcasts a `[PAINTED]` swarm message to all allies:

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

This means the entire swarm instantly learns the painter's position whenever any member is swept by enemy radar — at no cost to the AI author.

### Jamming interaction

If the painted tank is currently jamming (`Jam` or `JamAndSpoof`), the `OnPainted` callback still fires (the radar beam hits regardless of your own ECM mode), but the auto-broadcast is silently dropped by the engine because jamming blocks outgoing radio.

### Override example

```csharp
public override void OnPainted(PaintedEventArgs e)
{
    base.OnPainted(e);   // auto-broadcasts [PAINTED] to allies

    // Additional reaction: evasive manoeuvre
    SetTurnRight(45);
    SetBack(30);
}
```

To suppress the auto-broadcast and handle manually:

```csharp
public override void OnPainted(PaintedEventArgs e)
{
    // Don't call base — handle the [PAINTED] broadcast yourself with custom data
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

## ECM as a Countermeasure Chain

```
Red jammer runs Spoof
    → Blue radar map fills with Ghost-ECM contacts
    → Blue tanks fire on ghosts and miss
    → BlueEcmOperator scans a Ghost contact
        → calls OnScannedTank with name "Ghost-ECM..."
        → broadcasts EcmAlert to Blue swarm
        → activates Burnthrough
    → Blue swarm now filters 70% of ghost contacts
    → BlueEcmOperator switches to offensive Jam
        → RedEcmJammer is forced to switch from Spoof to Jam
        → ghost projection stops
        → Red swarm loses deception advantage

Additionally, every time an enemy radar paints a Blue tank:
    → OnPainted fires on the painted Blue tank
    → [PAINTED] message auto-broadcast to Blue swarm
    → All Blue tanks now know the enemy scanner's position
    → Blue attacker can fire on the scanner even without its own radar lock
```

This chain — Spoof → alert → Burnthrough → offensive Jam → force mode change — is the intended ECM meta-game. The `[PAINTED]` mechanic adds a reciprocal intelligence layer: aggressive radar use reveals the scanner's own position.

---

## Energy Budget Considerations

ECM competes directly with firing for energy.

| Mode | Cost/100 ticks | Approximate equivalent shots |
|------|---------------|-------------------------------|
| Burnthrough | 30 | ~6 power-1 shots |
| Jam | 50 | ~10 power-1 shots |
| Spoof | 80 | ~16 power-1 shots |
| JamAndSpoof | 130 | ~26 power-1 shots |

Running `Jam` or `JamAndSpoof` continuously is extremely expensive and also disables your radar and radio, making you dependent entirely on pre-jam intelligence. Use these modes in bursts.

For mixed tanks (`BlueEcmOperator`), the recommended pattern is:
- Default to Burnthrough at 0.3/tick to protect the team's radar picture.
- Use `Off` when energy drops below ~25 to avoid ECM-induced death.
- Use offensive Jam only when energy is comfortably high (> 70) and a confirmed ECM threat exists.

---

## Built-in ECM Tanks

### RedEcmJammer (Red Swarm)

| Property | Value |
|----------|-------|
| File | `TankSwarmCode.SwarmTanks.Red/RedEcmJammer.cs` |
| Role | EcmSpecialist |
| Cannon | None |
| Default mode | Spoof |
| Threat response | Jam for 25 ticks on hit, then revert to Spoof |
| Movement | Orbits arena centre (~22 % of smaller dimension radius) |

**Design intent**: Persistent battlefield deception. The jammer's ghosts spread into mid-arena where combat happens, forcing enemies to deal with a contaminated radar picture from tick 1.

---

### BlueEcmOperator (Blue Swarm)

| Property | Value |
|----------|-------|
| File | `TankSwarmCode.SwarmTanks.Blue/BlueEcmOperator.cs` |
| Role | EcmSpecialist |
| Cannon | Light (max 2.0 power) |
| Default mode | Burnthrough |
| Ghost detection | Inspects `OnScannedTank` for `"Ghost-"` prefix |
| Offensive mode | Jam for 20 ticks when energy > 70 and ECM confirmed |
| Movement | Figure-8 patrol |

**Design intent**: ECCM anchor for the Blue swarm. The operator's Burnthrough and `EcmAlert` broadcasts turn the whole swarm's Burnthrough coordination on when the enemy jammer is active.

---

## Adding ECM to Your Own Tank

Any tank can use ECM — it is not restricted to the built-in specialists.

```csharp
public override void OnTick(TickEventArgs e)
{
    // Activate Jam when threatened and energy allows
    if (State.Energy > 50 && _underFire)
        SetEcm(EcmMode.Jam);
    else if (State.Energy > 30)
        SetEcm(EcmMode.Burnthrough);
    else
        SetEcm(EcmMode.Off);

    // Normal movement and firing logic below...
}

public override void OnHitByBullet(HitByBulletEventArgs e) => _underFire = true;
```

To react to ally ECM alerts:

```csharp
public override void OnSwarmMessage(SwarmMessageEventArgs e)
{
    base.OnSwarmMessage(e);   // always call base to merge RadarShare

    if (e.Message.Type == SwarmMessageType.EcmAlert)
        _burnthroughTicks = 40;   // run Burnthrough for 40 ticks
}
```

To react to being painted (and retaliate):

```csharp
public override void OnPainted(PaintedEventArgs e)
{
    base.OnPainted(e);   // auto-broadcast [PAINTED] to allies

    // The painter's position is now known — record it as a fire target
    _knownThreat = e.PainterPosition;
}
```

To detect spoofing and avoid wasting fire on ghosts:

```csharp
public override void OnScannedTank(ScannedTankEventArgs e)
{
    if (e.Result.Name.StartsWith("Ghost-", StringComparison.Ordinal))
    {
        // This is a ghost — do not track it as a real enemy
        Broadcast(new SwarmMessage
        {
            SenderName = Name,
            Type       = SwarmMessageType.EcmAlert,
            CustomData = "Ghost contact detected — activating burnthrough",
            Timestamp  = Arena.TickNumber
        });
        return;   // don't call base — prevents ghost entering RadarMap
    }

    base.OnScannedTank(e);   // record real contact normally
}
```

---

## Inspector Panel Override

Any tank's ECM mode can be forced from the UI without modifying its AI code. Right-click a tank and choose **Attach Info Panel**, then use the **ECM cycle button** at the bottom of the panel:

```
Auto (AI)  →  OFF  →  JAM  →  SPOOF  →  JAM+SPOOF  →  ECCM  →  Auto (AI)  →  …
```

Each click cycles to the next mode. When an override is active:

- The button is highlighted purple with bold text.
- The engine ignores whatever mode the tank's `OnTick` requested and applies the override instead, deducting the correct energy cost.
- The override **persists** after the panel is closed — the tank stays in the forced mode until you reopen the panel and cycle back to **Auto (AI)**, or the arena is fully reset.

This is useful for testing ECM interactions without writing any code — for example, forcing `RedScout` into Spoof to see how the Blue swarm responds before building a dedicated jammer.

---

## Constants Reference

All ECM constants are in `ArenaConstants` (`TankSwarmCode.SwarmTank.Interfaces/ArenaConstants.cs`):

| Constant | Value | Description |
|----------|-------|-------------|
| `EcmJamCostPerTick` | 0.5 | Energy/tick for Jam |
| `EcmSpoofCostPerTick` | 0.8 | Energy/tick for Spoof |
| `EcmBurnthroughCostPerTick` | 0.3 | Energy/tick for Burnthrough |
| `EcmJamAndSpoofCostPerTick` | 1.3 | Energy/tick for JamAndSpoof (Jam + Spoof combined) |
| `EcmJamDropChance` | 0.50 | Jam scan-drop probability (no counter) |
| `EcmJamCorruptChance` | 0.30 | Jam scan-corrupt probability (no counter) |
| `EcmBurnthroughDropChance` | 0.08 | Jam scan-drop with Burnthrough active |
| `EcmBurnthroughCorruptChance` | 0.08 | Jam scan-corrupt with Burnthrough active |
| `EcmBurnthroughGhostFilterChance` | 0.70 | Ghost discard probability with Burnthrough |
| `EcmSpoofRadius` | 130.0 px | Maximum initial ghost offset from spoofer |
| `EcmSpoofGhostCount` | 2 | Ghost contacts per Spoof/JamAndSpoof tank |

---

[← Configuration & Constants](ch12-configuration.md) | [Table of Contents](TOC.md)
