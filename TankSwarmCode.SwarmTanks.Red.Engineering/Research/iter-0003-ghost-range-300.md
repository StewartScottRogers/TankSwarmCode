---
Iteration: 3
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-3: Ghost PreferredRange 150→300 (REFUTED)

## Hypothesis

Ghost is dying first in 28/200 matches. Increasing PreferredRange from 150→300 will keep Ghost out of the frontline, reduce its first-kill exposure, and maintain ECM jamming longer.

## Code Change

`RedGhostCortex.TankConfiguration.PreferredRange`: 150.0 → 300.0

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 74 wins (37%) | Blue: 126 wins (63%)

RedGhost first-kill count: 34 (up from 28 in iter-1)
RedGhost survival: 80/200 (vs 97/200 in iter-1)
```

## Analysis

**REFUTED. Red fell from 50% to 37%. Reverted.**

Ghost dying MORE often at 300px range, not less. Three reasons:
1. At 300px, Ghost is isolated from the team and easier to pick off alone
2. ECM jamming requires proximity — at 300px, Blue's targeting is less disrupted
3. Without ECM near the fight, Blue fights more effectively and Red's decisive wins collapse

Insight: Ghost must stay close to the battle (150px) for JamAndSpoof ECM to work. The effect is proximity-dependent — the closer Ghost is to enemies, the more their targeting is disrupted.

## Key Findings

1. ECM effectiveness is proximity-dependent: 150px > 300px
2. Isolation kills more tanks than proximity — the team provides mutual cover
3. PreferredRange 150 is the right value for Ghost

## Summary

**Red 37%** — regression. Reverted. Ghost PreferredRange stays at 150.
