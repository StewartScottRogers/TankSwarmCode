---
Iteration: 7
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-7: Aggressive Encircle at Any Advantage (REFUTED)

## Hypothesis

The default Encircle threshold (`allyCount >= enemies.Count * 2 && allyCount >= 3`) requires a 2:1 ally advantage before triggering Encircle. This is overly conservative. Lowering to `allyCount >= enemies.Count && allyCount >= 2` (any numerical advantage) should trigger Encircle sooner, accelerating first-kill timing and improving decisive win rate from 72%.

## Code Change

`SwarmCoordinator.SelectStrategy`: changed Encircle threshold from
```csharp
if (allyCount >= enemies.Count * 2 && allyCount >= 3)
    return SwarmStrategy.Encircle;
```
to:
```csharp
if (allyCount >= enemies.Count && allyCount >= 2)
    return SwarmStrategy.Encircle;
```

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 88 wins (44%) | Blue: 112 wins (56%)
```

## Analysis

**REFUTED. 50% → 44%. Regression of -6pp. Reverted.**

Aggressive Encircle created faster decisive wins AND faster decisive losses — the orbiting formation exposes individual tanks to coordinated Blue fire before they reach optimal position. Encircling at 1:1 parity means each Red tank is isolated on its orbit arc and can be picked off. The 2:1 requirement exists for a reason: you need enough firepower density during the orbit approach to survive the transition.

## Key Findings

1. Encircle at 1:1 parity backfires — orbit approach is a vulnerable transition state
2. The 2:1 threshold is not conservative — it's the minimum to survive the maneuver
3. Faster strategy triggering ≠ faster wins; formation transitions create temporary weakness

## Summary

**Red 44%** — regression. Reverted to original Encircle threshold.
