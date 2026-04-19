# Iter 12 — Cross-Seed Validation (BlueEcm MaxFP=1.0)

**Date:** 2026-04-18
**Branch:** `research/iter-12-cross-seed-validation`
**Status:** Confirmed — MaxFirePower=1.0 balance holds across seeds

---

## Hypothesis

The balanced result (51%/49%) from BlueEcm MaxFP=1.0 at seed 1000 is not seed-specific. Cross-validate at seed 2000 (same 200 matches, energy timeout) — expect Red 45–55%, Blue 45–55%.

## Code Change

None — same fix from iter-11: `BlueEcm MaxFirePower = 1.0` (unchanged).

## Run Parameters

```
--batch 200 --parallel 8 --seed 2000 --on-timeout energy --format table
Arena: default (800×600)
BlueEcm MaxFirePower=1.0 (iter-11 fix)
```

## Raw Results

```
Red: 92 wins (46%)  |  Blue: 108 wins (54%)  |  Draws: 0

Win quality: Red: +82E decisive (59), 33 TO +18E (35% via TO)
             Blue: +93E decisive (70), 38 TO +15E (35% via TO)
Decisive med: Red 394t / Blue 132t (Blue 3.0x faster)
Avg first kill: tick 24

Tank                  WinSurv            Rate/100t
RedGhost          65/92 (65E) 71%  WinSurv    2.91
RedArrow          35/92 (18E)                 16.51
BlueEcm           68/108 (44E) 63% WinSurv    8.71
BlueGuard         48/108 (24E)                12.13

Insights:
  RedGhost: MVP, Linchpin (alive:92%, dead:21%), Solo carry 39/92 (42%), ECM
  BlueEcm:  MVP, Solo carry 24/108 (22%), ECM
```

## Analysis

| Seed | Red% | Blue% | In Range? |
|------|------|-------|-----------|
| 1000 | 51%  | 49%   | ✅        |
| 2000 | 46%  | 54%   | ✅        |

Both seeds fall within the 45–55% balance target. The fix is **seed-robust**.

The ECM duel dynamics differ between seeds:
- Seed 1000: symmetric (Ghost 34%, BlueEcm 34% of team wins) → slight Red edge
- Seed 2000: asymmetric (Ghost 42%, BlueEcm 22%) → Blue overall wins more (54%)

Ghost is more dominant at seed 2000 (42% solo carry vs 34%), yet Blue wins 54% overall. This means Blue's non-ECM corps (Guard, Rush, Strike, Sharp) are relatively stronger at this seed mix, offsetting Ghost's carry advantage.

## Key Findings

1. **Balance fix is seed-robust.** MaxFP=1.0 keeps both tested seeds within ±5% of 50/50.
2. **ECM duel is non-deterministic.** Ghost's solo carry varies 34–42% across seeds; BlueEcm's varies 22–34%. Both tanks can dominate in isolation, but neither locks in a single-seed artifact.
3. **Ghost remains Linchpin across seeds.** Alive→92% Red wins / dead→21% Red wins at seed 2000 (similar to seed 1000 pattern).
4. **BlueEcm no longer Linchpin.** At seed 1000 it was Linchpin; at seed 2000 it is just MVP with solo carry. The symmetric ECM duel is seed-sensitive in its dynamics, but not in overall balance.

## Summary

**Confirmed — balance fix (BlueEcm MaxFP=1.0) holds across two independent seeds (1000 → 51%/49%, 2000 → 46%/54%).**

Both seeds are within the 45–55% balance window. No further code change needed. Research goal achieved.

**Next options:**
- HALT: balance confirmed at 2 seeds — declare done, human merges iter-11 fix.
- Iter 13: Seed 3000 for triple validation.
- Iter 13: Investigate Ghost's 34–42% solo carry variance across seeds (why does Ghost carry more at seed 2000 yet Blue wins more?).
