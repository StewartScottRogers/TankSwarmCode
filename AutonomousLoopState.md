# Autonomous Loop State

## Last Updated
2026-04-18 — Iteration 41 complete

## Current Iteration
**42** — pending

## Status
**BALANCE FIX CONFIRMED (iter-11).** BlueEcm MaxFirePower=1.0 achieves Red 51% / Blue 49%, triple-seed validated (seeds 1000/2000/3000).

Iter 41 confirmed BlueEcm is NOT Linchpin at PR=218 — Linchpin threshold narrowed to **212 < threshold ≤ 218**. Balance is perfect 50/50 at PR=218 despite no Linchpin. Two excellent 50/50 configurations now known: PR=212 (Linchpin active) and PR=218 (no Linchpin).

**Linchpin threshold: 212 < X ≤ 218 (6-unit window).**

**Awaiting human merge of iter-11 fix (BlueEcm MaxFirePower=1.0) to master.**

**Next hypothesis:** Test PR=215 (midpoint 212–218) to nail the exact Linchpin threshold. Predict: If Linchpin at PR=215, threshold is 215–218. If not Linchpin, threshold is 212–215. Balance prediction uncertain (non-monotonic region) — either 50/50 or slight deviation.

---

## Iteration Log

### Iter 41 — `research/iter-000041-blueecm-pr218`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — BlueEcm NOT Linchpin at PR=218; threshold narrowed to 212–218

**Hypothesis:** PR=218 midpoint binary search — predict BlueEcm loses Linchpin (threshold is 212–218).

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0, PR=218

**Results:**
- Red 100 (50%) / Blue 100 (50%) — **perfect 50/50 balance**
- BlueEcm: MVP (70/100), Solo carry (30/100), **NO Linchpin**, Rate/100t: 6.67
- RedGhost: MVP (61/100), Solo carry (33/100), **NO Linchpin**, Rate/100t: 2.90

**PR balance curve (MaxFP=1.0) — updated:**

| BlueEcm PR | Red%  | Blue% | Linchpin? |
|------------|-------|-------|-----------|
| 150        | 51%   | 49%   | Yes       |
| 200        | 54%   | 46%   | Yes       |
| 212        | 50%   | 50%   | Yes ✓     |
| 218        | 50%   | 50%   | No ✓      |
| 225        | 48%   | 52%   | No        |
| 250        | 56%   | 44%   | No        |
| 300        | 46%   | 54%   | No        |

**Key finding:** Linchpin threshold confirmed between PR=212 and PR=218 (6-unit window). Both PR=212 and PR=218 give 50/50 balance — balance mechanism differs (Linchpin vs non-Linchpin carrier) but outcome is identical.
**Code:** Reverted — MaxFP=1.5, PR=150 restored (master state).

---

### Iter 40 — `research/iter-000040-blueecm-pr212`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — BlueEcm IS Linchpin at PR=212; threshold narrowed to 212–225

**Hypothesis:** PR=212 midpoint binary search — predict BlueEcm retains Linchpin (threshold is 212–225).

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0, PR=212

**Results:**
- Red 101 (50%) / Blue 99 (50%) — **near-perfect balance**
- BlueEcm: MVP (70/99), **Linchpin (alive:85%/dead:25%)**, Solo carry (32/99), Rate/100t: 7.44
- RedGhost: MVP (64/101), Solo carry (37/101), ECM — no Linchpin flag

**Non-monotonic PR balance curve (MaxFP=1.0):**

| BlueEcm PR | Red%  | Blue% | Linchpin? |
|------------|-------|-------|-----------|
| 150        | 51%   | 49%   | Yes       |
| 200        | 54%   | 46%   | Yes       |
| 212        | 50%   | 50%   | Yes ✓     |
| 225        | 48%   | 52%   | No        |
| 250        | 56%   | 44%   | No        |
| 300        | 46%   | 54%   | No        |

**Key finding:** Linchpin threshold between PR=212 and PR=225. PR=212 gives best observed balance (50/50) with Linchpin active.
**Code:** Reverted — MaxFP=1.5, PR=150 restored (master state).

---

### Iter 39 — `research/iter-000039-blueecm-pr225`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — BlueEcm NOT Linchpin at PR=225; threshold narrowed to 200–225

**Hypothesis:** PR=225 midpoint binary search — predict BlueEcm loses Linchpin (threshold is 200–225 or 225–250).

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0, PR=225

**Results:**
- Red 95 (48%) / Blue 105 (52%) — Blue-favored (unexpected; predicted ~55% Red)
- BlueEcm: MVP (66/105), Solo carry (27/105), **NO Linchpin** — threshold confirmed in 200–225
- RedGhost: MVP (60/95), Solo carry (31/95), **NO Linchpin** — both ECMs non-Linchpin at PR=225
- BlueEcm Rate/100t: 7.82 (lower than PR=250's 9.99; medium-range still plays as hider)

**Key finding:** Linchpin threshold between PR=200 and PR=225. Balance unexpectedly Blue-favored at PR=225 — non-monotonic, high-variance region.
**Code:** Reverted — MaxFP=1.5, PR=150 restored (master state).

---

### Iter 38 — `research/iter-000038-blueecm-pr250`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — BlueEcm NOT Linchpin at PR=250; threshold narrowed to 200–250

**Hypothesis:** PR=250 midpoint test — predict BlueEcm loses Linchpin.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0, PR=250

**Results:**
- Red 112 (56%) / Blue 88 (44%) — Red dominant
- BlueEcm: MVP (51/88), **NO Linchpin** — threshold confirmed in 200–250
- RedGhost: MVP (66/112), Solo carry (33/112), **NO Linchpin**
- BlueEcm WinSurv: 51/88, Rate/100t: 9.99

**Key finding:** PR=250 peaks Red dominance at 56% — equal to original baseline. Linchpin threshold is between 200–250.
**Code:** Reverted — MaxFP=1.5, PR=150 restored (master state).

---

### Iter 19 — `research/iter-000019-blueecm-maxfp125`
**Date:** 2026-04-18
**Status:** ✅ PARTIALLY CONFIRMED — balance linear (correct), Linchpin prediction wrong

**Hypothesis:** MaxFP=1.25 → Red ~53–54%, Linchpin likely lost.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.25 + PR=150

**Results:**
- Red 108 (54%) / Blue 92 (46%) — **balance prediction correct**
- BlueEcm: MVP (65/92), **Linchpin (alive:90%/dead:21%)** — Linchpin prediction WRONG
- Survival: 72/200, WinSurv 71% — still high despite higher MaxFP

**Balance curve (MaxFP=1.0–1.5 is approximately linear):**

| MaxFP | Red%  | Blue% | Linchpin? |
|-------|-------|-------|-----------|
| 1.0   | 51%   | 49%   | Yes       |
| 1.25  | 54%   | 46%   | Yes       |
| 1.5   | 56%   | 44%   | No        |

**Key finding:** Linchpin threshold is between MaxFP=1.25 and MaxFP=1.5, not at exactly 1.0.
**Code:** Reverted — 1.25 is a probe only.

---

### Iter 18 — `research/iter-000018-blueecm-maxfp15-pr80`
**Date:** 2026-04-18
**Status:** ✅ REFUTED — MaxFP=1.5 does not dominate PR=80; effects approximately cancel

**Hypothesis:** MaxFP=1.5 + PR=80 will tip toward Red-dominant (MaxFP=1.5 dominates over PR=80).

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.5 + PR=80

**Results:**
- Red 104 (52%) / Blue 96 (48%) — **near-balanced, same as iter-11/17**
- BlueEcm: MVP (50/96), **NO Linchpin**, survival 61/200 (dropped from 80/200)
- Effects are additive: MaxFP=1.5 (+5pp Red) + PR=80 (+7pp Blue) ≈ cancel to 52/48

**2×2 grid (MaxFP × PR):**

| Config               | Red%  | Blue% | Linchpin? |
|----------------------|-------|-------|-----------|
| MaxFP=1.5, PR=150    | 56%   | 44%   | No        |
| MaxFP=1.0, PR=150    | 51%   | 49%   | Yes       |
| MaxFP=1.0, PR=80     | 44%   | 56%   | No        |
| MaxFP=1.5, PR=80     | 52%   | 48%   | No        |

**Key finding:** Linchpin requires MaxFP=1.0 AND PR=150 simultaneously. Parameter effects are approximately additive — no interaction synergy.
**Code:** Reverted — PR=80 probe only.

---

### Iter 14 — `research/iter-000014-blueecm-fp-curve`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — non-linearity is a sharp threshold between MaxFP=0.75 and 1.0

**Hypothesis:** MaxFP=0.75 (midpoint between 0.5 and 1.0) will reveal whether the balance curve is gradual or sharp.

**Run:** 200 matches, seed 1000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=0.75

**Results:**
- Red 82 (41%) / Blue 117 (58%) + 1 draw — **Blue dominant, similar to MaxFP=0.5 (60%)**
- RedGhost: MVP, **Linchpin** (alive:89%/dead:23%), solo carry 30/82 (37%) — Ghost critical to Red
- BlueEcm: MVP (83/117 = 71% WinSurv), **NOT Linchpin**, solo carry 32/117 (27%)
- BlueEcm is NOT Linchpin at 0.75 — Blue corps wins without it; ECM duel tilts to Ghost

**Curve table:**

| MaxFP | Red%  | Blue% | BlueEcm Linchpin? |
|-------|-------|-------|-------------------|
| 0.1   | 41%   | 59%   | No                |
| 0.5   | 40%   | 60%   | No                |
| 0.75  | 41%   | 58%   | No                |
| 1.0   | 51%   | 49%   | Yes (iter-11)     |
| 1.5   | 56%   | 44%   | Yes (baseline)    |

**Key finding:** Non-linearity is a single sharp threshold. The entire 0.1–0.75 range is one flat Blue-dominant regime. The Linchpin phase transition and balance crossover happen together between 0.75 and 1.0. MaxFP=1.0 is uniquely correct.
**Code:** Reverted — 0.75 is not a fix.

---

### Iter 13 — `research/iter-13-seed-3000-triple-validation`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — Triple-seed validation complete (no code change)

**Hypothesis:** BlueEcm MaxFP=1.0 balance holds at seed 3000 (third independent seed).

**Run:** 200 matches, seed 3000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0

**Results:**
- Red 101 (50.5%) / Blue 99 (49.5%) — ✅ closest to 50/50 of all three seeds
- Ghost: MVP, Linchpin (alive:96%/dead:22%), solo carry 48/101 = **48%** (highest across seeds)
- BlueEcm: MVP, Linchpin (alive:85%/dead:25%), solo carry 28/99 = **28%**
- Both ECM tanks simultaneously Linchpin — ECM duel at maximum tension, balance perfect

**Code:** No change — same iter-11 fix (BlueEcm MaxFP=1.0).

---

### Iter 12 — `research/iter-12-cross-seed-validation`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — Balance fix seed-validated (no code change)

**Hypothesis:** BlueEcm MaxFP=1.0 balance holds at seed 2000 (not just seed 1000).

**Run:** 200 matches, seed 2000, `--on-timeout energy`, default arena (800×600), BlueEcm MaxFP=1.0

**Results:**
- Red 92 (46%) / Blue 108 (54%) — ✅ within 45–55% balance target
- Ghost: MVP, Linchpin (alive:92%/dead:21%), solo carry 39/92 = **42%**
- BlueEcm: MVP, solo carry 24/108 = **22%**

**Code:** No change — same iter-11 fix (BlueEcm MaxFP=1.0).

---

### Iter 11 — `research/iter-11-blueecm-fp-calibrate-0.5`
**Date:** 2026-04-18
**Status:** ✅ CONFIRMED — BALANCE FIX FOUND

**Hypothesis:** Binary search between MaxFP=0.1 (Blue59%) and 1.5 (Red56%) — try 0.5, then 1.0.

**Binary search runs:**
- MaxFirePower=0.5: Red 40%, Blue 60% — still Blue-dominated
- MaxFirePower=1.0: Red **51%**, Blue **49%** — ✅ BALANCED (target: 45–52%)

**Code change KEPT:** BlueEcm MaxFirePower 1.5 → 1.0

**Root cause:** Ghost (MaxFP=0.1) is a hider; BlueEcm (1.5) over-fires, drains energy, dies in Red wins. Fix: 1.5→1.0 balances the ECM duel.

**Awaiting human merge.**

---

### Iters 1–10
See prior branch history. Key findings:
- Red 56% baseline is structural (ECM parameter sweeps all return 56%)
- Ghost's hider role (MaxFP=0.1) worth +16pp (iter-8)
- BlueEcm MaxFP=0.1 over-corrects to Blue 59% (iter-10)
- Balance insensitive to ECM params, ECM modes, arena ECM geometry
