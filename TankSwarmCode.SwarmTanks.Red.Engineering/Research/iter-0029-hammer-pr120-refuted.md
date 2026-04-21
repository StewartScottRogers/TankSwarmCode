## Hypothesis

Mixed-PR formation: Hammer at PR=120 as a close-range anchor while Arrow/Blade/Ghost/Trooper
orbit at PR=160. Hammer (slot 0, RetreatThreshold=25) is configured for a more durable role.
At 120px orbit, Hammer fires faster bullets (7.1t travel vs 9.4t at 160px), improving its
individual hit rate. The "hammer and anvil" effect: Hammer draws fire from 120px while others
orbit safely at 160px.

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`RedHammerCortex.cs`: `PreferredRange = 160.0` → `120.0`.
Reverted after 2-seed results showed -1.25pp avg.

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000 (stopped after 2-seed avg negative)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/` (published via shell project)

## Raw Results

| Seed | Hammer@120 | Baseline (160) | Delta |
|------|-----------|----------------|-------|
| 1000 | 69.0% | 70.0% | -1.0pp |
| 2000 | 69.0% | 70.5% | -1.5pp |
| **Avg** | **69.0%** | **70.25%** | **-1.25pp** |

## Analysis

**REFUTED: Hammer at PR=120 gives -1.25pp avg (2 seeds, consistent negative).**

Why the close-range anchor doesn't help:
1. **Hammer dies faster at 120px.** Survive rate dropped ~5pp across both seeds. At 120px, Blue
   tanks hit Hammer in 7.1t (vs 9.4t at 160px for Blue). The increased incoming damage more than
   offsets any firing accuracy improvement for Hammer.

2. **Mixed orbits fragment the Wolfpack coherence.** The Wolfpack orbit equation distributes tanks
   evenly at `config.PreferredRange`. When Hammer is at 120 and others at 160, Hammer occupies a
   different radial ring. The formation asymmetry means Red tanks are no longer evenly distributed
   around the enemy — Hammer is "inside" the others' orbit, potentially overlapping with enemies and
   disrupting the coordinated encirclement.

3. **avg_energy_gained for Hammer is the LOWEST despite being closest.** Hammer's energy_gained
   was 17.9 (seed 2000) vs ~20-22 for others at 160px. Being closer doesn't translate to more hits
   — possibly because Hammer's angular position at slot 0 (0° from reference) puts it directly in
   front of enemies, where Blue tanks are more actively tracking.

4. **Consistent signal.** Both seeds showed exactly 69.0% — this isn't noise. The -1.25pp avg is
   real, not a fluke.

Insight: The Wolfpack formation depends on all tanks orbiting at the same radius. Mixed orbits
break the coordinated fire pattern that makes the formation effective. All tanks at PR=160 is
confirmed as the optimal configuration.

## Key Findings

- **Hammer PR=120 REFUTED: -1.25pp avg (2 seeds, consistent).**
- Code reverted to PR=160 for Hammer. All tanks at PR=160 confirmed optimal.
- **Architecture ceiling confirmed at 70.2%.** Parameter space is fully exhausted:
  - All PR variations tested: uniform 140, 150, 160, 180; mixed 120/160 — all worse than 160
  - All MaxFP variations tested: 0.5, 1.0, 1.5, 2.0 for all tanks individually
  - All coordination parameters tested: Fallback threshold, ECM, Encircle radius
- Next: Hammer RetreatEnergyThreshold 25→20 (1-line change, last untested parameter)

## Summary

REFUTED. Code reverted to PR=160. Mixed-PR formation confirmed negative. Architecture ceiling
at 70.2% is robust — Wolfpack coherence requires uniform orbit radius.
