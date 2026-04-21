---
Iteration: 11
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-11: Arrow MaxFirePower 1.5 → 2.0 (REFUTED)

## Hypothesis

Arrow at MaxFP=1.5 (bullet speed 15.5) is the lightest attacker. Increasing to 2.0 (bullet speed 14) boosts damage per hit by 33% at modest accuracy cost. With Ghost's ECM active, Blue's targeting is degraded, making bullet speed less critical.

## Code Change

`RedArrowCortex`: `MaxFirePower = 1.5` → `MaxFirePower = 2.0`

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 83 wins (42%) | Blue: 117 wins (58%)
Arrow Rate/100t: 2.95 (down from 3.30 at 1.5)
Blue close-out when Red gets first kill: 46% (up from 32% at baseline)
```

## Analysis

**REFUTED. 44% → 42%. Regression of -2pp. Reverted.**

Slower bullets reduced Arrow's hit rate (rate 2.95 vs 3.30). At PR=220, even the small speed reduction (15.5 → 14) meaningfully degraded accuracy. The 33% power increase did not compensate. Additionally, Blue's close-out rate worsened: Blue won 46% of matches after Red got first kill (vs 32% at baseline), suggesting Arrow was less effective at following up first kills.

## Key Findings

1. At PR=220 (long range), bullet speed matters significantly — don't increase MaxFP for Arrow
2. Arrow MaxFP=1.5 with speed=15.5 is correctly calibrated for its range
3. Close-out rate is sensitive to Arrow's effectiveness

## Summary

**Red 42%** — regression. Reverted Arrow MaxFP to 1.5.
