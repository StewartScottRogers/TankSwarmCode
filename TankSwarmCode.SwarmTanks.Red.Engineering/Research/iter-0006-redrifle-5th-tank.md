---
Iteration: 6
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-6: RedRifle 5th Tank (REFUTED)

## Hypothesis

Red has 4 tanks vs Blue's 5 tanks — a 20% numerical disadvantage. Adding a 5th Red tank (RedRifle: MaxFP=3.0, PR=180, slot=4) with a parameterless constructor will reach parity and push Red above 50%.

## Code Change

- Added `RedRifleCortex.cs` (MaxFP=3.0, PR=180, slot=4)
- Added `RedRifle.cs` tank shell (parameterless constructor, SwarmId=1)
- Registered in CortexFactory

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 78 wins (39%) | Blue: 122 wins (61%)
Red Arrow rate dropped: 2.97 → 0.81 (Arrow became hider)
Red killed first in 60% of matches (up from 38%)
5-tank full team wins: 48/78 (62%)
```

## Analysis

**REFUTED. 50% → 39%. Adding 5th tank hurt. Reverted.**

Counterintuitively, going from 4 to 5 tanks made things worse:
1. Arrow's rate dropped from 2.97 to 0.81 — the 5th tank disrupted Arrow's combat positioning
2. Red gets killed first in 60% of matches (up from 38%) — more tanks = more targets for Blue
3. The swarm coordinator isn't designed for 5-tank Wolfpack — tanks likely interfere with each other's navigation
4. 10 coin-flip matches (<5E margin) in iter-6 vs only 8 in iter-1 — more razor-thin results, not cleaner wins

Ghost's proactive ECM was tuned for a 4-tank team. The 5th tank adds coordination complexity that outweighs the firepower gain.

## Key Findings

1. 4-tank Red + ECM is better than 5-tank Red + ECM
2. ECM-based strategy benefits from a tight, small team that moves coherently
3. Adding tanks dilutes swarm coordination in the Wolfpack algorithm
4. Don't add tanks without validating swarm coordination can handle them

## Summary

**Red 39%** — regression. RedRifle removed. Red stays at 4 tanks.
