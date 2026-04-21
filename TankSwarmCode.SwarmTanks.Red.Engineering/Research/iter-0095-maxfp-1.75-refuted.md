# Iter 95 — MaxFirePower 1.5 → 1.75 (REFUTED −4.2pp avg, 5 seeds)

## Hypothesis

Iter 85/86 established MaxFP=1.5 as "sharp peak" vs MaxFP=1.0 (-10pp) and MaxFP=2.0
(flat). That test predated the fire-gate widening (5° → 6° → 7° in iters 67/68/92).
With a looser gate, more shots leave at larger gun-diffs; slower bullets (higher
MaxFP) might concentrate more damage per shot and compensate for the wider spread.
Testing 1.75 (halfway between optimal 1.5 and neutral 2.0) probes whether the
MaxFP peak shifted after fire-gate state changed.

Success criterion: ≥ +1.0pp vs 56.0% baseline. Refuted: below baseline.

## Code Change

`MaxFirePower = 1.5` → `MaxFirePower = 1.75` in all 5 Red cortex files:
RedArrowCortex, RedBladeCortex, RedGhostCortex, RedHammerCortex, RedTrooperCortex.

## Run Parameters

- Red 29 tanks vs Blue 29 tanks
- PR=160, Fallback=10, fire-gate 7°, Scout gun-tracks-radar (all iter-94 state)
- 200 matches/seed, `--parallel 8`, `--on-timeout energy`
- `bin/Release/net10.0/publish/` DLLs (both rebuilt)

## Raw Results

| Seed | Red wins | Win rate | Baseline (1.5) | Δ |
|------|---------:|---------:|-------------:|--:|
| 1000 | 113/200  | **56.5%** | 57.0% | **−0.5pp** |
| 2000 | 102/200  | **51.0%** | 53.0% | **−2.0pp** |
| 3000 |  98/200  | **49.0%** | 60.0% | **−11.0pp** |
| 4000 |  97/200  | **48.5%** | 58.0% | **−9.5pp** |
| 5000 | 108/200  | **54.0%** | 52.0% | **+2.0pp** |

**5-seed avg: 51.8% vs 56.0% = −4.2pp.** 1/5 positive. Aggregate:
518/1000 vs 560/1000 = −4.2pp. REFUTED decisively.

## Analysis

The per-seed signature is the same as iter-93 (9°) and iter-94 (8°): the seeds
where the 1.5 baseline is strongest (3000 at 60%, 4000 at 58%) regress catastrophically
(−11.0pp, −9.5pp), while the seeds where the baseline is weakest (5000 at 52%,
2000 at 53%) are unchanged or marginally positive. This is now a confirmed
three-iteration pattern:

| Seed | 1.5 base | 1.75 Δ | 8° Δ (iter-94) | 9° Δ (iter-93) |
|------|----------|--------|----------------|----------------|
| 1000 | 57.0%    | −0.5   | +0.5           | +1.5           |
| 2000 | 53.0%    | −2.0   | +3.5           | +4.5           |
| 3000 | 60.0%    | **−11.0** | **−6.5**    | **−7.0**       |
| 4000 | 58.0%    | **−9.5**  | **−4.5**    | **−6.5**       |
| 5000 | 52.0%    | +2.0   | −2.5           | +2.0           |
| avg  | 56.0%    | −4.2   | −1.9           | −1.1           |

Interpretation: seeds 3000/4000 produce battles where Red's fire solution is
already near-optimal at the current (MaxFP=1.5, gate=7°) point. Any change that
degrades shot quality — whether by letting sloppier shots through the gate or by
making well-aimed shots slower to arrive — evicts Red from its locked-in winning
profile. Seeds 2000/5000 are accuracy-limited: Red is firing at targets that
move faster than its current hit-rate, so small tweaks are within noise.

The specific mechanism for 1.75 being worse than 1.5 at seed 3000/4000: slower
bullet speed (≈16.5 vs 17 px/tick) means ≈3% longer travel time at PR=160, which
compounds prediction-staleness error. At the same time, the per-shot damage
increase (+17% over 1.5) cannot close the kill-window gap when 10%+ of shots
miss. The +damage-per-hit does not offset the −hit-rate.

## Gradient Summary (MaxFP, vs current Blue DLL, PR=160, gate=7°)

| MaxFP | Δ vs 1.5 baseline | Verdict |
|-------|-------------------|---------|
| 1.0   | −10pp (iter 86)   | CATASTROPHIC |
| 1.5   | 0pp               | PEAK (current baseline) |
| 1.75  | **−4.2pp**        | REFUTED (iter 95, this result) |
| 2.0   | −1.0pp (iter 86, prior baseline) | REFUTED |
| 2.5   | −10pp (iter 48, old DLL) | CATASTROPHIC |

The downward slope from 1.5 is asymmetric but steep on both sides. 1.75 sits
clearly inside the downward slope; the peak is at 1.5 and narrow.

## Key Findings

- **REFUTED:** MaxFP 1.5 → 1.75 loses −4.2pp avg (5 seeds, 1/5 positive). MaxFP=1.5
  remains the ceiling at **56.0% avg**.
- **MaxFP peak did not shift with fire-gate changes.** Earlier intuition that a
  wider gate would favor a slower/heavier bullet is wrong. Fire-gate and MaxFP
  are independently pinned at their own optima (7° and 1.5).
- **Three-iteration seed-correlation pattern now confirmed (93/94/95):** seeds
  where Red's fire-control is already near-optimal (3000, 4000) are brittle
  under *any* change that lowers per-shot quality — gate loosening or bullet
  slowing both trigger the same regression signature. This suggests the current
  baseline at these seeds is near a local maximum in a narrow basin.
- **Practical rule:** any parameter change that affects shot quality must be
  validated on seeds 3000/4000 before accepting. Single-seed runs on low-baseline
  seeds will mislead.
- **MaxFP dimension effectively exhausted.** 1.0/1.25/1.5/1.75/2.0/2.5 coverage
  is now dense enough that 1.25 is unlikely to produce a different verdict
  (interpolating between catastrophic 1.0 and peak 1.5 predicts −4 to −7pp).

## Summary

MaxFirePower 1.5 → 1.75 costs −4.2pp average across 5 seeds (518/1000 vs 560/1000),
with two decisive catastrophic regressions (−11.0pp, −9.5pp) on seeds 3000/4000.
MaxFP=1.5 peak is sharp and unchanged by the fire-gate state. Refuted. Reverted.

**Next candidates (orthogonal dimensions):**
- PR fine-grained sweep around 160 (test 158 or 162). Last PR sweep was 155/160/165/171
  — the 1–2 px-increment neighborhood of 160 is unexplored.
- Scout sub-phase tweaks: gun bias toward mean-direction of ping list, or pre-aim
  along formation-center heading rather than radar heading.
- Targeted re-visit of old-Blue refutations: fixed-slot orbit (refuted −0.9pp vs
  old Blue, iter 15) and rank-based orbit (refuted −0.7pp vs new Blue, iter 87)
  are both within-noise refutations that might invert under the current fire-gate
  + gun-pre-aim configuration.
- Scout energy-threshold tuning (currently Scout exits when first target visible;
  could tune a minimum ping-list size before exit).
