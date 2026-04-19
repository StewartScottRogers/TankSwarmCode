# Iteration 15 — BlueEcm HasEcm=false: Structural Carry Test

**Branch:** `research/iter-000015-blueecm-noecm-structural`
**Date:** 2026-04-18

---

## Hypothesis

BlueEcm's carry role at MaxFP=1.0 is structural (survival-driven), not ECM-dependent. Setting HasEcm=false while keeping MaxFP=1.0 should maintain 45–55% balance — mirrors iter-5 (Ghost HasEcm=false had zero effect).

**Success criteria:** Balance stays within 45–55%; BlueEcm remains Linchpin or near-Linchpin.

---

## Code Change

```
BlueEcm MaxFirePower: 1.5 → 1.0 (baseline fix from iter-11)
BlueEcm HasEcm:      true → false (probe)
```

Note: MaxFirePower was also set to 1.0 (master is at 1.5; iter-11 fix awaiting human merge). This isolates the ECM dependency question relative to the confirmed balanced config.

---

## Run Parameters

- Batch: 200 matches
- Seed: 1000
- Arena: 800×600 (default)
- Timeout policy: `--on-timeout energy`

---

## Raw Results

```
Red: 103 wins (52%)  |  Blue: 97 wins (48%)  |  Draws: 0

Win quality: Red: +75E decisive (63), 40 TO +17E (38% via TO)
             Blue: +76E decisive (64), 33 TO +17E (34% via TO)
Decisive med: Red 340t / Blue 149t (Blue 2.3x faster)

Per-tank:
  RedGhost     65/200 surv  57/103 WinSurv (65E)  Rate 2.43
  RedArrow     47/200 surv  40/103 WinSurv (21E)  Rate 13.20
  RedBlade     31/200 surv  23/103 WinSurv (14E)  Rate 17.32
  RedHammer    31/200 surv  20/103 WinSurv (10E)  Rate 16.74

  BlueEcm      80/200 surv  71/97  WinSurv (38E)  Rate 7.39
  BlueGuard    41/200 surv  29/97  WinSurv (24E)  Rate 18.89
  BlueStrike   39/200 surv  32/97  WinSurv (17E)  Rate 13.89
  BlueRush     37/200 surv  27/97  WinSurv (17E)  Rate 16.99
  BlueSharp    33/200 surv  27/97  WinSurv (20E)  Rate 19.24

Insights:
  RedGhost:  MVP (57/103), Survivor, Solo carry 33/103 (25D+8TO), ECM
  RedArrow:  Survivor, Solo carry 21/103 (6D+15TO)
  RedBlade:  Top attacker, Glass cannon, Survivor

  BlueEcm:   MVP (71/97), Linchpin (alive:89%/dead:22%), Survivor, Solo carry 31/97 (15D+16TO), ECM
  BlueGuard: Expendable, Survivor
  BlueRush:  Expendable, Survivor
  BlueSharp: Top attacker, Glass cannon, Survivor, Primary target
```

---

## Analysis

### Balance: CONFIRMED within target
Red 52% / Blue 48% — within 45–55%. Virtually identical to iter-11's seed-1000 result (Red 51% / Blue 49%). Removing ECM from BlueEcm had no measurable effect on the win split.

### BlueEcm Linchpin status preserved
BlueEcm is Linchpin (alive:89%/dead:22%) even with HasEcm=false. The Linchpin role is NOT driven by ECM capability. BlueEcm's structural value is its survival, not its jamming.

### Comparison vs iter-11 baseline (MaxFP=1.0, HasEcm=true, seed 1000)
| Metric | Iter-11 (HasEcm=true) | Iter-15 (HasEcm=false) |
|--------|----------------------|------------------------|
| Red%   | 51%                  | 52%                    |
| Blue%  | 49%                  | 48%                    |
| BlueEcm Linchpin | Yes         | Yes                    |

Delta is within statistical noise. ECM removal is neutral.

### Ghost not Linchpin in this run
Ghost is solo-carry (33/103 = 32%) and MVP, but not labeled Linchpin. This contrasts with iter-13 (seed 3000) where both Ghost and BlueEcm were simultaneously Linchpin. Seed-1000 runs show Ghost as Linchpin in some but not all — appears seed-dependent. Not a meaningful change from removing BlueEcm's ECM.

### What IS driving BlueEcm's Linchpin role?
ECM is ruled out. Remaining candidates:
1. **PreferredRange=150** — standoff positioning keeps it alive longer
2. **RetreatEnergyThreshold=35** — aggressive retreat preserves energy for late-game survival
3. **MaxFP=1.0** — energy conservation (vs 1.5 over-firing) lets it outlast Red ECM duels

---

## Key Findings

1. **BlueEcm's Linchpin/carry role is structural, not ECM-dependent.** HasEcm=false has zero effect on balance or role assignment.
2. **Balance at MaxFP=1.0 holds without ECM** — 52%/48%, well within target.
3. **ECM hardware is NOT the mechanism** behind the MaxFP=1.0 fix. The fix is energy conservation, not ECM effectiveness.

---

## Summary

**Hypothesis CONFIRMED.** BlueEcm's carry role is structural (survival-driven). Removing ECM doesn't break it. Code reverted — HasEcm=false is a probe, not a fix.

**Next hypothesis:** BlueEcm's survival advantage comes from PreferredRange=150 (standoff positioning). Test: BlueEcm with PreferredRange=80 (close-quarters, like combat tanks) at MaxFP=1.0, HasEcm=true — expect Linchpin to weaken and Blue to lose ground.
