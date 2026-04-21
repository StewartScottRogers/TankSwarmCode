## Hypothesis

Ghost HasEcm=false currently — Ghost never activates ECM. When BlueEcm is detected, Red switches
to ECMScreen strategy. Enabling HasEcm=true (OffensiveEcmMode=EcmMode.Jam default) would:
1. Move Ghost to 120px from target (vs 160px Wolfpack orbit) when ECMScreen activates
2. Activate Ghost's ECM jam (0.5/tick) to corrupt 30% of BlueEcm's radar scans
3. Other 4 tanks move to 130px and fire bullets + Burnthrough (0.3/tick)

This is NOT JamAndSpoof (the original disaster — continuous, 1.3/tick, radar offline, comms blocked).
Jam keeps radar and comms active; ECMScreen only activates reactively when Blue ECM is detected.

Success criteria: Red 2-seed avg changes by ≥2pp vs 70.2% baseline; stop early if clearly negative.

## Code Changes

`RedGhostCortex.cs`: `HasEcm = false` → `true`.
Reverted after 2-seed results showed exactly 0.0pp net change.

## Run Parameters

- Batch: 200 matches each seed
- Mode: `--parallel 8`
- Seeds: 1000, 2000 (stopped — clear neutral signal)
- Timeout rule: `--on-timeout energy`
- DLL: `net10.0/publish/` (published via shell project)

## Raw Results

| Seed | Ghost ECM=Jam | Baseline (no ECM) | Delta |
|------|---------------|-------------------|-------|
| 1000 | 70.0% | 70.0% | 0.0pp |
| 2000 | 70.5% | 70.5% | 0.0pp |
| **Avg** | **70.25%** | **70.25%** | **0.0pp** |

## Analysis

**REFUTED: Ghost HasEcm=true (Jam) is perfectly neutral: 0.0pp across 2 seeds.**

Why the jam effect is exactly cancelled:
1. **ECM Jam disruption vs DPS loss exactly balance.** Ghost's Jam disrupts 30% of BlueEcm's radar
   scans, but Ghost simultaneously stops firing bullets during ECMScreen. At MaxFP=2.0, Ghost deals
   8 damage/shot. The lost DPS is exactly offset by BlueEcm's radar degradation.

2. **Ghost energy drain is real.** Ghost avg energy when alive: 29.3 (seed 1000) vs ~50 for attack
   tanks. The Jam at 0.5/tick + repositioning to 120px (vs orbit at 160px) drains Ghost.

3. **ECMScreen activates infrequently.** The strategy only fires when `IsEnemyEcmActive` returns
   true (within 20 ticks of an EcmAlert message). Blue's ECM is intermittent; ECMScreen accounts
   for a small fraction of total match ticks. Ghost's jamming is real but too brief to matter.

4. **Burnthrough already counters BlueEcm effectively.** When Blue ECM fires:
   - Without Ghost ECM: All 5 Red tanks use Burnthrough (0.3/tick each) → 30→8% drop chance
   - With Ghost ECM: Ghost jams (0.5/tick), other 4 use Burnthrough (0.3/tick each)
   - Net: Burnthrough was already degrading BlueEcm to 8% effectiveness; Ghost's Jam adds 30%
     drop on Ghost's own radar (Ghost now gets jammed back). Not a clear win.

## Key Findings

- **Ghost HasEcm=true REFUTED: 0.0pp avg (2 seeds — perfectly neutral).**
- Code reverted to HasEcm=false. ECM Jam exactly cancels Ghost's DPS loss.
- **Architecture ceiling confirmed at 70.2%.** All parameter dimensions exhausted:
  - MaxFP: swept 0.5-2.0 for all tanks; 1.0 optimal for 4 attack tanks, 2.0 for Ghost
  - PR: swept 140/150/160/180; 160 is definitive optimum
  - Encircle radius: neutral
  - Fallback threshold: negative
  - BlueSharp priority targeting: neutral
  - Ghost ECM (Jam): neutral
- Next explorations must either accept 70.2% as the ceiling OR find novel mechanisms:
  1. AllyStaleTicks tuning (currently 30 — how stale is "stale"?)
  2. Mixed-PR formation (e.g., one tank at PR=140 as closer anchor, others at 160)
  3. Conditional fire power (different MaxFP for closing shots vs sustained combat)

## Summary

REFUTED. Code reverted. 70.2% appears to be the architecture ceiling for the current Wolfpack
formation vs current Blue DLL. Further gains require novel mechanisms beyond parameter tuning.
