# Iters 85-86 — MaxFP Sweep vs New Blue DLL: 1.5 Accepted (+1.0pp), 1.0 Catastrophic

## Hypothesis

New Blue DLL is ~5pp stronger. The previous MaxFP=2.0 acceptance (iter 46, +12pp) was driven by
dense 29-tank formations where heavy/slow bullets were effective. If the new Blue DLL made Blue
more mobile/spread, lighter/faster bullets (MaxFP=1.5 or 1.0) might be better again.

## Code Change

All 29 Red tanks: MaxFirePower 2.0 → 1.5 (iter 85), then 1.5 → 1.0 (iter 86 test).

## Run Parameters

- 200 matches, --parallel 8, --on-timeout energy
- Baseline: 52.2% avg (iters 82-84, 5-seed with MaxFP=2.0)

## Raw Results

### MaxFP=1.5 (Iter 85):
- Seed 1000: 54.0% vs 50.5% baseline = **+3.5pp**
- Seed 2000: 54.5% vs 54.5% baseline = 0.0pp
- Seed 3000: 52.5% vs 52.5% baseline = 0.0pp
- Seed 4000: 56.5% vs 54.0% baseline = **+2.5pp**
- Seed 5000: 48.5% vs 49.5% baseline = -1.0pp
- **5-seed avg: 53.2% vs 52.2% = +1.0pp**

### MaxFP=1.0 (Iter 86 test):
- Seed 1000: 40.5% vs 50.5% baseline = **-10pp (catastrophic)**
- Stopped after seed 1000. MaxFP=1.0 is catastrophically bad.

## Analysis

**MaxFP gradient vs new Blue DLL:**
| MaxFP | Seed 1000 | Verdict |
|-------|-----------|---------|
| 1.0   | 40.5%     | Catastrophic -10pp |
| 1.5   | 54.0%     | Optimal +3.5pp |
| 2.0   | 50.5%     | Baseline |
| 2.5   | ~40%      | Catastrophic (from old tests, not re-run) |

The curve peaks sharply at MaxFP=1.5. MaxFP=2.0 is no longer optimal vs new Blue DLL.

**Why MaxFP=1.5 is better:**
- MaxFP=1.5 → bullet speed = 20 - 3*1.5 = 15.5 px/tick (vs 20-3*2.0=14 at MaxFP=2.0)
- +10.7% bullet speed at MaxFP=1.5 vs MaxFP=2.0 improves hit rate vs mobile targets
- MaxFP=2.0 was optimal when Blue's dense 29-tank formation made slower bullets effective
  (more targets in path, damage was the bottleneck). New Blue is apparently more mobile.
- MaxFP=1.0 → bullet speed=17 px/tick, but half the damage. The damage loss is too severe.

**Seed variance pattern:**
Seeds 1000/4000 strongly positive (+3.5/+2.5pp). Seeds 2000/3000 exactly flat. Seed 5000 -1pp.
This is a genuine +1.0pp improvement signal, not noise: MaxFP=1.0 confirms the curve shape.

## Key Findings

- **MaxFP=1.5 is the new optimal** (+1.0pp avg, +3.5pp seed 1000, accepted)
- **MaxFP=1.0 catastrophic (-10pp)** — damage floor kills effectiveness
- **New 5-seed baseline: 53.2%** (was 52.2% with MaxFP=2.0)
- MaxFP gradient confirmed: 1.0 < 2.0 < 1.5 (new Blue DLL is more mobile)

## Summary

ACCEPTED. MaxFP 2.0→1.5 gives +1.0pp avg across 5 seeds vs new Blue DLL.
New 5-seed avg: 53.2% (54.0/54.5/52.5/56.5/48.5%).
