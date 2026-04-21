---
Iteration: 5
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-5: Burnthrough for All Attackers (REFUTED)

## Hypothesis

Red's attackers never run Burnthrough. IsEnemyEcmActive never triggers (no Red tank broadcasts EcmAlert). BlueEcm at rate 1.55 is jamming Red's targeting unchallenged. Adding always-Burnthrough to non-ECM tanks will counter BlueEcm's jamming and improve kill accuracy.

## Code Change

`SwarmCoordinator.HandleEcm`: changed `else if (IsEnemyEcmActive(ctx)) mode = EcmMode.Burnthrough` to `else if (!config.HasEcm) mode = EcmMode.Burnthrough` (always Burnthrough for non-ECM tanks).

## Run Parameters

- Seed: 1000, Matches: 200, Parallel: 8, `--on-timeout energy`

## Raw Results

```
Red: 59 wins (30%) | Blue: 141 wins (70%)
RedArrow rate: 2.79 (normal)
RedBlade rate: 3.89 (up)
RedHammer rate: 3.79 (up)
DmgTaken increased for all Red tanks vs iter-1
```

## Analysis

**REFUTED. 50% → 30%. Catastrophic regression. Reverted.**

Burnthrough (0.3/tick per tank) drains energy continuously from all 3 attackers. This combined drain (~0.9/tick team) compounds over long matches and leaves Red tanks weaker in final engagements. Additionally, Burnthrough's ghost-filtering effect may be counteracting Ghost's own ghost contacts somehow, or the energy cost at 0.3/tick per tank over 2000+ tick matches is significant (3 × 600 = 1800 energy drained for 3 tanks at 2000 ticks).

The `IsEnemyEcmActive` reactive approach (only activate when EcmAlert received) was actually fine — reactive Burnthrough is cheaper and more targeted.

## Key Findings

1. Always-on Burnthrough is too energy-expensive for 2000+ tick matches
2. The reactive approach (IsEnemyEcmActive) is correct — don't burn energy countering ECM that may not be active
3. ECM drain compounds with match length — avoid always-on unless the gain is clearly >0.3/tick/tank

## Summary

**Red 30%** — massive regression. Reverted to iter-1 HandleEcm logic.
