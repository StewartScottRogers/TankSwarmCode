# Iter 5 — Ghost No ECM (Structural Carry Test)

**Date:** 2026-04-18
**Branch:** `research/iter-5-ghost-no-ecm`
**Status:** Confirmed

---

## Hypothesis

Ghost's carry is purely structural (hider/survivor role), not ECM. Setting Ghost's `HasEcm = false` will leave its solo carry rate and WinSurv unchanged (25–35% carry, MVP or near-MVP), while Red's win rate holds near 56%. If confirmed: ECM on Ghost is cosmetic and the real driver of Red's 56% is compositional.

## Code Change

`TankSwarmCode.SwarmTanks.Red/RedGhost.cs`:
- `HasEcm`: true → false
- `OffensiveEcmMode`: JamAndSpoof → (removed)

**Reverted** — observational test, no design change intended.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
```

## Raw Results

```
Red: 112 wins (56%)  |  Blue: 88 wins (44%)  |  Draws: 0

Win quality: Red: +66E decisive (79), 33 TO +18E (29% via TO)
             Blue: +75E decisive (55), 33 TO +15E (37% via TO)
Decisive med: Red 340t / Blue 129t (Blue 2.6x faster)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             73/200 (55D+13TO+5L)   68/112 (65E)      2.08
RedArrow             46/200 (29D+8TO+9L)    37/112 (18E)     13.18
RedBlade             36/200 (17D+5TO+14L)   22/112 (11E)     15.40
RedHammer            33/200 (18D+10TO+5L)   28/112 (10E)     16.68

BlueEcm              62/200 (36D+18TO+8L)   54/88  (32E)     10.16
BlueGuard            43/200 (30D+4TO+9L)    34/88  (24E)     13.64
BlueStrike           40/200 (24D+7TO+9L)    31/88  (18E)     15.36
BlueRush             37/200 (25D+4TO+8L)    29/88  (19E)     14.76
BlueSharp            34/200 (25D+6TO+3L)    31/88  (19E)     15.76

RedGhost insights: MVP (68/112), Survivor (5/88 losses), Solo carry (37/112, 26D+11TO)
                   [Note: labeled "ECM" due to TankRole.EcmSpecialist, but HasEcm=false — no actual ECM hardware]
BlueEcm insights:  MVP (54/88), **Linchpin** (alive: 87% Blue wins, dead: 25%), Survivor (8/112 losses)
                   [NEW: BlueEcm gained Linchpin status without Ghost's jamming suppression]

Win combos (Red):
  37x Ghost             (33%) [26D+11TO]
  19x Arrow             (17%) [12D+7TO]
  14x Hammer            (12%) [4D+10TO]
   9x Blade+Ghost        (8%) [7D+2TO]
   9x Arrow+Ghost        (8%) [9D]
   8x Ghost+Hammer       (7%) [8D]
```

## Analysis

| Criterion | Threshold | Iter-1 Baseline (ECM on) | Iter-5 (ECM off) | Pass? |
|---|---|---|---|---|
| Red win rate holds | 50–58% | 56% | 56% | ✅ |
| Ghost solo carry holds | 25–35% | 30% | 33% | ✅ |
| Ghost MVP or near-MVP | top WinSurv | 58% WinSurv | 61% WinSurv | ✅ |
| ECM confirmed cosmetic | carry unchanged | baseline | no change | ✅ |

## Key Findings

1. **Hypothesis confirmed: Ghost's carry is entirely structural.** Removing Ghost's ECM hardware (HasEcm=false) had zero negative effect — solo carry ticked up from 30% to 33%, WinSurv from 58% to 61%. Ghost is a passive hider: MaxFirePower=0.1 + RetreatEnergyThreshold=40 + non-threatening presence causes Blue to deprioritize Ghost. Blue eliminates itself fighting Arrow/Blade/Hammer; Ghost inherits wins.

2. **Ghost's ECM was actually hurting Blue's counter-ECM.** Without Ghost's JamAndSpoof suppressing BlueEcm's burnthrough, BlueEcm gained a new **Linchpin** insight (alive: 87% Blue wins, dead: 25%). Ghost's jamming was degrading BlueEcm's burnthrough effectiveness — removing it freed BlueEcm to operate more cleanly.

3. **Red's 56% advantage has nothing to do with Ghost's ECM.** Four ECM parameter sweeps (spoof radius, jam drop, jam corrupt, HasEcm) all return 56%. The advantage is compositional — somewhere in Red's combat roster (Arrow/Blade/Hammer matchups) or formation/retreat behavior vs Blue's roster.

4. **Ghost should be reclassified as Hider, not EcmSpecialist.** Rate/100t of ~2 qualifies as a Hider (threshold: rate < 3.0, non-ECM) in the Insights table. The TankRole.EcmSpecialist assignment is misleading — it describes the hardware intent, not the actual combat role.

5. **BlueEcm is Blue's Linchpin.** When Ghost's ECM is removed, BlueEcm naturally emerges as the Linchpin (87% alive → Blue win, 25% dead → Blue win). This was probably masked in baseline runs by Ghost's jam suppressing BlueEcm's burnthrough.

## Next Hypothesis (Iter 6)

**Red's 56% compositional advantage comes from BlueEcm being a single point of failure.** BlueEcm is now confirmed as Blue's Linchpin (87%/25%). If BlueEcm is removed or degraded, Blue should collapse to ~25% wins. Conversely, if BlueEcm is buffed (higher MaxFirePower, lower RetreatEnergyThreshold) to be harder to kill, Blue should approach 50%.

Test: Increase BlueEcm's combat ability — MaxFirePower from 1.5 → 2.5, RetreatEnergyThreshold from 35.0 → 25.0 (fight longer before retreating). This directly addresses the compositional imbalance by making Blue's key unit more durable in combat.

Success criteria:
- Red win rate drops from 56% toward 45–52%
- BlueEcm WinSurv increases (above 32%)
- BlueEcm retains Linchpin and MVP status
- Ghost still solo carries, but Red wins fewer overall

Branch: `research/iter-6-blueecm-combat-buff`
