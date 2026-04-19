# Iter 4 — Jam Drop Chance Nerf

**Date:** 2026-04-18
**Branch:** `research/iter-4-jam-drop-nerf`
**Status:** Refuted

---

## Hypothesis

Reducing EcmJamDropChance from 0.50 → 0.30 and EcmJamCorruptChance from 0.30 → 0.15 (jam effectiveness nerf) reduces Ghost's scan-corruption advantage, dropping its solo carry rate from 32% to ≤20% and Red's win rate from 56% toward 45–52%. BlueEcm (MaxFirePower=1.5) survives the nerf better than Ghost (Rate/100t ≈ 2) since BlueEcm has real combat ability.

## Code Change

`TankSwarmCode.SwarmTank.Interfaces/ArenaConstants.cs`:
- `EcmJamDropChance`: 0.50 → 0.30 (–40%)
- `EcmJamCorruptChance`: 0.30 → 0.15 (–50%)

**Reverted** — hypothesis refuted, no code change persists.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
```

## Raw Results

```
Red: 111 wins (56%)  |  Blue: 89 wins (44%)  |  Draws: 0

Win quality: Red: +65E decisive (76), 35 TO +20E (31% via TO)
             Blue: +74E decisive (55), 34 TO +14E (38% via TO)
Decisive med: Red 340t / Blue 129t (Blue 2.6x faster)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             71/200 (52D+13TO+6L)   65/111 (65E)      2.13
RedArrow             46/200 (27D+10TO+9L)   37/111 (18E)     13.01
RedBlade             36/200 (16D+6TO+14L)   22/111 (10E)     14.96
RedHammer            33/200 (18D+10TO+5L)   28/111 (10E)     16.28

BlueEcm              61/200 (36D+17TO+8L)   53/89  (31E)      9.91
BlueGuard            44/200 (31D+5TO+8L)    36/89  (21E)     13.95
BlueStrike           41/200 (24D+7TO+10L)   31/89  (19E)     15.45
BlueRush             39/200 (24D+5TO+10L)   29/89  (18E)     14.07
BlueSharp            34/200 (25D+6TO+3L)    31/89  (20E)     16.09

RedGhost insights: MVP (65/111), Survivor (6/89 losses), Solo carry (35/111, 25D+10TO), ECM

Win combos (Red):
  35x Ghost             (32%) [25D+10TO]
  20x Arrow             (18%) [12D+8TO]
  14x Hammer            (13%) [4D+10TO]
   9x Blade+Ghost        (8%) [7D+2TO]
   9x Arrow+Ghost        (8%) [8D+1TO]
   8x Ghost+Hammer       (7%) [8D]
```

## Analysis

| Criterion | Threshold | Iter-1 Baseline | Iter-3 | Iter-4 | Pass? |
|---|---|---|---|---|---|
| Red win rate | 45–52% | 56% | 56% | 56% | ❌ |
| Ghost solo carry | ≤20% | 30% | 32% | 32% | ❌ |
| Ghost MVP | retained | 58% WinSurv | 59% | 59% | ✅ |

## Key Findings

1. **Jam parameters are not the binding constraint.** Reducing EcmJamDropChance by 40% and EcmJamCorruptChance by 50% produced zero measurable effect. Three ECM parameter sweeps (iter-2 arena, iter-3 spoof radius, iter-4 jam chances) have all returned 56%/32% — the balance is insensitive to ECM parameter changes.

2. **Ghost's carry is structural, not ECM-driven.** Ghost's decisive solos (25/35 = 71%) happen at near-zero combat rate (2.13/100t). Blue is not being ECM-deceived into losing — Blue is running out of energy fighting Red's real combatants (Arrow/Blade/Hammer) while Ghost simply outlasts by not being a threat. Ghost's MaxFirePower=0.1 and RetreatEnergyThreshold=40 make it a passive hider. ECM is cosmetic at the carry level.

3. **Arrow and Hammer absorb ECM nerf cleanly.** Arrow gained 2 more solos (18→20), Hammer held at 14. These combat tanks compensate automatically when Ghost is slightly less sticky — the swarm self-balances.

4. **56% Red advantage is compositional, not ECM-based.** Red's win rate has been completely stable across all ECM parameter changes. The advantage comes from something else — likely Red's formation, retreat patterns, or combat matchups against Blue's specific roster.

## Next Hypothesis (Iter 5)

**Ghost's carry is purely structural (hider/survivor role), not ECM.** If Ghost's `HasEcm = false`, its solo carry rate and WinSurv should be unchanged (or only slightly reduced), while Red's overall win rate holds near 56%. The ECM hardware is not contributing to Ghost's effectiveness.

Success criteria:
- Red win rate remains 50–58% (±2% of baseline)
- Ghost solo carry remains 25–35% of Red wins
- Ghost still achieves MVP or near-MVP WinSurv
- If these hold: ECM on Ghost is confirmed cosmetic → next question is what drives Red's 56%

If Ghost's carry collapses without ECM (carry <20%, WinSurv <40%), then ECM IS load-bearing but through a mechanism not captured by the drop/corrupt/spoof parameters (perhaps the energy drain on BlueEcm's burnthrough that offsets Red's ECM cost).

Branch: `research/iter-5-ghost-no-ecm`
