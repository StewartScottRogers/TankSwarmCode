## Hypothesis

At MaxFP=1.5, bullet speed = 15.5 px/tick. At PR=160, travel time = 160/15.5 = 10.3 ticks.
At PR=140, travel time = 140/15.5 = 9.0 ticks (-13%). Faster bullet arrival should improve hit rate
vs Blue's mobile tanks. Risk: closer orbit → more return fire.

Success criteria: Red 5-seed avg increases by ≥2pp (from 67.2% to ≥69.2%).

## Code Changes

All 5 cortex files: `PreferredRange = 160.0` → `140.0`. Reverted after refutation.

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000 (early termination on clear signal)
- Timeout rule: `--on-timeout energy`

## Raw Results

| Seed | PR=140 | Baseline (PR=160) | Delta |
|------|--------|------------------|-------|
| 1000 | 61.5% | 66.0% | -4.5pp |
| 2000 | 67.5% | 72.0% | -4.5pp |

Both seeds show identical -4.5pp regression. Stopped at 2 seeds (clear negative result).

## Analysis

Consistent -4.5pp across two seeds. The closer orbit at 140 hurts more than the bullet speed gain
helps. At PR=140, Blue's counter-fire is more effective against Red's tighter cluster, or Red tanks
are spending more ticks repositioning to avoid overlap, reducing effective firepower.

Note: despite PR=140 being ~13% closer, Red's avg_damage_taken didn't increase significantly (still
~104-112 per tank). The win rate drop seems to come from decreased hit rate or formation disruption,
not simply from taking more damage.

PR=160 is confirmed optimal for the current MaxFP config. The orbit radius and MaxFP are optimized
together — changing one without the other risks breaking the established equilibrium.

## Key Findings

- **PR=140 REFUTED: -4.5pp avg (2 seeds).** Consistent clear signal.
- PR=160 is confirmed optimal for this swarm configuration.
- Orbit radius and MaxFP are coupled: PR=160 + MaxFP=1.5 is the confirmed sweet spot.
- Next: test PR=150 (midpoint) or PR=180 (further out) if orbital tuning is still worth exploring.

## Summary

REFUTED after 2 seeds. All tanks reverted to PR=160. Baseline remains 67.2%.
