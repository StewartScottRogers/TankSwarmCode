---
Iteration: 2
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-2: BlueEcm Priority Targeting (REFUTED)

## Hypothesis

BlueEcm was Blue's Linchpin in iter-1 (alive: 77%, dead: 22%). Hypothesis: explicitly prioritizing BlueEcm as the kill target in SwarmCoordinator would kill it more often and push Red win rate above 50%.

## Code Change

`SwarmCoordinator.RunEpochLogic`: changed target selection from `OrderBy(c => c.Energy)` to `OrderBy(c => c.Name == "BlueEcm" ? 0 : 1).ThenBy(c => c.Energy)`.

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8
- `--on-timeout energy`, `--format table`

## Raw Results

```
Red: 63 wins (32%) | Blue: 137 wins (68%)

Tank                   WinSurv    Rate/100t
RedHammer               52/63       2.40
RedArrow                52/63       2.27
RedBlade                53/63       3.14
RedGhost                35/63       0.00

BlueSharp              122/137      1.69  ← MVP
BlueGuard              120/137      2.81  ← Top attacker
BlueEcm                112/137      1.09
```

## Analysis

**REFUTED. Catastrophic regression: 50% → 32%. Change reverted.**

The Linchpin correlation from iter-1 was observational, not causal. By forcing BlueEcm targeting:
1. Red chases a healthy BlueEcm while other Blue tanks kill Red — disrupts kill chains
2. "Weakest first" naturally killed BlueEcm when it happened to be weakest; forcing it was wrong
3. BlueSharp returns as dominant (MVP 89%) with no Linchpin for Blue anymore
4. Red is killing BlueEcm first in only 21 matches (vs 27 in iter-1) — less effective despite priority

The "weakest first" target selection creates natural kill chains. Any deviation costs winnable matches.

## Key Findings

1. Never hardcode enemy name targeting — battlefield energy state drives kill priority
2. Linchpin correlations are observational: the cause was "weakest first" kill chains, not BlueEcm's special role
3. Reverting to iter-1 state (proactive ECM + weakest first) is the correct baseline

## Summary

**Red 32%** — massive regression. Reverted. Baseline restored to proactive ECM only (iter-1).
