---
Iteration: 8
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-8: ECM-Conditional Encircle Threshold (REFUTED)

## Hypothesis

If Ghost (ECM, slot=3) is alive and jamming, Red has an information advantage — Blue tanks have 50% scan drops and ghost contacts. In that state, Encircle only requires a 1-tank advantage (`allyCount > enemies.Count`) rather than 2:1. This uses Ghost's jamming as a force multiplier to lower the safe threshold for Encircle maneuvers.

## Code Change

`SwarmCoordinator.SelectStrategy`: replaced fixed Encircle threshold with ECM-conditional:
```csharp
bool ecmAlive = AllyEntryMap.Values.Any(e => e.Slot == 3 && ctx.Arena.TickNumber - e.LastSeen < AllyStaleTicks);
bool encircleThreshold = ecmAlive ? allyCount > enemies.Count : allyCount >= enemies.Count * 2;
if (encircleThreshold && allyCount >= 2)
    return SwarmStrategy.Encircle;
```

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 92 wins (46%) | Blue: 108 wins (54%)
```

## Analysis

**REFUTED. 50% → 46%. Regression of -4pp. Reverted.**

Better than iter-7 (-6pp) but still negative. Ghost's JamAndSpoof cuts Blue's scan accuracy but doesn't prevent Blue tanks from targeting and firing on orbiting Red tanks. The orbit approach is still a vulnerable transition even with ECM cover — the 50% scan drop doesn't prevent Blue from tracking Red by position, only by radar. Additionally, while ecmAlive is true, Red triggers Encircle too early (3v3, 4v4 situations) before Red has the firepower advantage needed to survive the orbit.

## Key Findings

1. ECM jamming doesn't protect against positional targeting during Encircle transition
2. Lowering the Encircle threshold even with a conditional is net-negative
3. Original `allyCount >= enemies.Count * 2 && allyCount >= 3` appears to be correctly calibrated

## Summary

**Red 46%** — regression. Reverted to original Encircle threshold (`allyCount >= enemies.Count * 2 && allyCount >= 3`).
