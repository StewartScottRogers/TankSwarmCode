---
Iteration: 13
Date: 2026-04-20
Branch: research/iter-000082-blueecm-pr171-seed3000
---

# Iter-13: Post-12 Stacking Explorations (ALL REFUTED)

## Overview

After confirming Arrow PR=180 (+4pp), tested stacking additional changes on top. All failed. Tested at seed 1000 vs iter-12 baseline (48%).

## Changes Tested

### Hammer PR 200 → 160 (on top of Arrow PR=180)
**Result: 42% (-6pp from iter-12)**

Hammer at same range as Blade (160) created crowding. Both tanks competing for same position. Faster first kills for both sides — Red lost the coin flip more often. DPS interference worse than gain.

### Fallback Threshold 30 → 10 (on top of Arrow PR=180)
**Result: 46% (-2pp from iter-12)**

With Fallback threshold reduced, Red stays aggressive longer at low energy. First kill rate improved (64% Red gets first) but close-out rate worsened (Blue wins 37% after Red kills first, vs 33%). Net negative.

### Blade MaxFP 2.5 → 3.0 (on top of Arrow PR=180)
**Result: 18% (-30pp CATASTROPHIC)**

Both Blade (3.0) and Hammer (3.0) at same MaxFP creates bullet-speed desynergy (both at speed=11). Combined rate dropped drastically (Hammer 4.01→2.84, Blade 3.55→2.73). One attacker at MaxFP=3.0 (Hammer) is fine; two at 3.0 causes DPS collapse. Never have two close-range attackers at MaxFP=3.0.

### Arrow PR 180 → 200 (retest)
**Result: 46% (-2pp from iter-12)**

Arrow at same range as Hammer (200) reduces pack benefit. Arrow survivals dropped slightly vs PR=180. PR=180 remains optimal.

### Blade PR 160 → 140 (on top of Arrow PR=180)
**Result: 44% (-4pp from iter-12)**

Blade at 140 is very close to BlueEcm (MaxFP=5.0, PR=150). Heavy fire from BlueEcm at close range kills Blade and Arrow faster. Arrow first-kills increased to 28 (from 16 at iter-12).

### Ghost PR 150 → 130 (on top of Arrow PR=180)
**Result: 46% (-2pp from iter-12)**

Ghost slightly closer to target. Timeout rate increased; Red wins fewer timeouts. Not a clear improvement over 150.

### AllyPingInterval 15 → 10 (on top of Arrow PR=180)
**Result: 38% (-10pp CATASTROPHIC)**

Same pattern as VolleyIntervalTicks=20 regression: frequency constant change causes large unexpected regression with no logical explanation from code analysis. DO NOT CHANGE FREQUENCY CONSTANTS.

## Key Findings

1. Iter-12 (Arrow PR=180) appears to be a local optimum for range configuration
2. Two tanks at MaxFP=3.0 causes catastrophic DPS desynergy
3. One tank per "damage tier" (Ghost=0.1, Arrow=1.5, Blade=2.5, Hammer=3.0) is correctly balanced
4. Frequency constants (AllyPingInterval, VolleyIntervalTicks) are brittle — never tune them
5. PR gradient (150/160/180/200) works because each tank occupies a distinct ring — no competition

## Summary

All post-12 stacking attempts refuted. Arrow PR=180 is the current ceiling via range tuning.
