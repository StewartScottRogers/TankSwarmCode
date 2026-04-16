# Chapter 11: Arena Rendering & UI

[← Building Your Own Tank](ch10-custom-tank.md) | [Table of Contents](TOC.md) | [Next: Configuration & Constants →](ch12-configuration.md)

---

## Overview

The renderer is implemented as `ArenaUserControl` (`TankSwarmCode/ArenaUserControl.cs`), a WinForms `UserControl` that hosts the `ArenaEngine` and paints each frame using GDI+. Rendering is decoupled from the simulation: the renderer reads **immutable snapshots** (`TankState`, `BulletState`) and never mutates engine state.

---

## ArenaUserControl

`ArenaUserControl` is responsible for:

1. Owning the `ArenaEngine` instance.
2. Driving the tick loop via a `System.Windows.Forms.Timer` set to the configured ticks/second rate.
3. Painting the arena (double-buffered GDI+ surface) after each tick.
4. Handling mouse input for the interactive inspection panel.

Double buffering is enabled so each completed frame is swapped in atomically — no flicker even at high tick rates.

---

## Visual Elements

### Tank Body

Each tank is drawn as a filled square (18 × 18 px by default) rotated to match `TankState.Heading`. Team colour is determined by `SwarmId`:

- SwarmId 1 (Red Swarm) → red body
- SwarmId 2 (Blue Swarm) → blue body
- SwarmId 0 or other → a neutral colour

### Gun Barrel

A line of 22 px extends from the tank centre in the direction of `GunHeading`. The gun line is drawn on top of the rotated body.

### Radar Beam

A shorter line (16 px) in the direction of `RadarHeading` represents the current radar pointing angle. It is rendered in a lighter tint to distinguish it from the gun.

### Energy Bar

A horizontal bar (36 × 4 px) above each tank shows current energy as a proportion of maximum:

- Green: energy > 50%
- Yellow: energy 25–50%
- Red: energy < 25%

### Radar Halo

When a scan detects a target, an expanding/fading circular pulse (sonar halo) is rendered at the scanning tank's position. Each halo lives for **10 ticks** (~1.7 s at the default speed), expanding outward and becoming more transparent as it ages. Multiple halos can be active simultaneously.

### Destroyed Tank Hull

When a tank's energy reaches 0, its body stops moving but remains visible as a darkened hull at `DestroyedAtTick` position. The hull persists until the round resets.

### Explosion Animation

Upon destruction, a 2.1-second burn animation plays at the tank's last position:

- Phase 1 (0–700 ms): bright orange/yellow expanding flash
- Phase 2 (700–1400 ms): contracting fire glow
- Phase 3 (1400–2100 ms): dissipating smoke (grey fading to transparent)

Multiple simultaneous explosions are each tracked independently.

### Bullets

Each active bullet is rendered as a small filled circle (radius ~3 px) at `BulletState.Position`. A short tail in the direction opposite to travel gives a visual velocity cue.

### Buildings

Buildings are filled rectangles drawn in a muted grey/brown colour with a darker border. They are painted before tanks and bullets so they appear as background obstacles.

---

## Interactive Features

### Click to Inspect

**Left-click** on any tank body to open the **info panel**, which displays:

- Tank name and swarm
- Current energy
- Position (X, Y)
- Headings (body, gun, radar)
- Velocity
- Role

The panel updates live every tick while the tank is selected.

### Right-click to Pin

**Right-click** a tank to pin it — the info panel follows only that tank regardless of other clicks.

### Double-click to Follow

**Double-click** a tank to lock the viewport (if scrolling is enabled) on that tank.

---

## TankSwarmArena (Main Form)

`TankSwarmArena` (`TankSwarmCode/TankSwarmArena.cs`) is the root WinForms window. It hosts `ArenaUserControl` and provides the application shell.

### Controls

| Control | Function |
|---------|----------|
| **Add Tanks** menu | Select pre-built swarms or custom tank classes |
| **Start** button | Initialise a new round and begin the tick loop |
| **Stop** button | Pause the simulation without resetting |
| **Step** button | Advance exactly one tick (useful for debugging) |
| **Reset** button | Clear the arena and all tank instances |
| Speed slider | Adjust simulation rate (1 tick/s to unlimited) |
| Arena size fields | Set width × height; applied on next round start |

### Radio Log

A scrolling panel shows swarm messages as they are delivered. Each entry includes:

- Tick number
- Sender name (colour-coded by swarm)
- Message type
- Key fields (target name, position if present)

The log is capped at **300 entries**; it auto-trims to 250 when the limit is reached.

---

## ArenaConfigurationUserControl

A small embedded form (`TankSwarmCode/ArenaConfigurationUserControl.cs`) lets users set the arena **width** and **height** before starting a round. Values are validated to ensure a minimum usable arena size.

---

## ScreenWakeLock

`TankSwarmCode/ScreenWakeLock.cs` calls `SetThreadExecutionState` to prevent the Windows display from sleeping while a simulation is running. The lock is acquired when the tick loop starts and released when it stops.

---

## Render Settings

The following rendering options can be adjusted at runtime (via properties on `ArenaUserControl`):

| Setting | Default | Description |
|---------|---------|-------------|
| Antialiasing | On | Smooths tank and bullet edges |
| Radar trails | On | Show the radar sweep arc trail |
| Explosion animations | On | Burn/smoke sequences on destruction |
| Energy bars | On | Show energy bars above tanks |
| Info panel | Off | Show live state panel (enabled on click) |

---

## Rendering Cycle

```
Timer fires
    │
    ▼
ArenaEngine.Tick()          — one physics step; publishes new TankState snapshots
    │
    ▼
ArenaUserControl.Invalidate()    — marks control dirty
    │
    ▼
OnPaint(PaintEventArgs e)
    ├─ FillBackground()
    ├─ DrawBuildings()           — muted rectangles
    ├─ DrawBullets()             — small circles with tails
    ├─ DrawRadarHalos()          — expanding/fading pulses
    ├─ DrawTanks()               — body, gun, radar beam, energy bar
    │     ├─ Alive tanks:        — full colour, rotating body
    │     └─ Destroyed hulls:    — dark greyscale
    ├─ DrawExplosions()          — per-tank animation state machine
    └─ DrawInfoPanel()           — (if a tank is selected)
```

All GDI+ objects (`Pen`, `Brush`, `SolidBrush`) are cached or created once per render settings change to avoid per-frame allocation.

---

[← Building Your Own Tank](ch10-custom-tank.md) | [Table of Contents](TOC.md) | [Next: Configuration & Constants →](ch12-configuration.md)
