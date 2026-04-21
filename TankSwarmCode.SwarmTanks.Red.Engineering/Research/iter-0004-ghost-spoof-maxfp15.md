---
Iteration: 4
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-4: Ghost Spoof Mode + MaxFP 1.5 (REFUTED)

## Hypothesis

JamAndSpoof disables Ghost's radar, firing, and radio. Switching to Spoof keeps all these active while still generating ghost contacts. With radar active, the enemy check stays true continuously (no pulse) and Ghost can fire. Combined with MaxFP=1.5, Ghost becomes a real attacker + jammer.

## Code Change

- Ghost OffensiveEcmMode: JamAndSpoof → Spoof
- Ghost MaxFirePower: 0.1 → 1.5
- Override ECM: `ctx.SetEcm(EcmMode.Spoof)` instead of JamAndSpoof

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 65 wins (32%) | Blue: 135 wins (68%)

RedGhost Rate/100t: 1.22 (firing now)
RedGhost EGained: 19.1
RedGhost Survival: 134/200 (much better)
BlueSharp: Co-MVP, 117/135
```

## Analysis

**REFUTED. Red 32% (down from 50%). Reverted.**

Spoof alone is worse than JamAndSpoof despite Ghost being more capable:
1. JamAndSpoof's Jam component makes Ghost nearly invisible (20% scan rate vs 100% with Spoof)
2. Blue can freely target Ghost with Spoof → Ghost dies as efficiently but ECM value drops
3. The 50% drop + 30% corrupt rate of Jam is more valuable than Ghost firing at 1.5 power
4. "Invisible Ghost doing ECM" > "Visible Ghost fighting + spoofing"

## Key Findings

1. Ghost's invisibility under JamAndSpoof is MORE valuable than Ghost firing
2. Spoof generates ghost contacts but Ghost remains a full target
3. JamAndSpoof is the correct mode for Ghost — it's designed for "dedicated ECM tanks with no cannon"

## Summary

**Red 32%** — regression. Reverted. JamAndSpoof + MaxFP 0.1 is the right Ghost config.
