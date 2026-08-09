# Iter 14 — BlueEcm MaxFP Curve (0.75 probe)

**Date:** 2026-04-18
**Branch:** `research/iter-000014-blueecm-fp-curve`
**Status:** Confirmed — non-linearity is a sharp threshold near MaxFP=1.0. Code reverted (0.75 not a fix).

---

## Hypothesis

The MaxFP=0.5→1.0 range contains a non-linear threshold. Testing MaxFP=0.75 (midpoint) will reveal whether balance improves gradually (linear curve) or stays Blue-dominant until a sharp crossover near 1.0.

**Success criteria:**
- Red 45–55%: flat/broad curve (0.75 is already balanced)
- Red 56–65%: still Red-dominated (threshold above 0.75, gradual Red-side)
- Blue 55–60%: Blue-leaning but improving (gradual curve with crossover between 0.75–1.0)
- Blue >57%: confirms sharp threshold — 0.75 is still deep Blue territory

## Code Change

`BlueEcm.cs`: MaxFirePower 1.5 → 0.75 (probe)

**Reverted** — 0.75 makes balance worse, not better.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
BlueEcm MaxFirePower=0.75
```

## Baseline (master, MaxFP=1.5, same run)

```
Red: 113 wins (56%)  |  Blue: 87 wins (44%)
RedGhost: MVP, Solo carry (36/113, 25D+11TO), ECM
BlueEcm: MVP (54/87), Linchpin (alive:84%, dead:24%), ECM
```

## Raw Results (MaxFP=0.75)

```
Red: 82 wins (41%)  |  Blue: 117 wins (58%)  |  Draws: 1

Win quality: Red: +74E decisive (55), 27 TO +17E (32% via TO)
             Blue: +72E decisive (80), 37 TO +17E (31% via TO)
Decisive med: 219t — Red 382t / Blue 157t (Blue 2.4x faster)
Avg first kill: tick 24

Insights:
  RedGhost   MVP (48/82), Linchpin (alive:89%, dead:23%), Solo carry (30/82, 21D+9TO), ECM
  RedArrow   Survivor (11/118 losses)
  RedBlade   Top attacker (rate 19.00), Glass cannon (21/82), Survivor (9/118 losses)
  RedHammer  Expendable (14/82), Survivor (12/118 losses)
  BlueEcm    MVP (83/117), Survivor (5/83 losses), Solo carry (32/117, 16D+16TO), ECM
  BlueGuard  Top attacker (rate 19.15), Glass cannon (36/117), Survivor (8/83 losses)
  BlueRush   Expendable (36/117), Survivor (6/83 losses)
  BlueStrike Expendable (34/117), Survivor (6/83 losses)
  BlueSharp  Expendable (31/117), Primary target (12% first kill)
```

## Analysis

### Cross-value table

| MaxFP | Red%  | Blue% | BlueEcm Linchpin? | BlueEcm WinSurv |
|-------|-------|-------|-------------------|-----------------|
| 0.1   | 41%   | 59%   | No                | 93/121 = 77%    |
| 0.5   | 40%   | 60%   | No                | ~75% (est)      |
| 0.75  | 41%   | 58%   | No                | 83/117 = 71%    |
| 1.0   | 51%   | 49%   | Yes (iter-11)     | ~90%            |
| 1.5   | 56%   | 44%   | Yes (baseline)    | 54/87 = 62%     |

### Key observation: the curve is flat, then jumps

The 0.1, 0.5, and 0.75 datapoints all cluster in the 40–42% Red range (Blue dominant, 57–60%). The jump to 51% at MaxFP=1.0 is the only significant transition in the entire 0.1–1.5 range. The non-linearity is **not** gradual — it is a single sharp threshold between 0.75 and 1.0.

### Linchpin shift explains the threshold

At MaxFP=0.1/0.5/0.75: BlueEcm is NOT Linchpin. BlueEcm survives in Blue wins passively (71% WinSurv at 0.75) but is not critical to Blue winning. The Blue combat corps (Guard, Rush, Strike, Sharp) wins without needing BlueEcm. BlueEcm's lower MaxFP drains less energy but also deals less damage, allowing Red's Ghost to dominate the ECM duel.

At MaxFP=1.0: BlueEcm becomes Linchpin. The moderate firepower puts BlueEcm in a "sweet spot" where it:
1. Drains enough energy to matter in combat (more than 0.75)
2. Doesn't over-drain like at 1.5 (where it dies in Red wins)
3. Maintains ECM effectiveness long enough to carry Blue wins

At MaxFP=1.5: BlueEcm is again Linchpin, but Red's ghost dominates because BlueEcm over-drains and dies in Red wins (44% WinSurv).

### Ghost behavior at 0.75

RedGhost is Linchpin at 0.75 (alive:89%/dead:23%) — Ghost is now critical to Red competing. This mirrors iter-10 where BlueEcm at 0.1 made Ghost the Linchpin. The pattern holds: when BlueEcm has lower MaxFP, Ghost's ECM dominance becomes structurally critical for Red.

Interestingly, RedGhost's solo carry dropped from 36/113 (32%) at baseline to 30/82 (37%) at MaxFP=0.75 — Ghost carries a LARGER fraction of Red's fewer wins, further confirming its Linchpin status when Blue is dominant.

## Key Findings

1. **Threshold confirmed.** MaxFP=0.75 gives Blue 58% — nearly identical to 0.5 (60%) and 0.1 (59%). The entire 0.1–0.75 range is one flat Blue-dominant regime.
2. **Sharp crossover between 0.75 and 1.0.** The balance shifts from Blue 58% to Red 51% in just a 0.25 MaxFP increment. This is not gradual.
3. **Linchpin phase transition.** BlueEcm is NOT Linchpin at ≤0.75, but IS Linchpin at 1.0+. The Linchpin assignment tracks the balance crossover exactly.
4. **MaxFP=1.0 remains the unique sweet spot.** Lower values keep BlueEcm passive (survives more but wins less effectively), higher values over-drain it. The threshold is narrower than expected.

## Summary

**Confirmed — non-linearity is a sharp threshold. The 0.1–0.75 range is a flat Blue-dominant regime; the crossover to balance happens between 0.75 and 1.0.**

The iter-11 fix (MaxFP=1.0) is uniquely correct. No intermediate value in the 0.1–0.75 range achieves balance. Code reverted.

**Next hypothesis:** BlueEcm's role at MaxFP=1.0 is structural (survival/carry), not ECM-dependent — mirrors iter-5 for Ghost. Testing BlueEcm HasEcm=false with MaxFP=1.0 should maintain 45–55% balance if the role is structural.
