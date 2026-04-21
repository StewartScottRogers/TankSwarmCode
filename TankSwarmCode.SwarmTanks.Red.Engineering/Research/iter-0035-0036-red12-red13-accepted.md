# Iter 35-36 — Red12 + Red13: Numerical Superiority (ACCEPTED)

## Hypothesis

If matching Blue's 12 tanks gave +47.8pp, then exceeding Blue's count (13, 14 tanks)
should yield further gains via numerical superiority. Each additional tank adds DPS and
orbit coverage.

## Code Changes

- Iter 35: Added Red12 (slot 12, MaxFP=1.0, PR=160) — 13 Red tanks vs Blue 12
- Iter 36: Added Red13 (slot 13, MaxFP=1.0, PR=160) — 14 Red tanks vs Blue 12

## Raw Results

### Iter 35 (13 tanks): baseline was 62.8% (12 tanks)

| Seed | Red % | Delta |
|------|-------|-------|
| 1000 | 66%   | +8pp  |
| 2000 | 70%   | +6pp  |
| 3000 | 68%   | +6pp  |
| 4000 | 68%   | +5pp  |
| 5000 | 64%   | -3pp  |
| **Avg** | **67.2%** | **+4.4pp** |

### Iter 36 (14 tanks): baseline was 67.2% (13 tanks)

| Seed | Red % | Delta |
|------|-------|-------|
| 1000 | 70%   | +4pp  |
| 2000 | 74%   | +4pp  |
| 3000 | 69%   | +1pp  |
| 4000 | 78%   | +10pp |
| 5000 | 70%   | +6pp  |
| **Avg** | **72.2%** | **+5.0pp** |

## Analysis

Numerical superiority continues to yield gains. Each additional tank adds:
- One more DPS source orbiting at 160px
- One more orbital slot (redistributing coverage as tanks die)
- Redundancy: Red can absorb more losses before losing combat effectiveness

Pattern: 12→13 tanks: +4.4pp; 13→14 tanks: +5.0pp. Returns not yet diminishing.

## Summary

**ACCEPTED: +9.4pp cumulative (62.8% → 72.2% over 2 iters)**. Continue adding tanks.
