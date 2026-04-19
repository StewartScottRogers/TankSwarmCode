# Iter 2 — Ghost Cramped Arena

**Date:** 2026-04-18
**Branch:** `research/iter-2-ghost-cramped-arena`
**Status:** Inconclusive

---

## Hypothesis

Cramped arena (350×250) reduces RedGhost solo carry rate by ≥50% (from 30% to ≤15%) by limiting ECM spoofing effectiveness. If confirmed, Ghost's decisive solos depend on long-range ECM spoof range, not close-quarters durability.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --width 350 --height 250 --format table
Arena: cramped (350×250)
```

No code changes — purely observational.

## Raw Results

```
Red: 69 wins (34%)  |  Blue: 131 wins (66%)  |  Draws: 0

Win quality: Red: +88E decisive (38), 31 TO +21E (44% via TO)
             Blue: +142E decisive (102), 29 TO +27E (22% via TO)
Decisive med: Red 170t / Blue 76t (Blue 2.2x faster)

Tank                    Surv                 WinSurv          Rate/100t
RedGhost            33/200 (25D+8TO+0L)   33/69  (75E)      2.41
RedBlade            35/200 (15D+10TO+10L) 25/69  (26E)     35.00
RedHammer           35/200 (9D+12TO+14L)  21/69  (10E)     28.12
RedArrow            27/200 (13D+7TO+7L)   20/69  (30E)     36.56

BlueEcm             80/200 (66D+8TO+6L)   74/131 (62E)     23.85
BlueRush            79/200 (63D+9TO+7L)   72/131 (41E)     32.19
BlueGuard           72/200 (54D+11TO+7L)  65/131 (41E)     29.32
BlueSharp           67/200 (53D+7TO+7L)   60/131 (34E)     40.96
BlueStrike          67/200 (53D+6TO+8L)   59/131 (36E)     35.02

RedGhost insights: MVP (33/69), Linchpin (alive: 100%, dead: 22%), All-in (33 surv, 0 loss), ECM

Win combos (Red):
  12x Ghost              (17%) [8D+4TO]
  11x Blade+Ghost        (16%) [8D+3TO]
   9x Hammer             (13%) [3D+6TO]  ← note: not 13x, Hammer solo dropped
   9x Blade              (13%) [3D+6TO]
   9x Arrow              (13%) [3D+6TO]
   5x Arrow+Ghost         (7%) [5D]
   (+ 4 one-off combos)
```

## Analysis

| Criterion | Threshold | Actual | Pass? |
|---|---|---|---|
| Solo carry reduction | ≥50% reduction | 43% (30%→17%) | ❌ |
| Solo carry rate | ≤15% of Red wins | 17% (12/69) | ❌ (close) |
| ECM mechanism | near-zero combat rate | 2.41 | ✅ |
| Direction of effect | cramped reduces solo carry | Yes, 30%→17% | ✅ |
| All-in role | zero loss-survivals | 0 loss-survivals | ✅ |
| MVP role | — | 33/69 = 48% WinSurv | ✅ |
| Linchpin | — | yes (22% Red wins without Ghost) | NEW |

## Key Findings

1. **Direction confirmed, magnitude missed by a narrow margin.** Solo carry fell from 30% to 17% (43% reduction) — close to the ≥50% threshold and ≤15% target, but not there. The hypothesis is directionally correct but the threshold was too aggressive.

2. **Ghost becomes Linchpin in cramped arena.** Without Ghost, Red wins only 22% of matches (vs 34% overall). This is the opposite of what the hypothesis expected — cramped arena doesn't diminish Ghost's importance, it concentrates it. Ghost enables other Red tanks through ECM suppression rather than solo outlasting.

3. **Ghost regained All-in status in cramped arena.** In the default arena, Ghost had 5 loss-survivals — suggesting some defensive hiding. In the cramped arena, Ghost survives only in wins (0 loss-survivals). Nowhere to retreat, so Ghost only survives when Red wins.

4. **Red collapsed from 56% to 34%.** Cramped arena heavily nerfs Red overall. Blue's blitz speed doubled (76t vs 129t median decisive). Red's entire defensive strategy (ECM hiding + outlasting) is disrupted by the small map — Blue rushes too fast to allow the grind.

5. **BlueEcm stays MVP at 57% WinSurv.** BlueEcm's burnthrough mode makes it a combat contributor in cramped arena (Rate/100t = 23.85), not just a jammer. In the default arena BlueEcm was already Blue's MVP — the cramped arena didn't change this.

6. **Blade+Ghost combo emerges (16% of Red wins).** In default arena, Ghost solos dominated. In cramped arena, Ghost pairs with Blade frequently — suggests Ghost is buying time for Blade to finish Blue off rather than outlasting alone.

## Next Hypothesis (Iter 3)

**Reducing RedGhost's jamming radius by 25% (nerf to ECM effectiveness) in the default arena brings Red's win rate from 56% toward 50% without eliminating Ghost's MVP contribution.**

Success criteria:
- Red win rate drops from 56% to 45–52% (closer to parity)
- Ghost solo carry rate drops from 30% to ≤20%
- Ghost retains MVP role (still top WinSurv on Red)

Branch: `research/iter-3-ghost-ecm-nerf`
