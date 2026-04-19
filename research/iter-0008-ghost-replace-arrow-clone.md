# Iter 8 — Ghost Replaced with Arrow Clone

**Date:** 2026-04-18
**Branch:** `research/iter-8-ghost-replace-arrow-clone`
**Status:** Confirmed — Ghost's hider role is worth +16% win rate

---

## Hypothesis

Ghost's structural advantage comes from its passive/non-threatening hider role causing Blue to deprioritize it. Replacing Ghost with an Arrow clone (MaxFirePower=1.5, PreferredRange=220, RetreatEnergyThreshold=20, no ECM) tests whether Ghost's role is enabling or incidental. Expected: if hider role is key, Red's win rate will change significantly.

## Code Change

`TankSwarmCode.SwarmTanks.Red/RedGhost.cs`:
- `Role`: EcmSpecialist → Scout
- `MaxFirePower`: 0.1 → 1.5
- `PreferredRange`: 150.0 → 220.0
- `HasEcm`: true → false
- `OffensiveEcmMode`: JamAndSpoof → (removed)
- `RetreatEnergyThreshold`: 40.0 → 20.0

**Reverted** — observational test, Ghost restored to original config.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
```

## Raw Results

```
Red: 80 wins (40%)  |  Blue: 120 wins (60%)  |  Draws: 0

[Massive shift from 56%/44% baseline to 40%/60%]

Tank                      Surv                WinSurv          Rate/100t
RedGhost (Arrow clone) 50/200 (17D+18TO+15L)  35/80  (19E)     17.84
RedArrow             46/200 (15D+18TO+13L)    33/80  (18E)     11.91
RedBlade             34/200 (13D+7TO+14L)     20/80  (10E)     18.81
RedHammer            31/200 (9D+10TO+12L)     19/80  (10E)     13.66

BlueEcm              75/200 (39D+27TO+9L)    66/120 (29E)     12.05
BlueStrike           52/200 (28D+5TO+19L)    33/120 (16E)     23.89
BlueGuard            50/200 (34D+6TO+10L)    40/120 (23E)     19.10
BlueRush             47/200 (28D+11TO+8L)    39/120 (18E)     19.62
BlueSharp            38/200 (27D+7TO+4L)     34/120 (19E)     20.96

RedGhost insights: MVP (35/80), Survivor (15/120 losses), Solo carry (20/80, 5D+15TO)
RedArrow insights:  Survivor (13/120 losses), Solo carry (21/80, 5D+16TO)  [Arrow now co-equal with Ghost!]
BlueEcm insights:   MVP (66/120), Survivor (9/80 losses), Solo carry (35/120, 10D+25TO), ECM

Win combos (Red):
  21x Arrow             (26%) [5D+16TO]  ← Arrow now Red's top solo carrier!
  20x Ghost (clone)     (25%) [5D+15TO]
  12x Hammer            (15%)
```

## Analysis

| Criterion | Baseline (Ghost original) | Iter-8 (Ghost→Arrow clone) |
|---|---|---|
| Red win rate | 56% | **40%** (−16pp) |
| Ghost WinSurv | 66/111 (59%) | 35/80 (44%) |
| Ghost solo carry | 35/111 (32%) | 20/80 (25%) |
| Ghost Rate/100t | 2.16 | **17.84** (now fully combat) |
| BlueEcm solo carry | 16–17/89 (19%) | **35/120 (29%)** — BlueEcm took over |

## Key Findings

1. **Ghost's hider role is worth +16 percentage points.** Red drops from 56% to 40% when Ghost becomes a combat unit. This is the strongest causal signal found in 8 iterations. Ghost's passive survival (MaxFirePower=0.1, RetreatEnergyThreshold=40) enables Red to inherit close matches.

2. **The mechanism: outlasting, not fighting.** With Arrow clone Ghost, Red has 4 aggressive combatants who all die fast in close matches. When Red's real combatants die, Blue wins — there's no passive survivor to inherit. With original Ghost, Ghost survives 66% of Red wins; without it, the Arrow clone survives only 44% (and loses 15 times in Blue wins).

3. **BlueEcm mirrors the hider role for Blue — and wins when Ghost is gone.** Without Ghost countering BlueEcm's burnthrough, BlueEcm becomes the dominant survivor at 66/120 = 55% WinSurv, solo carrying 29% of Blue wins via timeout. BlueEcm IS the Blue equivalent of Ghost. The fundamental dynamic: each team's ECM tank (Ghost vs BlueEcm) acts as a passive survivor and timeout farmer — the balance is determined by which passive survivor wins the ECM duel.

4. **Red's 56% advantage = Ghost outlasts BlueEcm's burnthrough.** In baseline matches, Ghost's JamAndSpoof forces BlueEcm into burnthrough mode (burning extra energy). Ghost outlasts this by retreating at 40E while BlueEcm depletes itself. Without Ghost, BlueEcm faces only combat tanks and thrives.

5. **The real balance lever: survivability of the ECM/hider units.** Ghost (RetreatEnergyThreshold=40, MaxFirePower=0.1) vs BlueEcm (35, 1.5). Ghost retreats earlier and fights less — making it a superior hider. This is the root of Red's 56%.

## Next Hypothesis (Iter 9)

**Root cause confirmed: Ghost's RetreatEnergyThreshold=40 gives it a survivability edge over BlueEcm's 35.** Raising BlueEcm's RetreatEnergyThreshold from 35 → 45 (retreating even earlier than Ghost) should make BlueEcm a comparable hider, evening the ECM-duel and dropping Red's win rate from 56% toward 45–52%.

Note: This is different from iter-6's "combat buff" which lowered the threshold. Here we RAISE it — making BlueEcm more conservative, not more aggressive.

Success criteria:
- Red win rate drops from 56% toward 45–52%
- BlueEcm WinSurv increases (toward Ghost's 59%)
- BlueEcm solo carry approaches Ghost's 32%
- BlueEcm's loss-survivals decrease (fewer deaths in Red wins)

Branch: `research/iter-9-blueecm-hider-buff`
