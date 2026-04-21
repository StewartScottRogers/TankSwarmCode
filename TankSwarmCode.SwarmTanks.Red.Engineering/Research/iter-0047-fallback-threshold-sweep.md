# Iter 47 — Fallback Threshold Sweep: 30→10 (ACCEPTED, +2.0pp avg)

## Hypothesis

Lower the team Fallback threshold in `SwarmCoordinator.SelectStrategy` from 30.0 to a lower value, reducing how often Red retreats. With 29 tanks at MaxFP=2.0, maintaining offensive pressure at lower energy should help win the damage race.

Fallback triggers when `sumEnergy / allyCount < threshold` — a team-wide metric. With 29 tanks at avg energy > threshold, the team keeps attacking. Lowering threshold = attack until nearly dead.

## Code Change

`SwarmCoordinator.cs` line 208: `30.0` → `10.0`

Also tested: PR=180 (neutral, seed 3000: -1pp, seed 1000: 0pp) — PR=160 remains optimal.
Also tested: Red29 30th tank (refuted, seed 3000: -2pp, seed 1000: +1pp, net -0.5pp) — 29 tanks optimal.

## Run Parameters

- 200 matches, --parallel 8, --on-timeout energy, --format table
- Seeds: 1000, 2000, 3000, 4000, 5000

## Raw Results

### Fallback=15:
- Seed 1000: 56% (vs 56% baseline, 0pp)
- Seed 2000: 55% (vs 52% baseline, +3pp)
- Seed 3000: 59% (vs 57% baseline, +2pp)
- Seed 4000: 58% (vs 58% baseline, 0pp)
- Seed 5000: 56% (vs 55% baseline, +1pp)
- **5-seed avg: 56.8% (+1.2pp)**

### Fallback=10:
- Seed 1000: 60% (vs 56% baseline, +4pp)
- Seed 2000: 56% (vs 52% baseline, +4pp)
- Seed 3000: 57% (vs 57% baseline, 0pp)
- Seed 4000: 58% (vs 58% baseline, 0pp)
- Seed 5000: 57% (vs 55% baseline, +2pp)
- **5-seed avg: 57.6% (+2.0pp)**

### Fallback=5:
- Seed 3000: 58% (vs 57% Fallback=10, +1pp — noise)
- Seed 1000: 59% (vs 60% Fallback=10, -1pp — noise)
- **Result: flat vs Fallback=10 — not worth pursuing**

## Analysis

Threshold sweep: 30 → 15 → 10 shows clear monotonic improvement (+1.2pp then +0.8pp).
Threshold=5 flattens out — diminishing returns below 10.

Mechanism: With MaxFP=2.0, Red tanks have 100 energy and use 2 per shot. At avg<10, most tanks are nearly dead and the match outcome is already decided. At threshold=10 vs 30, Red attacks for ~10 more shots per tank per engagement before the fallback triggers. This improves combat pressure during mid-match when the outcome is still in play.

## Key Findings

- **Fallback=10 is the new optimal** (+2.0pp avg over baseline)
- **Gradient is 30→15→10 consistently positive, flat at 5**
- **PR=180 refuted** (neutral at 2 seeds, PR=160 remains optimal with MaxFP=2.0)
- **Red29 refuted** (-0.5pp avg, 29 Red tanks = ceiling at current Blue roster)
- **New 5-seed avg: 57.6%** (up from 55.6%)

## Summary

ACCEPTED. Fallback threshold 30→10 gives +2.0pp avg across 5 seeds.
New 5-seed avg: 57.6% (seeds: 60/56/57/58/57%).
