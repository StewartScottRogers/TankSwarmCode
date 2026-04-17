# Chapter 14: Tank Energy

[← ECM System](ch13-ecm-system.md) | [Table of Contents](TOC.md)

---

## Overview

Every tank enters a round with **100 energy** (`ArenaConstants.TankStartEnergy`). Energy is the single resource that governs firing, ECM, survival, and the victory condition: a tank is eliminated the moment its energy reaches zero.

Read the current energy at any time from `State.Energy` (inside `SwarmTankBase`) or from any `TankState` snapshot.

---

## Energy Drains

### Firing

Firing costs energy equal to the bullet's fire power:

```
energy -= firePower          // firePower ∈ [0.1, 3.0]
```

If `State.Energy < firePower`, the shot is silently cancelled — the engine never fires a bullet a tank cannot afford. Tanks running `EcmMode.Jam` or `EcmMode.JamAndSpoof` cannot fire at all.

### Taking a bullet hit

Bullet damage follows the Robocode formula (from `BulletState.Damage`):

```
damage = 4 × power + (power > 1 ? 2 × (power − 1) : 0)
```

| Fire power | Damage |
|------------|--------|
| 0.1 (min)  | 0.4    |
| 1.0        | 4.0    |
| 2.0        | 10.0   |
| 3.0 (max)  | 16.0   |

### Wall collision

Hitting a wall deals velocity-scaled damage:

```
wallDamage = max(|velocity| × 0.5 − 1.0, 0)
```

At maximum velocity (8 px/tick) this is 3.0. Grazes below 2 px/tick cost nothing.

### Tank–tank collision

Both tanks in a body collision each lose **0.6 energy** (`ArenaConstants.TankCollisionDamage`).

### ECM modes

Running ECM drains energy every tick the mode is active:

| Mode | Cost/tick |
|------|-----------|
| Jam | 0.5 |
| Spoof | 0.8 |
| JamAndSpoof | 1.3 (Jam + Spoof combined) |
| Burnthrough | 0.3 |

If a tank has insufficient energy to sustain its requested ECM mode, the engine silently ignores the request for that tick.

---

## Energy Gains

### Shooting an enemy

When your bullet hits an enemy, you recover energy equal to three times the bullet's power:

```
energyReturn = 3 × firePower
```

| Fire power | Energy returned |
|------------|-----------------|
| 0.1        | 0.3             |
| 1.0        | 3.0             |
| 3.0        | 9.0             |

This means accurate shooting is self-sustaining: a tank that lands consistent hits bleeds the target while replenishing itself.

---

## Death Condition

The engine calls `CheckDeath` after every energy-reducing event. When `Energy <= 0`, the tank is marked dead (`IsAlive = false`, `DestroyedAtTick` is set to the current tick number) and removed from all further physics processing. `OnDeath()` fires on the tank. The renderer displays a burning hulk at the tank's last position until the round resets.

---

## Power / Speed / Damage Trade-offs

Bullet speed decreases as power increases:

```
bulletSpeed = 20 − 3 × firePower
```

| Fire power | Speed (px/tick) | Damage | Energy return |
|------------|-----------------|--------|---------------|
| 0.1        | 19.7            | 0.4    | 0.3           |
| 1.0        | 17.0            | 4.0    | 3.0           |
| 2.0        | 14.0            | 10.0   | 6.0           |
| 3.0        | 11.0            | 16.0   | 9.0           |

**Heavy bullets** hit harder and return more energy but are easier to dodge at range.  
**Light bullets** travel faster and are harder to dodge but deal minimal damage and return almost nothing.

A common tactic is to fire lighter bullets when the enemy is far away (hard to lead) and switch to heavy power at close range or for a potential kill shot.

---

## Watching Energy from AI Code

```csharp
public override void OnTick(TickEventArgs e)
{
    double energy = State.Energy;

    // Desperate survival: stop ECM to conserve energy
    if (energy < 20)
        SetEcm(EcmMode.Off);

    // Kill shot: fire maximum power when the target is weak and close
    if (energy >= 3.0 && targetIsClose && targetIsWeak)
        SetFire(3.0);
}
```

`OnBulletHit` fires when your bullet connects:

```csharp
public override void OnBulletHit(BulletHitEventArgs e)
{
    // e.Bullet.EnergyReturn shows how much energy was credited this tick
    // e.Victim.Energy shows the target's remaining energy
    if (e.Victim.Energy <= 0)
    {
        // Kill confirmed — target is destroyed
    }
}
```

---

## Key Constants

| Constant | Value | Notes |
|----------|-------|-------|
| `TankStartEnergy` | 100 | Starting energy each round |
| `BulletMinPower` | 0.1 | Minimum fire power |
| `BulletMaxPower` | 3.0 | Maximum fire power |
| `TankCollisionDamage` | 0.6 | Per-tank cost of body collision |
| `WallDamageFactor` | 0.5 | Multiplier in wall damage formula |
| `WallDamageThreshold` | 1.0 | Minimum velocity for wall damage |
| `EcmJamCostPerTick` | 0.5 | Energy cost of Jam mode |
| `EcmSpoofCostPerTick` | 0.8 | Energy cost of Spoof mode |
| `EcmBurnthroughCostPerTick` | 0.3 | Energy cost of Burnthrough mode |

See [Chapter 12: Configuration & Constants](ch12-configuration.md) for the full constants reference.

---

[← ECM System](ch13-ecm-system.md) | [Table of Contents](TOC.md)
