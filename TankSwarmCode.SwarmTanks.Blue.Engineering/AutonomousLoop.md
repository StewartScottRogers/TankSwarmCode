# Autonomous Karpathy Loop — Blue Engineering Research Iteration

**Run autonomously. Do not ask questions. Do not wait for input. Make all decisions yourself.**

## Context

This is one iteration of the Karpathy Loop for Blue Engineering research. Each iteration follows the cycle: Hypothesize → Run CLI → Analyze → Record → Next.

You are running from the **repo root** (the parent of `TankSwarmCode.SwarmTanks.Blue.Engineering`).

Read `TankSwarmCode.SwarmTanks.Blue.Engineering/AutoResearch.md` for the research framework, thresholds, baselines, and CLI parameter reference.

Read `AutonomousLoopState.md` for the current iteration count and next hypothesis.

## Focus

Blue Engineering owns the Blue swarm. Research goals, in priority order:

1. **Validate balance holds** — confirm the 51%/49% fix (BlueEcm MaxFirePower=1.0, iter-11) generalises across seeds.
2. **Improve Blue carry capacity** — BlueEcm (solo carry 34%) mirrors Ghost's role; find levers that increase BlueEcm's WinSurv or Blue's overall win rate without overcorrecting.
3. **Protect BlueEcm's hider role** — BlueEcm (MaxFirePower=1.0, RetreatEnergyThreshold=35) is now Blue's structural carry. Do NOT buff its firepower above 1.5 (iter-6: backfired to 62% Red) or reduce below 0.1 (iter-10: overcorrected to 59% Blue).

## Blue Swarm Profile (from research log)

| Tank | Role | Key stat | Notes |
|------|------|----------|-------|
| BlueEcm | ECM, MVP, Hider | WinSurv ~62% in Blue wins, solo carry 34% | MaxFirePower=1.0 — hider/carry since iter-11 fix |
| BlueRush | Combat | Drives fast decisive wins | Blue decisive median 150–220t; Rush is first-blood driver |
| BlueStrike | Combat | Rate/100t ~23 (high) | Combat tank; high engagement rate |
| BlueSharp | Support | Long-range fire support | 300px range, coordinates with volley fire |
| BlueGuard | Defender | Defensive role | Lower firepower; contributes to survival |

## Suggested Next Hypotheses (pick the first untested one)

1. **Cross-seed validation:** The 51%/49% balance (seed 1000, 200 matches) holds at seed 2000. Success: Blue 45–55% at seed 2000.
2. **BlueEcm MaxFP micro-tune:** Reducing BlueEcm MaxFirePower from 1.0→0.8 reduces energy drain further, raising BlueEcm WinSurv from ~62% to ≥65% and pushing Blue win rate to ≥51%. Success: BlueEcm WinSurv increases, Blue win rate ≥51%.
3. **BlueRush PreferredRange sweep:** Increasing BlueRush's `PreferredRange` from 180→220px allows earlier engagement, raising BlueRush first-kill count by ≥4 and reducing Blue decisive median by ≥20t. Success: Blue win rate stays ≥48%, decisive median drops.

---

## Iteration Protocol

### 1. Load prior state

Read `AutonomousLoopState.md` on `master`. Use the `nextHypothesis` field. If none, use hypothesis #1 from the list above.

### 2. Set hypothesis

State the concrete, falsifiable claim with measurable success criteria before touching any code.

### 3. Create iteration branch

```
git checkout master
git pull
git checkout -b research/iter-{NNNNNNN}-{slug}
```

All work from this point happens on this branch. Never commit directly to `master`.

### 4. Run baseline benchmark

Before changing any code, capture the current baseline:

```
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 200 --parallel 8 --seed 1000 --on-timeout energy --format table
```

If either DLL is missing, publish it first:

```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

### 5. Make code changes (if the hypothesis requires them)

If the hypothesis is purely observational (e.g. cross-seed validation), skip to step 6.

If the hypothesis requires a code change:
- Make the minimal change to the relevant `TankConfig` value in `TankSwarmCode.SwarmTanks.Blue/`.
- **Rebuild BOTH DLLs after any change.** Stale DLLs have contaminated runs before (iter-7, iter-9).
- Re-run the benchmark.

### 6. Analyze

Evaluate against the hypothesis using the thresholds in `TankSwarmCode.SwarmTanks.Blue.Engineering/AutoResearch.md`.

Verdict: **Confirmed** / **Refuted** / **Inconclusive**.

Also check:
- Did BlueEcm's WinSurv or solo carry drop? If so, the change harmed Blue's structural carry — Refuted regardless of win rate.
- Did BlueEcm's MaxFirePower remain at 1.0 in the baseline run? Verify before comparing.

### 7. Update documentation

If code changed, update the minimum set of docs:

| Code area changed | Documentation to update |
|---|---|
| `TankConfig` values in any Blue tank | `Documentation/ch09-builtin-tanks.md` roster table + tank section; `TankSwarmCode.SwarmTanks.Blue.Engineering/README.md` |
| New tank class added or removed | `ch09`, `ch01`, `ch02`, Blue Engineering README |
| `SwarmBrainBase` strategy logic | `ch09-builtin-tanks.md` Epoch Strategies table; Blue Engineering README |

Do not update docs for code that did not change.

### 8. Commit code + docs

```
git add Research/iter-{NNNNNNN}-{slug}.md AutonomousLoopState.md
git commit -m "[iter-{NNNNNNN}] record findings — {verdict}"
```

### 9. Apply verdict

#### If Confirmed or Inconclusive

Leave the branch as-is. Human reviews and decides whether to merge to `master`.

#### If Refuted

Revert the code + docs commit:

```
git revert HEAD --no-edit
```

Then rebuild both DLLs to clear stale binaries:

```
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release
```

### 10. Record findings

Write a new research file `Research/iter-{NNNNNNNN}-{slug}.md` with sections: Hypothesis, Code Change, Run Parameters, Raw Results, Analysis, Key Findings, Summary.

Update `AutonomousLoopState.md` **on the iteration branch**:

- `iteration`: incremented count
- `branch`: branch name
- `hypothesis`: what was tested
- `keyMetrics`: Blue win %, BlueEcm WinSurv, BlueEcm solo carry, Blue decisive median
- `verdict`: Confirmed / Refuted / Inconclusive
- `notes`: surprises, BlueEcm impact, anything non-obvious
- `nextHypothesis`: next concrete, falsifiable Blue improvement to test

```
git add Research/iter-{NNNNNNN}-{slug}.md AutonomousLoopState.md
git commit -m "[iter-{NNNNNNN}] record findings — {verdict}"
```

### 11. Propagate state to master

Cherry-pick only the `AutonomousLoopState.md` + research file commit to `master`:

```
git checkout master
git cherry-pick <state-commit-hash>
git checkout research/iter-{NNNNNNN}-{slug}
```

This is the **only** write the loop makes directly to `master`.

### 12. Stop

Output a one-paragraph summary: what was tested, what the result was, and what the next hypothesis is. Then exit.

---

## Rules

- Never ask the user a question. Never pause for confirmation.
- If you hit a build failure or missing file, fix it and continue.
- Each iteration must produce a concrete verdict.
- **Never commit directly to `master`.** All experimental work goes on the iteration branch.
- **Rebuild BOTH DLLs** after any code revert or change.
- **BlueEcm is sacrosanct.** Do NOT set MaxFP above 1.5 (combat-buffing kills BlueEcm) or below 0.1 (overcorrects to 59% Blue). If a change drops BlueEcm WinSurv below 50% or solo carry below 20%, treat it as Refuted.
- **Documentation and code are committed together.** Never commit a behaviour change without the corresponding doc update.
- **Refuted changes are always reverted** before the state commit.
