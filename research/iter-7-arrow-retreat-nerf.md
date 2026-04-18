# Iter 7 — RedArrow Retreat Nerf

**Date:** 2026-04-18
**Branch:** `research/iter-7-arrow-retreat-nerf`
**Status:** No Effect (Inconclusive)

---

## Hypothesis

Red's 56% advantage is driven by RedArrow's extreme first-kill aggression (RetreatEnergyThreshold=20). Arrow gets first blood in 60% of matches consistently. Raising RedArrow's RetreatEnergyThreshold from 20 → 35 reduces Arrow's first-kill frequency, keeps matches numerically even longer, and drops Red's win rate from 56% toward 45–52%.

## Code Change

`TankSwarmCode.SwarmTanks.Red/RedArrow.cs`:
- `RetreatEnergyThreshold`: 20.0 → 35.0

**Reverted** — no effect, code restored.

**Note:** Initial iter-7 test run accidentally used the iter-6 buffed Blue dll (BlueEcm MaxFirePower=2.5, RetreatEnergyThreshold=25) and returned 62%. After rebuilding Blue with baseline config, the correct result was obtained.

## Run Parameters

```
--batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
Arena: default (800×600)
Blue: baseline config (BlueEcm MaxFirePower=1.5, RetreatEnergyThreshold=35) — rebuilt before final run
```

## Raw Results (Correct Run)

```
Red: 111 wins (56%)  |  Blue: 89 wins (44%)  |  Draws: 0

Win quality: Red: +68E decisive (76), 35 TO +19E (31% via TO)
             Blue: +74E decisive (52), 37 TO +14E (41% via TO)
Decisive med: Red 343t / Blue 129t (Blue 2.7x faster)

Tank                      Surv                WinSurv          Rate/100t
RedGhost             71/200 (54D+12TO+5L)   66/111 (67E)      2.22
RedArrow             49/200 (26D+12TO+11L)  38/111 (18E)     12.98
RedBlade             37/200 (16D+6TO+15L)   22/111 (10E)     14.87
RedHammer            33/200 (18D+9TO+6L)    27/111 (10E)     15.68

BlueEcm              64/200 (36D+18TO+10L)  54/89  (31E)      9.98
BlueGuard            42/200 (28D+6TO+8L)    34/89  (21E)     13.14
BlueStrike           40/200 (23D+7TO+10L)   30/89  (17E)     14.31
BlueRush             37/200 (23D+6TO+8L)    29/89  (19E)     13.90
BlueSharp            32/200 (24D+5TO+3L)    29/89  (20E)     15.53

RedGhost insights: MVP (66/111), Survivor (5/89 losses), Solo carry (35/111, 26D+9TO), ECM
First kills: Red: Arrow:36  Blade:31  Hammer:28  Ghost:25  (unchanged from baseline)
```

## Analysis

| Criterion | Threshold | Baseline | Iter-7 | Pass? |
|---|---|---|---|---|
| Red win rate | 45–52% | 56% | 56% | ❌ |
| Arrow first kills | <28 | ~35 | 36 | ❌ |
| Red first-blood frequency | <50% | 60% | 60% | ❌ |

## Key Findings

1. **Arrow's retreat threshold does not affect first-kill count.** Arrow gets first blood because it's fast and engages early — first kills happen at tick ~24 when all tanks have full energy (100E). The retreat threshold only fires when energy drops below 35; at tick 24, Arrow still has ~80+E. The parameter doesn't govern first-kill behavior at all.

2. **Arrow's solo pattern shifted but not count.** Arrow solos went from 12D+6TO → 10D+10TO (more timeout solos, fewer decisive). Arrow retreats earlier, grinds timeouts instead of decisive wins — but still carries the same number of wins. Red's total win count is unchanged.

3. **56% is remarkably stable.** Seven iterations of parameter changes (ECM spoof radius, jam drop/corrupt, HasEcm=false, BlueEcm combat buff, Arrow retreat) have all returned 56% (or 62% when accidentally using buffed Blue dll). Red's structural advantage is deep.

4. **Process error note: Always rebuild both dlls after reverting.** After reverting a dll-affecting code change, explicitly rebuild and confirm the published artifact before running benchmarks. The iter-7 initial run with stale Blue dll (showing 62%) was caught because results matched iter-6 exactly.

## Next Hypothesis (Iter 8)

**Ghost is Red's hidden structural advantage via FormationSlot position and hider role, not ECM.** Since ECM is cosmetic (iter-5), Ghost's advantage must be behavioral — specifically, Ghost's passive non-threatening behavior causes Blue to deprioritize it while eliminating real combatants. If Ghost is replaced with a copy of RedArrow (same MaxFirePower=1.5, PreferredRange=220, RetreatEnergyThreshold=20), Red gains a 5th aggressive combatant. This tests: does Ghost's passive survival role HURT Red (Blue can focus on 3 real threats instead of 4) or HELP Red (Ghost inherits wins cheaply)?

If Red's win rate INCREASES when Ghost→Arrow clone: Ghost's passive survival is actually costly — Red would be better with 4 combatants. Need to nerf a different aspect.
If Red's win rate STAYS at 56%: The win rate is driven by something other than Ghost's role entirely.
If Red's win rate DECREASES: Ghost's passive role genuinely enables wins — it draws Blue attention, buys time for Arrow/Blade/Hammer to prevail.

Branch: `research/iter-8-ghost-replace-arrow-clone`
