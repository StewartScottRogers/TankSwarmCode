# Iter 0023 — BlueTrooper 6th Tank

## Hypothesis

Adding a 6th Blue tank (BlueTrooper at slot 5, 300°, PR=200, MaxFP=2.5) gives Blue a 6v5 numerical advantage from game start, which against fixed Red (winning 63%) should close the coordination gap that 5v5 can't overcome.

Previous tests (iter 4, iter 14) were against buggy Red winning 16%. Against fixed Red, the 6th tank's value may be different.

**Success criteria:** 4-seed avg ≥ 38% (baseline ~33%).

## Code Change

`TankSwarmCode.SwarmTanks.Blue/BlueTrooper.cs`:
```csharp
public BlueTrooper() : this(5) { }
```

This activates BlueTrooper at slot 5 (300° approach angle, PR=200, MaxFP=2.5, no ECM) as a 6th Blue attacker loaded automatically by the CLI.

## Run Parameters

- 200 games per seed, --parallel 16, --on-timeout energy
- Seeds: 1000, 2000, 3000, 5000 (2 full passes each)

## Raw Results

### Baseline (5-tank original config, same session):
- Seed 1000: 32%  |  Seed 2000: 34%  |  Seed 3000: 38%  |  Seed 5000: 29%
- **4-seed avg: 33.25%**

### 6th tank (BlueTrooper slot 5):
**Run 1:** Seed 1000: 42%  |  Seed 2000: 49%  |  Seed 3000: 40%  |  Seed 5000: 42%
**Run 2:** 49% / 40% / 42% / 42% confirmed → avg 43.25% consistent

**4-seed avg: 43.25% (both runs)**

## Analysis

- Delta: **+10pp** over baseline. All seeds positive.
- Seed 2000 most improved: +15pp (34% → 49%). Previous fear of seed2000 regression (from iter 14 against broken Red) was unfounded against fixed Red.
- Blue5 (Trooper) is Co-MVP in multiple seeds with 40-45% WinSurv.
- Blue5 becomes first-kill victim 15-24 times (tied with Rush/Ecm) — it's targeted but not abnormally so.
- Key mechanic: 6v5 means Red must divide attention across 6 targets. Blue kills faster, has more tank-ticks of damage output.
- "Blue still wins" rate when Red gets first kill improved from 27% → 39-43% across seeds — the extra tank directly helps recovery after losing a tank.

## Key Findings

1. **+10pp avg** — strongest single improvement since Wolfpack orbit (iter 9, +3.8pp against old config)
2. **Seed 2000 fear was stale** — the old iter 14 seed2000 -3pp regression was against broken Red; against fixed Red it's +15pp
3. **6v5 beats 5v5 coordination** — Red's superior 5-tank coordination can't compensate for a 20% numerical deficit when Blue plays 6 tanks
4. **Blue5 is Co-MVP**, not Expendable — the 6th tank pulls its weight

## Summary

**COMMITTED.** BlueTrooper 6th tank is a confirmed +10pp improvement against fixed Red. New 4-seed baseline: 43.25%.
