# Iter 3 — Ghost ECM Nerf (Spoof Radius)

**Date:** 2026-04-18
**Branch:** `research/iter-3-ghost-ecm-nerf`
**Status:** Refuted

---

## Hypothesis

Reducing RedGhost's ECM spoof radius by 25% (EcmSpoofRadius: 130.0 → 97.5) in the default arena (800×600) brings Red's win rate from 56% toward 45–52%, drops Ghost's solo carry rate from 30% to ≤20%, while Ghost retains MVP status.

## Code Change

`TankSwarmCode.SwarmTank.Interfaces/ArenaConstants.cs`:
- `EcmSpoofRadius`: 130.0 → 97.5 (–25%)

**Reverted** — hypothesis refuted, no code change persists.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
```

## Raw Results

```
Red: 111 wins (56%)  |  Blue: 89 wins (44%)  |  Draws: 0

Win quality: Red: +68E decisive (77), 34 TO +20E (30% via TO)
             Blue: +73E decisive (56), 33 TO +15E (37% via TO)
Decisive med: Red 329t / Blue 129t (Blue 2.6x faster)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             71/200 (52D+14TO+5L)   66/111 (66E)      2.24
RedArrow             44/200 (28D+8TO+8L)    36/111 (20E)     13.03
RedBlade             36/200 (16D+6TO+14L)   22/111 (12E)     15.77
RedHammer            35/200 (19D+10TO+6L)   29/111 (10E)     16.27

BlueEcm              64/200 (39D+16TO+9L)   55/89  (32E)     10.10
BlueStrike           43/200 (25D+8TO+10L)   33/89  (18E)     15.72
BlueGuard            40/200 (30D+3TO+7L)    33/89  (21E)     14.30
BlueRush             39/200 (24D+6TO+9L)    30/89  (19E)     14.28
BlueSharp            33/200 (24D+6TO+3L)    30/89  (20E)     16.45

RedGhost insights: MVP (66/111), Survivor (5/89 losses), Solo carry (36/111, 25D+11TO), ECM

Win combos (Red):
  36x Ghost             (32%) [25D+11TO]
  18x Arrow             (16%) [12D+6TO]
  14x Hammer            (13%) [4D+10TO]
   9x Arrow+Ghost        (8%) [8D+1TO]
   9x Ghost+Hammer       (8%) [9D]
   8x Blade+Ghost        (7%) [6D+2TO]
   (+ more combos)
```

## Analysis

| Criterion | Threshold | Iter-1 Baseline | Actual | Pass? |
|---|---|---|---|---|
| Red win rate | 45–52% | 56% | 56% | ❌ |
| Ghost solo carry | ≤20% | 30% | 32% | ❌ |
| Ghost MVP | retained | 58% WinSurv | 59% WinSurv | ✅ |
| Direction of effect | carry drops | — | carry increased | ❌ |

## Key Findings

1. **EcmSpoofRadius is not the binding constraint.** Reducing it by 25% had zero measurable effect. Red win rate and Ghost solo carry are statistically identical to the iter-1 baseline (56%/30% → 56%/32%). The spoof radius doesn't constrain Ghost's effectiveness.

2. **Ghost's carry is via Jam, not spoof geometry.** Ghost solos decisively 25/36 times (69%), same ratio as iter-1 (74% decisive). Decisive wins mean Blue self-eliminates from ECM-induced misdirection — and that mechanism is the Jam (scan drop/corrupt), which applies regardless of the spoof radius. Ghost doesn't need to spawn ghosts far away to deceive Blue; the scan corruption does the work.

3. **Spoof radius reduction may have slightly helped Ghost.** Solo carry actually ticked up from 30% to 32%. Plausibly: with closer-range ghost echoes, Blue's scanner is slightly more confused in the zone where combat actually happens.

4. **Loss-survivals identical (5).** Ghost's durability profile didn't change — same role, same energy pattern.

## Next Hypothesis (Iter 4)

**The binding constraint is EcmJamDropChance (currently 0.50).** Reducing Jam drop chance from 0.50 → 0.30 and corrupt chance from 0.30 → 0.15 (a direct jam effectiveness nerf) will reduce Ghost's scan-corruption advantage and lower its solo carry rate. Both RedGhost and BlueEcm use Jam, but RedGhost depends on it for survival (Rate/100t ≈ 2); BlueEcm has real firepower (MaxFirePower=1.5) so survives the nerf better.

Success criteria:
- Red win rate drops from 56% toward 45–52%
- Ghost solo carry drops from 32% to ≤20%
- Ghost retains MVP role (still top WinSurv on Red)
- BlueEcm WinSurv rises or holds (less suppressed by Ghost's jam)

Branch: `research/iter-4-jam-drop-nerf`
