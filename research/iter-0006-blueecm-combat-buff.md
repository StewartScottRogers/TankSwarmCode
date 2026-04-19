# Iter 6 — BlueEcm Combat Buff

**Date:** 2026-04-18
**Branch:** `research/iter-6-blueecm-combat-buff`
**Status:** Refuted — buffing BlueEcm's combat increases Red's win rate

---

## Hypothesis

Red's 56% compositional advantage comes from BlueEcm being Blue's Linchpin single point of failure. Buffing BlueEcm's combat ability (MaxFirePower 1.5→2.5, RetreatEnergyThreshold 35.0→25.0) makes BlueEcm harder to kill and raises its solo carry rate, bringing Red's win rate from 56% toward 45–52%.

## Code Change

`TankSwarmCode.SwarmTanks.Blue/BlueEcm.cs`:
- `MaxFirePower`: 1.5 → 2.5 (+67%)
- `RetreatEnergyThreshold`: 35.0 → 25.0 (fights longer before retreating)

**Reverted** — hypothesis refuted, code restored.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
```

## Raw Results

```
Red: 124 wins (62%)  |  Blue: 76 wins (38%)  |  Draws: 0

Win quality: Red: +70E decisive (70), 54 TO +18E (43% via TO)
             Blue: +73E decisive (39), 37 TO +11E (48% via TO)
Decisive med: Red 340t / Blue 119t (Blue 2.9x faster)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             72/200 (52D+20TO+0L)   72/124 (67E)      2.03
RedArrow             58/200 (26D+16TO+16L)  42/124 (13E)     11.03
RedBlade             44/200 (22D+9TO+13L)   31/124  (9E)     15.45
RedHammer            39/200 (18D+13TO+8L)   31/124  (8E)     12.37

BlueEcm              50/200 (22D+11TO+17L)  33/76  (13E)     10.82
BlueGuard            38/200 (20D+11TO+7L)   31/76  (25E)     10.74
BlueRush             36/200 (12D+10TO+14L)  22/76  (20E)     13.88
BlueStrike           34/200 (21D+3TO+10L)   24/76  (20E)     14.70
BlueSharp            33/200 (17D+7TO+9L)    24/76  (20E)     12.54

RedGhost insights: MVP (72/124), **All-in** (72 surv, 0 loss-survivals), Solo carry (35/124, 19D+16TO), ECM
BlueEcm insights:  MVP (33/76), Survivor (17/124 losses), ECM  [Linchpin GONE — 17 loss-survivals]

Win combos (Blue):
  14x Guard             (18%) [4D+10TO]  ← Guard emerged as Blue's top solo carrier!
  11x Rush              (14%) [3D+8TO]
   9x Ecm               (12%) [2D+7TO]   ← BlueEcm solo wins collapsed from baseline
```

## Analysis

| Criterion | Threshold | Iter-1 Baseline | Iter-6 (Buff) | Pass? |
|---|---|---|---|---|
| Red win rate | 45–52% | 56% | 62% | ❌ (went wrong direction) |
| BlueEcm WinSurv increases | >32% | 62%† | 43% | ❌ |
| BlueEcm Linchpin retained | — | Linchpin† | No Linchpin | ❌ |

*(† iter-5 without Ghost jam showed Linchpin; iter-1 baseline: ~60% WinSurv)*

## Key Findings

1. **Combat buff destroyed BlueEcm's effectiveness.** Higher MaxFirePower drains energy faster when firing; lower RetreatEnergyThreshold means BlueEcm commits to fights it can't survive. Result: 17 loss-survivals (up from 8–9 baseline), total survivals dropped from 64→50. BlueEcm went from MVP with 60%+ WinSurv to a unit bleeding out in Red wins.

2. **BlueEcm's role is ECM+survival, NOT combat.** This mirrors the Ghost finding: tanks in ECM roles derive their value from surviving, not fighting. The 1.5 MaxFirePower baseline was calibrated to burn through Ghost's jam occasionally while preserving energy for ECM operations and survival.

3. **Ghost regained All-in status (0 loss-survivals).** With BlueEcm less effective at counterjamming, Ghost faces a weaker opponent and now survives only in wins. Red's grip on matches became tighter.

4. **Blue's win pattern completely restructured.** Guard (14x solos) replaced BlueEcm (9x solos) as Blue's top carrier — BlueEcm was burning to death, freeing Guard to collect surviving wins in long matches. This reveals Guard as a latent carry if BlueEcm falls.

5. **Red's timeout dominance surged (43% of Red wins via TO, up from 30%).** Ghost now grinds out more timeouts because BlueEcm is less able to pressure Red with burnthrough.

## Next Hypothesis (Iter 7)

**Red's 56% advantage is driven by RedArrow's extreme first-kill aggression (RetreatEnergyThreshold=20).** Arrow gets first blood in 60% of matches consistently. This early numerical advantage compounds into Red wins. Raising RedArrow's RetreatEnergyThreshold from 20 → 35 (matching BlueEcm's retreat level) reduces Arrow's first-kill frequency, allows more matches to stay numerically even, and should drop Red's win rate from 56% toward 45–52%.

Success criteria:
- Red win rate drops from 56% toward 45–52%
- Arrow first-kill count drops below 28 (vs baseline ~35)
- Red's first-kill advantage in 60% of matches drops toward 50%
- Ghost still solos carries (structural), Arrow solos may drop

Branch: `research/iter-7-arrow-retreat-nerf`
