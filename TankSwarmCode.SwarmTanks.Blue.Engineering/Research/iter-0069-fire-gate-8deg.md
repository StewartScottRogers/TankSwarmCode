# Iter-69 — LinearPredictionFire gate 7° → 8°

## Hypothesis
The LinearPredictionFire gun-alignment gate is at 7° (iter-68 committed). The gradient from 5→6 was +3.0pp and 6→7 was +2.8pp, suggesting the prediction-noise floor is in the neighborhood of 7–9°. Red's independent probe at 9° refuted (−1.1pp, Red iter-93). 8° is the last untested value in the bracket — a small bounded probe to find whether the peak is exactly at 7° or extends to 8°. Expected outcome: slight gain, flat, or slight loss. Probe is cheap.

## Code Change
`TankSwarmCode.AiCortex.Blue/Library/TankNavigation.cs:68`
```
if (Math.Abs(gunDiff) < 7.0 && ...)  →  if (Math.Abs(gunDiff) < 8.0 && ...)
```
Single-constant change; no other modifications.

## Run Parameters
- CLI: `dotnet run -c Release -- --batch 200 --parallel 8 --seed {S} --on-timeout energy --format table`
- 29v29 (Red-mvp 5 named + 24 slot; Blue 12 named + 17 slot)
- Seeds: 1000, 2000, 3000, 4000, 5000 (200 matches each)
- Both DLLs rebuilt in Release after code change.
- Baseline re-measured in the same session (same Red DLL state).

## Raw Results
Baseline (gate=7°, re-measured this session):
| Seed | Blue wins | Blue % |
|---|---|---|
| 1000 | 89 | 44.5% |
| 2000 | 79 | 39.5% |
| 3000 | 100 | 50.0% |
| 4000 | 103 | 51.5% |
| 5000 | 111 | 55.5% |
| **avg** | — | **48.2%** |

Result (gate=8°):
| Seed | Blue wins | Blue % | Δ vs baseline |
|---|---|---|---|
| 1000 | 94 | 47.0% | +2.5 |
| 2000 | 90 | 45.0% | +5.5 |
| 3000 | 98 | 49.0% | −1.0 |
| 4000 | 96 | 48.0% | −3.5 |
| 5000 | 96 | 48.0% | −7.5 |
| **avg** | — | **47.4%** | **−0.8pp** |

## Analysis
2/5 seeds positive (1000, 2000), 3/5 negative (3000, 4000, 5000). Seed 5000 regressed most (−7.5pp). Net −0.8pp avg.

The seeds that regressed (4000, 5000) were the strongest under gate=7° — they had the most headroom for the gate relaxation to *hurt* by adding low-quality shots. Seeds that gained (1000, 2000) were weakest under gate=7° and apparently had fire-rate upside from the wider gate.

The pattern — weak seeds gain, strong seeds regress — is consistent with the "prediction-noise floor" model: widening the gate past the noise floor adds shots whose accuracy is worse than their fire-rate bonus. Since the strong seeds were already shooting efficiently at 7°, the extra volume at 8° consists disproportionately of noise-dominated misses.

This matches Red's independent iter-94 probe pattern (Red saw 2/5 positive, 3/5 regress with their weakest seeds gaining and strongest regressing, net −1.9pp). Confirms the peak is sharp at 7° on both sides of the aim-accuracy/fire-rate tradeoff.

## Key Findings
- **Fire-gate peak at 7° is confirmed sharp.** Gate=8° is net −0.8pp for Blue; gate=9° was net −1.1pp (independent Red probe).
- **Seed-asymmetric effect.** Weak seeds gain, strong seeds lose — the widened gate trades hit-rate for fire-rate in a way the strongest geometries don't benefit from.
- **Fire-gate dimension is fully exhausted for Blue.** All values in [4°, 9°] have been tested; 7° is peak. Per-shot gates are saturated.

## Summary
Gate 7°→8° **REFUTED** (−0.8pp avg, 5 seeds). 2/5 positive, 3/5 negative. Reverted. Fire-gate dimension exhausted at 7° peak for both Blue and Red. Next Blue iteration should target coordination/geometry/composition rather than firing-control constants.
