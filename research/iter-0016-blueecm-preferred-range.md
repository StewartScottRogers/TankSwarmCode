# Iteration 16 — BlueEcm PreferredRange=80: Standoff vs Close-Quarters

**Branch:** `research/iter-000016-blueecm-preferred-range`
**Date:** 2026-04-18

---

## Hypothesis

BlueEcm's Linchpin role at MaxFP=1.0 comes from PreferredRange=150 (standoff positioning). Moving to PreferredRange=80 (close-quarters, like combat tanks) should weaken the Linchpin pattern and cause Blue to lose ground relative to the 49% baseline.

**Success criteria:** Linchpin label weakens; Blue win rate drops toward 45% or below.

---

## Code Change

```
BlueEcm MaxFirePower:   1.5 → 1.0 (iter-11 fix baseline, for clean comparison)
BlueEcm PreferredRange: 150 → 80  (probe)
```

---

## Run Parameters

- Batch: 200 matches
- Seed: 1000
- Arena: 800×600 (default)
- Timeout policy: `--on-timeout energy`

---

## Raw Results

```
Red: 89 wins (44%)  |  Blue: 111 wins (56%)  |  Draws: 0

Win quality: Red: +70E decisive (57), 32 TO +19E (35% via TO)
             Blue: +66E decisive (78), 33 TO +15E (29% via TO)
Decisive med: Red 353t / Blue 178t (Blue 2.0x faster)

Per-tank:
  RedGhost     51/200 surv  44/89  WinSurv (70E)  Rate 2.67
  RedArrow     40/200 surv  32/89  WinSurv (21E)  Rate 17.32
  RedHammer    32/200 surv  22/89  WinSurv  (9E)  Rate 16.78
  RedBlade     28/200 surv  20/89  WinSurv (12E)  Rate 18.98

  BlueEcm      84/200 surv  74/111 WinSurv (39E)  Rate 8.46
  BlueGuard    40/200 surv  35/111 WinSurv (25E)  Rate 18.41
  BlueRush     44/200 surv  32/111 WinSurv (16E)  Rate 15.39
  BlueSharp    40/200 surv  38/111 WinSurv (11E)  Rate 18.22
  BlueStrike   39/200 surv  35/111 WinSurv (16E)  Rate 16.30

Insights:
  RedGhost:  MVP (44/89), Survivor, Solo carry 25/89 (17D+8TO), ECM

  BlueEcm:   MVP (74/111), Survivor, Solo carry 29/111 (15D+14TO), ECM
  BlueGuard: Top attacker, Glass cannon, Survivor
```

---

## Analysis

### Balance: REFUTED prediction direction
Red 44% / Blue 56% — Blue became MORE dominant than the iter-11 baseline (49%). The hypothesis predicted Blue would lose ground; instead Blue gained 7pp.

### Linchpin: CONFIRMED weakened
BlueEcm has no Linchpin label — confirmed as expected. At PreferredRange=80, BlueEcm is an aggressive close-range attacker, not a survival anchor. The Linchpin pattern at PreferredRange=150 is real, but removing it makes Blue stronger, not weaker.

### What actually happened
Moving BlueEcm to close-quarters (80) increased its combat contribution:
- Rate 8.46 (vs ~7.4 at PreferredRange=150 from iter-15)
- Blue decisive wins: 78 (vs 64 in iter-15) — more fast kills
- Decisive med 178t (vs 149t in iter-15) — slightly slower but still commanding tempo

BlueEcm at PreferredRange=80 joins the Blue corps attack line rather than hanging back. This adds firepower that Blue didn't need and tips balance toward Blue.

### PreferredRange=150 serves double duty
The standoff positioning at PreferredRange=150 both:
1. Creates the Linchpin survival pattern (BlueEcm outlives teammates)
2. Limits BlueEcm's raw firepower contribution (it hangs back)

Removing it increases aggression AND kills the Linchpin role. These are two sides of the same effect.

### Comparison table

| Config                        | Red%  | Blue% | BlueEcm Linchpin? |
|-------------------------------|-------|-------|-------------------|
| MaxFP=1.0, PR=150 (iter-11)   | 51%   | 49%   | Yes               |
| MaxFP=1.0, PR=150, NoECM (15) | 52%   | 48%   | Yes               |
| MaxFP=1.0, PR=80 (iter-16)    | 44%   | 56%   | No                |
| MaxFP=0.75 (iter-14)          | 41%   | 58%   | No                |
| MaxFP=1.5 baseline            | 56%   | 44%   | Yes               |

PreferredRange=80 at MaxFP=1.0 sits alongside the 0.75 regime — Blue dominant, no Linchpin.

---

## Key Findings

1. **PreferredRange=150 IS what creates the Linchpin pattern** — confirmed. Removing it eliminates Linchpin status.
2. **Balance moves the wrong direction** — Blue gains ground (56%) instead of losing it. The standoff range limits BlueEcm's aggression, which is part of why MaxFP=1.0 + PR=150 is the balanced config.
3. **Linchpin ≠ stronger Blue** — the Linchpin survival role at PR=150 co-exists with balance; aggressive close-range BlueEcm (PR=80) tips Blue over without creating Linchpin.

---

## Summary

**Hypothesis PARTIALLY CONFIRMED, PARTIALLY REFUTED.** Linchpin weakened as predicted (✅), but Blue gained ground instead of losing it (❌). PreferredRange=150 is the mechanism behind Linchpin — but it also acts as a firepower limiter that keeps Blue in balance. Code reverted — PR=80 is a probe, not a fix.

**Next hypothesis:** RetreatEnergyThreshold=35 (BlueEcm retreats earlier than combat tanks at 20–25) may be the other survival driver. Test: BlueEcm with RetreatEnergyThreshold=20 at MaxFP=1.0, PreferredRange=150 — expect survival to drop, Linchpin to weaken, and balance to shift toward Red.
