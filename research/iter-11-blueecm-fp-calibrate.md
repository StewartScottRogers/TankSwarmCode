# Iter 11 — BlueEcm Firepower Calibration (Binary Search)

**Date:** 2026-04-18
**Branch:** `research/iter-11-blueecm-fp-calibrate-0.5`
**Status:** Confirmed — MaxFirePower=1.0 achieves balance

---

## Hypothesis

Binary search calibration: BlueEcm MaxFirePower between 0.1 (Blue 59%) and 1.5 (Red 56%) has a crossover to the 45–52% balance target. Starting with 0.5, then 1.0.

## Code Change (KEPT — confirmed balance fix)

`TankSwarmCode.SwarmTanks.Blue/BlueEcm.cs`:
- `MaxFirePower`: 1.5 → **1.0**

This is the confirmed balance fix. Code NOT reverted.

## Binary Search Results

| MaxFirePower | Red | Blue | Notes |
|---|---|---|---|
| 0.1 | 41% | 59% | BlueEcm = pure hider, beats Ghost's JamAndSpoof |
| 0.5 | 40% | 60% | Still Blue-dominated |
| **1.0** | **51%** | **49%** | ✅ **BALANCED** |
| 1.5 | 56% | 44% | Baseline (Red-dominated) |

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
Final value: BlueEcm MaxFirePower=1.0
```

## Full Results at MaxFirePower=1.0

```
Red: 102–103 wins (51%)  |  Blue: 97–98 wins (49%)  |  Draws: 0

Win quality: Red: +73E decisive (63), 39 TO +17E (38% via TO)
             Blue: +73E decisive (65), 33 TO +16E (33% via TO)
Decisive med: Red 333t / Blue 149t (Blue 2.2x faster)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             ~60/200               56/103 (67E)  54%  WinSurv
RedArrow             ~46/200               42/103         ...
BlueEcm              ~70/200               72/97 (29E)   74%  WinSurv  Rate~6.5

RedGhost insights: MVP (56/103), Survivor (8/97 losses), Solo carry (33/103, 26D+7TO), ECM
BlueEcm insights:  MVP (72/97), Linchpin (alive: 90%, dead: 21%), Survivor (8/103 losses),
                   Solo carry (33/97, 15D+18TO), ECM

Win combos (Red):
  33x Ghost       (32%) [26D+7TO]
  21x Arrow       (20%) [6D+15TO]
  11x Blade       (11%)
  10x Arrow+Ghost (10%)

Win combos (Blue):
  33x Ecm         (34%) [15D+18TO]  ← BlueEcm solo carry now mirrors Ghost!
```

## Analysis

| Criterion | Target | Baseline | Iter-11 (MaxFP=1.0) | Pass? |
|---|---|---|---|---|
| Red win rate | 45–52% | 56% | **51%** | ✅ |
| Ghost solo carry | unchanged | 32% | **32%** | ✅ |
| BlueEcm solo carry | approaching Ghost | 19% | **34%** | ✅ |
| Ghost MVP retained | — | 59% WinSurv | 54% WinSurv | ✅ |
| BlueEcm MVP | — | 62% WinSurv | **74% WinSurv** | ✅ |

## Key Findings

1. **Balance achieved at MaxFirePower=1.0.** The jump from 0.5 (Blue 60%) to 1.0 (Red 51%) is non-linear, suggesting a threshold effect where BlueEcm's combat energy drain at 1.0 equals the ECM durability advantage enough to let Ghost win more ECM duels.

2. **Both ECM hiders now carry ~33% of their team's wins.** Ghost: 33/103 = 32%. BlueEcm: 33/97 = 34%. The ECM duel is now genuinely symmetric in carry rate.

3. **BlueEcm gained Linchpin status at parity.** With proper energy balance, BlueEcm is Blue's Linchpin (90%/21%) — confirming it's carrying Blue correctly.

4. **Ghost WinSurv dropped slightly from 59% to 54%.** With BlueEcm better-calibrated and surviving more Red wins with burnthrough, Ghost's dominance is slightly reduced. Ghost still MVP but at a more reasonable level.

5. **Root cause chain fully traced:**
   - Ghost's MaxFirePower=0.1 + RetreatEnergyThreshold=40 makes it a hider
   - Ghost outlasts BlueEcm (1.5 fire drains energy, retreats at 35E)
   - Fix: Lower BlueEcm MaxFirePower from 1.5 → 1.0 
   - At 1.0: BlueEcm drains less energy per shot, survives longer, carries more wins
   - Result: 51%/49% — balanced

## Summary for Human Review

**This branch contains the confirmed balance fix:** `BlueEcm.MaxFirePower` lowered from `1.5` to `1.0`.

The fix narrows the hider-role energy asymmetry between Ghost (0.1 fire) and BlueEcm (1.0 fire, was 1.5). At 1.0, BlueEcm drains ~33% less energy per shot, allowing it to survive longer in matches and carry Blue wins more effectively — matching Ghost's structural carry role.

**Recommend merging this branch to master.**
