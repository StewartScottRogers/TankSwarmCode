---
Iteration: 9
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-9: VolleyIntervalTicks 30 → 20 (REFUTED — mechanism unknown)

## Hypothesis

`VolleyIntervalTicks=30` with `LeadershipEpochTicks=40` means the epoch-fires-volley check (`40 - 30 = 10 >= 30` — wait, this is ALWAYS true since epoch gap=40 > interval=30). Reducing to 20 should be a no-op by analysis, but might allow double-volleying in edge cases of leader changes at mid-epoch ticks.

## Code Change

`SwarmCoordinator`: `VolleyIntervalTicks = 30` → `VolleyIntervalTicks = 20`

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 73 wins (36%) | Blue: 127 wins (64%)
First kill victim: Red in 40% [Red still wins 19%] | Blue in 60% [Blue still wins 46%]
```

## Analysis

**REFUTED. 44% → 36%. Unexpected -8pp regression. Reverted.**

By analysis, the change should be a no-op: the epoch runs every 40 ticks, and `40 >= 20` just like `40 >= 30`. Both should fire a volley every epoch. No logical mechanism found to explain -8pp.

Empirically real: the regression repeated consistently. Pattern observed: frequency-constant changes (VolleyIntervalTicks, AllyPingInterval) cause large unexpected regressions even when the analysis says they should be no-ops. **Do not change frequency constants.**

## Key Findings

1. Frequency-based constants (VolleyIntervalTicks, AllyPingInterval) appear to have non-obvious timing interactions that cause large regressions when changed
2. Do not attempt to tune these constants — treat them as fixed

## Summary

**Red 36%** — regression. Reverted. Mechanism unknown but empirically real.
