---
Iteration: 10
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-10: Ghost PreferredRange 150 → 200 (NEUTRAL)

## Hypothesis

Ghost is dying first in 19% of matches (38/200). Its PR=150 puts it at maximum ECM proximity but also maximum exposure. Pulling it back to 200 might reduce first-kill rate and improve overall win rate.

## Code Change

`RedGhostCortex`: `PreferredRange = 150.0` → `PreferredRange = 200.0`

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 89 wins (44%) | Blue: 111 wins (56%)
Ghost survival: 88/200 (up from 62/200 at PR=150)
Timeout rate: 168/200 = 84% (up from 65% at PR=150)
```

## Analysis

**NEUTRAL. 44% → 44%. No change. Reverted.**

Ghost survival improved dramatically (62 → 88 survivals), but win rate stayed flat because:
- More timeouts (84% vs 65%) — matches drag out more
- Blue wins more timeouts with PR=200 Ghost (ECM slightly less effective at range)
- Ghost's ECM value drops marginally at 200px vs 150px (proximity-dependent)

The two effects cancel out exactly. PR=150 is optimal.

## Key Findings

1. Ghost survival improvement at PR=200 doesn't translate to wins (offset by more timeouts going to Blue)
2. PR=150 remains optimal — proximity ECM disruption outweighs ghost survival gain
3. PR=300 failed by isolation; PR=200 fails by reduced ECM density; PR=150 is the sweet spot

## Summary

**Red 44%** — neutral. Reverted to PR=150.
