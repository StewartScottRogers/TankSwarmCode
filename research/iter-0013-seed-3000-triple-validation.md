# Iter 13 — Seed 3000 Triple-Validation (BlueEcm MaxFP=1.0)

**Date:** 2026-04-18
**Branch:** `research/iter-13-seed-3000-triple-validation`
**Status:** Confirmed — balance fix validated across three independent seeds

---

## Hypothesis

The balanced results at seed 1000 (51%/49%) and seed 2000 (46%/54%) are not artifacts of two specific seeds. A third seed (3000) should also fall within the 45–55% balance target.

## Code Change

None — same fix from iter-11: `BlueEcm MaxFirePower = 1.0` (unchanged).

## Run Parameters

```
--batch 200 --parallel 8 --seed 3000 --on-timeout energy --format table
Arena: default (800×600)
BlueEcm MaxFirePower=1.0 (iter-11 fix)
```

## Raw Results

```
Red: 101 wins (50%)  |  Blue: 99 wins (50%)  |  Draws: 0

Win quality: Red: +77E decisive (66), 35 TO +27E (34% via TO)
             Blue: +82E decisive (74), 25 TO +15E (25% via TO)
Decisive med: Red 320t / Blue 160t (Blue 2.0x faster)
Avg first kill: tick 24

Tank                  WinSurv                        Rate/100t
RedGhost          74/101 (68E) MVP Linchpin           2.63
BlueEcm           70/99  (46E) MVP Linchpin           9.17

Insights:
  RedGhost: MVP, Linchpin (alive:96%, dead:22%), Solo carry 48/101 (48%), ECM
  BlueEcm:  MVP, Linchpin (alive:85%, dead:25%), Solo carry 28/99  (28%), ECM, Survivor (12/101 losses)
```

## Analysis

### Cross-seed validation table

| Seed | Red%  | Blue% | In Range? | Ghost Solo% | BlueEcm Solo% |
|------|-------|-------|-----------|-------------|---------------|
| 1000 | 51%   | 49%   | ✅        | 34%         | 34%           |
| 2000 | 46%   | 54%   | ✅        | 42%         | 22%           |
| 3000 | 50.5% | 49.5% | ✅        | 48%         | 28%           |

All three seeds fall within the 45–55% balance target. **The fix is seed-robust across three trials.**

### Seed 3000 specifics

- Ghost's solo carry rate hit 48% — highest across all three seeds. Yet Blue is still nearly even at 49.5%. This confirms that Ghost's carry dominance is offset by Blue's corps (Guard, Rush, Strike) performing well when BlueEcm is not needed.
- BlueEcm returned to Linchpin status at seed 3000 (alive:85%/dead:25%), similar to seed 1000. Seed 2000 was the anomaly where BlueEcm was merely MVP.
- Both ECM tanks are simultaneously Linchpin at seed 3000 — the ECM duel is at maximum tension while overall balance holds exactly at 50/50.

### Ghost solo carry trend

Ghost's solo carry rate increases across seeds (34% → 42% → 48%), yet balance holds or improves. This suggests Ghost's carry rate is *not* the binding balance constraint — it's the effectiveness of Blue's corps that determines the outcome, not Ghost's absolute carry ceiling.

## Key Findings

1. **Triple-seed confirmation.** BlueEcm MaxFP=1.0 passes all three seeds (1000/2000/3000) within the 45–55% target. Fix is robust.
2. **Ghost solo carry is seed-variable (34–48%) but not balance-breaking.** Even at 48% carry, balance holds because Blue's corps compensates.
3. **BlueEcm Linchpin status is seed-variable.** Seeds 1000 and 3000 show it as Linchpin; seed 2000 does not. The ECM duel intensity varies but never breaks balance.
4. **Both ECM tanks can be simultaneously Linchpin** (seed 3000) — the symmetric ECM duel can run at full tension without destabilizing overall win rates.

## Summary

**Confirmed — BlueEcm MaxFP=1.0 is seed-robust across three independent trials.**

| Seed | Red%  | Blue% |
|------|-------|-------|
| 1000 | 51%   | 49%   |
| 2000 | 46%   | 54%   |
| 3000 | 50.5% | 49.5% |

**HALT: Research goal fully achieved. Human should merge iter-11 fix (BlueEcm MaxFirePower=1.0) to master.**
