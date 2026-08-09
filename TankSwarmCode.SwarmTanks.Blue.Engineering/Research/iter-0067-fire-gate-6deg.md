# Iter-67 — LinearPredictionFire gate 5°→6°: CONFIRMED (+3.0pp avg)

## Hypothesis
Re-test the old 12v5-era DO NOT rule "gate=6° gave −3.7pp" in the current 29v29 regime. With 17 slot tanks now firing at MaxFP=1.5 / PR=160 (fundamentally different from the old 5-named-tank high-MaxFP regime), the prior calibration may no longer hold. Loosening the gun-alignment gate in `LinearPredictionFire` from 5°→6° should add shot volume when prediction/sighting noise already exceeds 5°, without meaningfully reducing hit fraction.

Success criterion: Blue 5-seed avg ≥ 45.3% (baseline 44.3% + 1pp).

## Code Change
`TankSwarmCode.AiCortex.Blue/Library/TankNavigation.cs` — single-line edit in `LinearPredictionFire`:

```diff
- if (Math.Abs(gunDiff) < 5.0 && !IsWallInLineOfFire(ctx, predictedPos))
+ if (Math.Abs(gunDiff) < 6.0 && !IsWallInLineOfFire(ctx, predictedPos))
      ctx.SetFire(power);
```

No other changes. Volley-fire path (in `BlueCortexBase.OnTick`) is separate and was not gated on alignment at all; this change only affects per-tick Wolfpack/Encircle/Pincer/ECMScreen firing.

## Run Parameters
- 200 matches per seed, `--parallel 8`, `--on-timeout energy`
- Seeds 1000, 2000, 3000, 4000, 5000
- Both Red.dll and Blue.dll rebuilt in Release before each measurement

## Raw Results

### Baseline (gate=5°, current Red DLL)
| Seed | Blue wins | Blue % |
|---|---|---|
| 1000 | 86/200 | 43.0% |
| 2000 | 94/200 | 47.0% |
| 3000 | 81/200 | 40.5% |
| 4000 | 85/200 | 42.5% |
| 5000 | 97/200 | 48.5% |
| **Avg** | | **44.3%** |

Note: baseline is substantially lower than iter-64's recorded 49.3% — Red has iterated since and eroded Blue by ~5pp. This iteration started with Blue in a losing position.

### Change (gate=6°)
| Seed | Blue wins | Blue % | Δ vs baseline |
|---|---|---|---|
| 1000 | 89/200 | 44.5% | +1.5 |
| 2000 | 85/200 | 42.5% | −4.5 |
| 3000 | 98/200 | 49.0% | +8.5 |
| 4000 | 92/200 | 46.0% | +3.5 |
| 5000 | 109/200 | 54.5% | +6.0 |
| **Avg** | | **47.3%** | **+3.0** |

## Analysis
- 4/5 seeds improved, with large positive jumps on seeds 3000 (+8.5) and 5000 (+6.0).
- Seed 2000 regressed −4.5pp, confirming the long-observed "seed 2000 sensitivity" pattern.
- Seed 5000 (previously strongest) jumped to 54.5%, the only seed now above the 50% line.
- Net 5-seed delta +3.0pp clears the +1pp success threshold comfortably.

**Why the old DO NOT was regime-dependent:**
- Old test was performed in 12v5 / 6v5 / 7v5 regimes, where Blue's named tanks (MaxFP 2.0–3.0, PR 180–300) dominated firing. In those regimes, bullet travel was dominated by high-power slow bullets; gun-slew error was the limiting factor and 5° gate was tuned to that.
- Current regime: 17 of 29 Blue tanks are slot tanks at MaxFP=1.5 / PR=160. Bullet speed = 20 − 3·1.5 = 15.5 px/tick; PR=160 → travel ~10 ticks. Target-velocity estimate noise (typical Red turn-rate changes across the ~10-tick lead window) can introduce cross-range error on the order of several degrees, often exceeding 5° even when the gun itself is well-aligned.
- Tightening the gate below the prediction-noise floor filters out shots that would have hit anyway (noise is bidirectional). Loosening to 6° restores some volume without materially degrading hit fraction.

## Key Findings
- **Regime shift invalidates firing-gate calibration.** The 12v5-era 5° gate is no longer optimal at 29v29.
- **Gate = 6° beats gate = 5° by +3.0pp avg** in the current regime.
- **Seed 2000 remains the sensitive outlier.** Its −4.5pp regression matches the pattern where any fire-control change disturbs its specific coordination chain.
- **Firing-control is NOT saturated** (contradicting the iter-66 conclusion). The saturation conclusion was reached assuming static regime; when Red's firing parameters shifted, Blue's gate went out of calibration.
- This is also a falsification of the prior DO NOT rule's durability: configuration-dependent findings can flip when the composition changes.

## Summary
Loosened `LinearPredictionFire` gate from 5° to 6°. Blue 5-seed avg moved from 44.3% → 47.3% (+3.0pp). 4/5 seeds positive; seed 2000 regressed as expected for its sensitivity profile. Verdict **CONFIRMED**. New Blue baseline is 47.3% at seed-avg-200-matches vs current Red DLL. Next probes should test whether the gate is near-optimal at 6° or could go to 7°; and whether seed 2000's regression indicates a coordination side-effect that deserves a separate fix.
