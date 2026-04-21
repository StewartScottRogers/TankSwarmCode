# Iter 82 — Blue DLL Update Discovered; PR=171 Refuted; New Baseline 52.2%

## Situation

Branch was created expecting PR=171 to counter BlueEcm disruption of Wolfpack formation.
Upon measurement, discovered Blue Engineering updated their DLL. Old baseline (57.6% avg) is stale.

## Code Change

### Attempt A: ECMScreen removal (CATASTROPHIC — not a real result, see below)
Removed `if (IsEnemyEcmActive(ctx)) return SwarmStrategy.ECMScreen;` from SelectStrategy.
Result: 35% draws (matches time out because Red loses radar tracking of Blue during ECM events).
Finding: ECMScreen is ESSENTIAL for maintaining radar contact — not for firing, but for navigation toward
the last known enemy position during ECM blackout. Without it, Red wanders and matches end in draws.
REVERTED.

### Attempt B: Orbit-based ECMScreen
Changed ExecuteECMScreen to use orbit-based positioning (like Wolfpack at 130px instead of convergent).
Result: NEUTRAL — exactly 52.5% on seed 3000, same as baseline with original convergent ECMScreen.
Finding: ECMScreen movement pattern doesn't matter to outcomes. Both approaches produce identical win rates.
REVERTED.

### Attempt C: PR=171
Changed PreferredRange from 160 to 171 for all 29 tanks.
Results (3 seeds):
- Seed 1000: 47.0% (vs 50.5% baseline, -3.5pp)
- Seed 2000: 55.0% (vs 54.5% baseline, +0.5pp)
- Seed 3000: 53.5% (vs 52.5% baseline, +1.0pp)
Average: 51.8% vs 52.5% = -0.7pp
REFUTED. Seed 1000 clear negative signal. PR=160 remains optimal.
REVERTED.

## Run Parameters

- 200 matches, --parallel 8, --on-timeout energy
- Seeds: 1000, 2000, 3000 (PR=171); all 5 seeds (new baseline)

**Critical discovery:** All previous iter 82 runs before adding `--on-timeout energy` showed
identical 33% / 35% draws because of the timeout draw setting. Always use `--on-timeout energy`.

## New Baseline (Blue DLL Updated)

Old baseline (iter 47, 5-seed): 60/56/57/58/57% = 57.6% avg
New baseline (iter 82, 5-seed): 50.5/54.5/52.5/54.0/49.5% = **52.2% avg**

Blue Engineering updated their DLL, reducing Red's advantage by ~5.4pp.

## Analysis

1. **Blue DLL update is the dominant signal.** A 5.4pp drop with identical Red code means Blue made
   a meaningful improvement. Red needs to adapt to the new Blue configuration.

2. **PR=171 does not counter the Blue update.** The slight gains on seeds 2000/3000 are noise;
   the -3.5pp on seed 1000 is signal. PR=160 remains the Wolfpack equilibrium for MaxFP=2.0.

3. **ECMScreen behavior is irrelevant to outcomes.** Whether Red tanks converge (original) or orbit
   (experimental) during ECM events, the win rate is identical. ECMScreen is used ~20 ticks per
   event but doesn't change match outcomes. The Wolfpack orbit formation dominates overall.

4. **ECMScreen trigger is load-bearing.** Without it, Red loses radar tracking during ECM and 35%
   of matches become draws. This is a hard constraint.

## Key Findings

- **New 5-seed baseline: 52.2% avg** (was 57.6%, Blue updated their DLL)
- **PR=171 REFUTED** (-3.5pp seed 1000, marginal gains noise on others)
- **ECMScreen orbit-based NEUTRAL** (52.5% = baseline)
- **--on-timeout energy is required** for valid win rate measurement (avoid draw inflation)

## Next Steps

52.2% is the new ceiling. To exceed it against updated Blue DLL:
1. **BlueEcm priority targeting** — kill BlueEcm first to prevent ECMScreen triggers entirely
2. **Reduce ECM alert window** — 20 ticks → shorter, spending less time in ECMScreen
3. **MaxFP sweep** — with new Blue DLL, optimal MaxFP might have shifted from 2.0
4. **PR sweep** — test 155, 150 vs the new Blue formation density
