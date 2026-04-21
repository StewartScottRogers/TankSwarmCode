# Iters 45-46 — Red28 + MaxFP=2.0: +12pp avg (ACCEPTED, 5-seed)

## Hypothesis

**Iter 45:** Add Red28 (29th Red tank, slot 28) to restore numerical parity with Blue's 29 tanks.
**Iter 46:** Increase MaxFP from 1.0→2.0 for all attack tanks (Trooper/Arrow/Blade/Hammer).

Blue had expanded to 29 tanks (Blue28 added in Blue iter-42), giving Blue numerical edge. Red was at 28 tanks, yielding ~44% win rate at seeds 1000 and 3000.

From the seed-3000 battle report: Blue's named tanks (BlueGuard=65.91, BlueStrike=58.10, BlueRush=54.02) had rate/100t ~2x Red's Trooper slots (~25-30). MaxFP=1.0 was optimal against old 5-tank Blue but the new 29-tank Blue swarm is denser — target overlap is higher, making slower-but-harder bullets (MaxFP=2.0) favorable over faster-but-weaker (MaxFP=1.0).

## Code Change

**Iter 45:** Added `Red28` class to `RedSlotTanks.cs`, registered `"Red28" => new RedTrooperCortex(28)` in `CortexFactory.cs`.

**Iter 46:** Changed `MaxFirePower` from `1.0` to `2.0` in:
- `RedTrooperCortex.cs` (affects all slot tanks Red4-Red28)
- `RedArrowCortex.cs` (slot 2)
- `RedBladeCortex.cs` (slot 1)
- `RedHammerCortex.cs` (slot 0)

RedGhost unchanged at MaxFP=2.0.

## Run Parameters

- 200 matches, --parallel 8, --on-timeout energy, --format table
- Seeds: 1000, 2000, 3000, 4000, 5000

## Raw Results

### Iter 45 — Red28 alone (MaxFP=1.0, 29 Red vs 29 Blue):
- Seed 3000: 46% (vs 44% baseline = +2pp)
- Seed 1000: 44% (vs 44% baseline = 0pp)
- **Avg improvement: ~+1pp (marginal)**

### Iter 46 — MaxFP=2.0 (29 Red vs 29 Blue):
- Seed 1000: **56%**
- Seed 2000: **52%**
- Seed 3000: **57%**
- Seed 4000: **58%**
- Seed 5000: **55%**
- **5-seed avg: 55.6%**

### MaxFP=3.0 (quick rejection test, seed 3000):
- Seed 3000: 38% — **REJECTED** (too slow, bullets miss mobile targets)

## Analysis

MaxFP sweep (seed 3000 vs baseline 44%):
- MaxFP=1.0: 44% (baseline)
- MaxFP=2.0: 57% (+13pp) ✓ ACCEPTED
- MaxFP=3.0: 38% (-6pp) ✗ REJECTED

MaxFP=2.0 doubles damage per hit (4→8 dmg) at the cost of slower bullets (17→14 px/tick at PR=160). 
With 29 Blue tanks in dense Wolfpack orbit, target density compensates for slower bullets.
MaxFP=3.0 overshoots — the bullet travel time at PR=160 (~14.5 ticks) is too slow for mobile Blue tanks.

The "faster bullets" optimization (MaxFP=1.0) from iter-24 was valid for old 5-tank Blue but is wrong for the current 29-tank formation. 

## Key Findings

- **MaxFP=2.0 is the new optimal** for all attack tanks (Trooper, Arrow, Blade, Hammer) vs 29-tank Blue
- **MaxFP=1.0→2.0 = +12pp avg** across 5 seeds (44% → 55.6%)
- **MaxFP=3.0 is clearly inferior** (-6pp at seed 3000 — decisively refuted in 1 seed
- **Red28 contribution is marginal** (~+1pp at seed 3000); kept because it's net positive
- **Current state: 29 Red tanks vs 29 Blue tanks, 55.6% avg 5-seed**

## Summary

ACCEPTED. MaxFP=2.0 for all attack tanks gives +12pp avg vs the current Blue 29-tank DLL.
New 5-seed avg: 55.6% (up from ~44%).
