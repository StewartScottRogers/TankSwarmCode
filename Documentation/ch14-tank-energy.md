# Chapter 14 — Tank Energy

## Overview

Every tank enters a round with **100 energy** (`ArenaConstants.TankStartEnergy`). Energy is the single resource that governs firing, ECM, survival, and victory condition: a tank is eliminated the moment its energy reaches zero.

Energy can be read at any time from `TankState.Energy`.

---

## Energy Drains

### Firing

Firing costs energy equal to the bullet's fire power:

```
energy -= firePower          // firePower ∈ [0.1, 3.0]
```

If `TankState.Energy < firePower` the shot is silently skipped — the engine never fires a bullet a tank cannot afford.

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

At maximum velocity (8 px/tick) this is 3.0. Slow-speed grazes below 2 px/tick cost nothing.

### Tank-tank collision

Both tanks in a body collision each lose `0.6` energy (`ArenaConstants.TankCollisionDamage`).

### ECM modes

Running ECM drains energy every tick the mode is active:

| Mode           | Cost / tick |
|----------------|-------------|
| Jam            | 0.5         |
| Spoof          | 0.8         |
| JamAndSpoof    | 1.3 (both)  |
| Burnthrough    | 0.3         |

If a tank has insufficient energy to sustain its requested ECM mode, the engine silently ignores the request for that tick.

---

## Energy Gains

### Shooting a tank

When your bullet hits an enemy, you recover energy equal to three times the bullet's power:

```
energyReturn = 3 × firePower
```

| Fire power | Energy returned |
|------------|-----------------|
| 0.1        | 0.3             |
| 1.0        | 3.0             |
| 3.0        | 9.0             |

This means aggressive, accurate shooting is self-sustaining: a tank that lands consistent hits bleeds the target while replenishing itself.

---

## Death Condition

The engine calls `CheckDeath` after every energy-reducing event. When `Energy <= 0`, the tank is marked dead (`IsAlive = false`, `DestroyedAtTick` is recorded) and removed from all further physics processing. The renderer displays a burning hulk at the tank's last position.

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

**Heavy bullets** hit harder and return more energy, but are easier to dodge at range.  
**Light bullets** travel faster and are harder to dodge, but deal minimal damage and return almost nothing.

A common tactic is to fire light bullets when the enemy is far (hard to lead) and switch to heavy power at close range or when a kill shot is available.

---

## Watching Energy from AI Code

```csharp
protected override void OnTick(TickEventArgs e)
{
    double myEnergy = e.MyState.Energy;

    // Desperate survival: stop ECM to save energy
    if (myEnergy < 20)
        SetEcmMode(EcmMode.Off);

    // Kill shot available: fire maximum power
    if (myEnergy >= 3.0 && targetInSights)
        SetFire(3.0);
}
```

`OnBulletHit` fires when your bullet connects, letting you confirm the energy return:

```csharp
protected override void OnBulletHit(BulletHitEventArgs e)
{
    // e.Bullet.EnergyReturn shows exactly how much energy was credited
    // e.VictimState.Energy shows the target's remaining energy
}
```

---

## Key Constants (ArenaConstants)

| Constant                  | Value | Notes                          |
|---------------------------|-------|--------------------------------|
| `TankStartEnergy`         | 100   | Starting energy each round     |
| `BulletMinPower`          | 0.1   | Minimum fire power             |
| `BulletMaxPower`          | 3.0   | Maximum fire power             |
| `TankCollisionDamage`     | 0.6   | Per-tank cost of body collision|
| `WallDamageFactor`        | 0.5   | Multiplier in wall damage formula |
| `WallDamageThreshold`     | 1.0   | Minimum velocity before wall damage |
| `EcmJamCostPerTick`       | 0.5   | Energy cost of Jam mode        |
| `EcmSpoofCostPerTick`     | 0.8   | Energy cost of Spoof mode      |
| `EcmBurnthroughCostPerTick` | 0.3 | Energy cost of Burnthrough mode |
