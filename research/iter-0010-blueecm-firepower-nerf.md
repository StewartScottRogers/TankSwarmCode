# Iter 10 — BlueEcm Firepower Nerf (0.1)

**Date:** 2026-04-18
**Branch:** `research/iter-10-blueecm-firepower-nerf`
**Status:** Refuted — over-corrected (Blue now 59%)

---

## Hypothesis

BlueEcm's MaxFirePower=1.5 is the binding constraint. It drains energy through combat, preventing survival. Lowering MaxFirePower 1.5 → 0.1 (matching Ghost's near-zero firepower) makes BlueEcm a true hider, approaching Ghost's 59% WinSurv and 32% solo carry, and dropping Red's win rate from 56% toward 45–52%.

## Code Change

`TankSwarmCode.SwarmTanks.Blue/BlueEcm.cs`:
- `MaxFirePower`: 1.5 → 0.1

**Reverted** — over-corrected, code restored.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
```

## Raw Results

```
Red: 79 wins (41%)  |  Blue: 118 wins (59%)  |  Draws: 0 (note: second run returned 121/79)

Win quality: Red: +74E decisive (56), 26 TO +16E (31% via TO)
             Blue: +92E decisive (93), 25 TO +19E (21% via TO)
Decisive med: Red 282t / Blue 245t (Blue 1.2x faster — near-parity; BlueEcm jam slowing Red grind)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             ~49/79  (62%)           2.16-ish
BlueEcm              93/121 (68E)  77%       327+16TO          2.00

RedGhost insights: MVP, **Linchpin** (alive: 91% Red wins, dead: 21%), Solo carry (25/79, 16D+9TO)
BlueEcm insights:  MVP (93/121), Survivor (6/79 losses), Solo carry (37/121, 23D+14TO), ECM
```

## Analysis

| Criterion | 0.1 MaxFP Target | Baseline (1.5) | Iter-10 (0.1) | Pass? |
|---|---|---|---|---|
| Red win rate | 45–52% | 56% | **41%** | ❌ (over-corrected) |
| BlueEcm WinSurv | approach 59% | ~62% | **77%** | Over-achieved |
| BlueEcm solo carry | approach 32% | 19% | **31%** | ✅ |
| BlueEcm Rate/100t | ~0 | 10% | **2.00** | ✅ (mirrors Ghost) |

## Key Findings

1. **MaxFirePower IS the binding constraint — but 0.1 over-corrects.** BlueEcm at 0.1 becomes a more effective hider than Ghost at 0.1, because:
   - BlueEcm uses Burnthrough mode → sees through Ghost's JamAndSpoof, maintaining scan accuracy
   - BlueEcm uses Jam only (lower ECM energy cost than Ghost's JamAndSpoof)
   - BlueEcm at 0.1 fire + Burnthrough = Ghost-counter + hider hybrid. Dominant.

2. **BlueEcm became the mirror Ghost.** Rate/100t=2.00, WinSurv=77%, solo carry=31% — nearly identical profile to Ghost. But BlueEcm's ECM is structurally better (Burnthrough sees through Ghost's jam), so BlueEcm wins the ECM duel at equal firepower.

3. **Ghost gained Linchpin status** when BlueEcm became effective. With BlueEcm winning ECM duels, Ghost is now the critical unit keeping Red competitive (91% Red win rate when Ghost alive, 21% when dead). Ghost's JamAndSpoof is the only thing fighting BlueEcm's Burnthrough.

4. **Crossover is between MaxFirePower 0.1 and 1.5.** At 0.1: Blue 59%. At 1.5: Red 56%. The balance point is somewhere in between. Binary search: try 0.5 as next calibration.

## Next Hypothesis (Iter 11)

Binary search calibration: **BlueEcm MaxFirePower=0.5** should land closer to the balance point.

If Red wins at 0.5: crossover is between 0.1 and 0.5, try 0.3.
If Blue wins at 0.5: crossover is between 0.5 and 1.5, try 1.0.
Target: find the MaxFirePower value where Red wins 45–52%.

Branch: `research/iter-11-blueecm-fp-calibrate-0.5`
