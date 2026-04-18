# Iter 9 — BlueEcm Hider Buff (Retreat Threshold)

**Date:** 2026-04-18
**Branch:** `research/iter-9-blueecm-hider-buff`
**Status:** Refuted — retreat threshold is not the binding constraint

---

## Hypothesis

Ghost's RetreatEnergyThreshold=40 gives it a survivability edge over BlueEcm's 35. Raising BlueEcm's RetreatEnergyThreshold from 35 → 45 (retreating earlier than Ghost) makes BlueEcm a comparable hider, evening the ECM duel and dropping Red's win rate from 56% toward 45–52%.

## Code Change

`TankSwarmCode.SwarmTanks.Blue/BlueEcm.cs`:
- `RetreatEnergyThreshold`: 35.0 → 45.0

**Reverted** — hypothesis refuted, code restored.

**Note:** Initial accidental run (before rebuilding Red after iter-8 revertal) used stale Arrow-clone Ghost dll and returned 40% Red. Correct run used rebuilt baseline Red dll (Ghost original config).

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
Red: rebuilt with baseline Ghost config after iter-8 revert
```

## Raw Results (Correct Run)

```
Red: 112 wins (56%)  |  Blue: 88 wins (44%)  |  Draws: 0

Win quality: Red: +68E decisive (77), 35 TO +17E (31% via TO)
             Blue: +72E decisive (55), 33 TO +15E (37% via TO)
Decisive med: Red 343t / Blue 125t (Blue 2.7x faster)
First kills: Red: Arrow:36  Blade:31  Hammer:28  Ghost:25
             Blue: Sharp:24  Strike:17  Guard:15  Ecm:12  Rush:12
```

## Analysis

| Criterion | Threshold | Baseline | Iter-9 | Pass? |
|---|---|---|---|---|
| Red win rate | 45–52% | 56% | 56% | ❌ |

## Key Findings

1. **Retreat threshold is not the binding constraint for Ghost's survival.** Even with BlueEcm retreating at 45E (earlier than Ghost's 40E), the balance is unchanged. Ghost's survival advantage comes from a different mechanism.

2. **MaxFirePower is the binding constraint.** Ghost shoots at MaxFirePower=0.1 per shot — essentially zero energy drain from combat. BlueEcm shoots at 1.5 per shot. Every time BlueEcm fires in a match, it loses 1.5E; Ghost loses 0.1E. Over a long match, this compounds dramatically. BlueEcm burns through energy fighting while Ghost conserves it. The retreat threshold matters less if BlueEcm has already spent its energy on combat before the threshold fires.

3. **Process error (second iter in a row): Always rebuild both dlls after reverting.** The stale Arrow-clone Ghost dll from iter-8 contaminated the first iter-9 run. Established rule: after reverting any code change, rebuild that dll before running benchmarks. Consider: always run a "sanity check" match count comparison against known baseline before recording results.

## Next Hypothesis (Iter 10)

**BlueEcm's MaxFirePower=1.5 is the binding constraint.** It drains energy through combat, preventing survival even with a high retreat threshold. Lowering BlueEcm's MaxFirePower from 1.5 → 0.1 (matching Ghost's near-zero combat firepower) makes BlueEcm a true hider: it conserves energy for ECM and survival rather than draining it through shooting. This should make BlueEcm survive more Red wins and inherit close-match wins via ECM timeout.

Success criteria:
- Red win rate drops from 56% toward 45–52%
- BlueEcm WinSurv increases substantially (toward Ghost's 59%)
- BlueEcm solo carry approaches Ghost's 32%
- BlueEcm Rate/100t drops toward 0 (confirms near-zero combat)

Branch: `research/iter-10-blueecm-firepower-nerf`
