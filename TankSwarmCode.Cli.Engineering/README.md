# TankSwarmCode CLI Engineering

This project is the workspace for running headless batch matches and conducting hypothesis-driven balance research using `TankSwarmCode.Cli`.

---

## Quick Start

**Prerequisites**: .NET SDK 10.0+, published DLLs for both swarms (see Building below)

```bash
# Single match, table output
TankSwarmCode.Cli --bot1 Red.dll --bot2 Blue.dll --format table

# 100-match benchmark with summary
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 100 --parallel 8 --seed 1000 \
  --on-timeout energy --format table
```

### Building the CLI and swarm DLLs

```bash
# Publish swarm DLLs
dotnet publish TankSwarmCode.SwarmTanks.Red/TankSwarmCode.SwarmTanks.Red.csproj -c Release
dotnet publish TankSwarmCode.SwarmTanks.Blue/TankSwarmCode.SwarmTanks.Blue.csproj -c Release

# Run via dotnet (no publish needed)
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -- \
  --bot1 TankSwarmCode.SwarmTanks.Red/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Red.dll \
  --bot2 TankSwarmCode.SwarmTanks.Blue/bin/Release/net10.0/TankSwarmCode.SwarmTanks.Blue.dll \
  --batch 100 --format table

# Publish CLI as single-file exe
dotnet publish TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -c Release
```

---

## Command Reference

### Match options

| Flag | Default | Description |
|------|---------|-------------|
| `--bot1 <dll>` | required | Path to Swarm 1 DLL |
| `--bot2 <dll>` | required | Path to Swarm 2 DLL |
| `--seed <int>` | random | RNG seed; incremented by 1 per match in a batch |
| `--max-ticks <int>` | 5000 | Ticks before a match is declared timed-out |
| `--batch <N>` | 1 | Run N matches sequentially or in parallel |
| `--parallel <N>` | 1 | Max concurrent matches (writes are lock-serialised) |
| `--on-timeout <rule>` | `draw` | Tie-break policy when `--max-ticks` is reached |

#### `--on-timeout` values

| Value | Behaviour |
|-------|-----------|
| `draw` | No winner declared (default) |
| `energy` | Winner = swarm with most total energy among living tanks |
| `survivors` | Winner = swarm with more living tanks; equal counts remain a draw |

### Arena options

| Flag | Default | Description |
|------|---------|-------------|
| `--width <double>` | 800 | Arena width in pixels |
| `--height <double>` | 600 | Arena height in pixels |

### Output options

| Flag | Default | Description |
|------|---------|-------------|
| `--format <fmt>` | `json` | Output format: `json`, `table`, or `csv` |
| `--summary` | off | Append aggregate stats after a batch (JSON mode only; table mode always shows it) |

### Discovery

```bash
TankSwarmCode.Cli --list MySwarm.dll
```

Lists every `ISwarmTank` class in the DLL, flagging any that lack a parameterless constructor.

---

## Output Formats

### Table (`--format table`)

Human-readable. For a batch, each match is listed followed by a summary section. Use for interactive research sessions.

### JSON / NDJSON (default)

Single match → one JSON object. Batch → NDJSON (one JSON line per match), suitable for piping to `jq` or a training script.

### CSV (`--format csv`)

Header row + one data row per match. Export to `results.csv` for spreadsheet analysis.

---

## Summary Statistics

The summary (always shown in table mode; use `--summary` for JSON) includes:

- **Win tally** — win counts and percentages per swarm plus draws
- **Win quality** — average energy margin; `(N% via TO)` flags timeout-heavy wins
- **Decisive median ticks** — per-swarm tempo with a comparison note (e.g. "Red 1.4x faster")
- **First kill tick** — split by which swarm took the first casualty
- **Speed distribution** — matches bucketed by duration (≤200t / 201–1000t / 1001+t / timeout)
- **Per-tank survival table** — survival rate, win-survival rate, avg ticks alive, damage taken, energy gained, combat rate per 100t
- **Insights** — automatic role labels (see below)
- **Win combo frequencies** — survivor lineups that repeat across matches

### Insight labels

| Label | Criteria |
|-------|----------|
| ECM | Tank whose `Role` is `EcmSpecialist` |
| MVP / Co-MVP | Survived in the most (or near-most) decisive wins |
| Hider | Non-ECM tank with combat rate < 3.0/100t |
| Expendable | Present in wins but survives < 33% of them; not the top attacker |
| Top attacker | Highest energy-gain rate among non-ECM tanks |
| Glass cannon | Top attacker with win-survival rate still below 33% |
| Linchpin | Swarm win rate drops > 55 pp when this tank is dead |
| All-in | Survives only in wins (never survives a loss), ≥ 5 survivals |
| Survivor | Survives ≥ 5 defeats |
| Fragile | Survival rate < 10% across ≥ 20 matches |
| Primary target | First-killed > 1.5× the swarm average, ≥ 5 incidents |
| Solo carry | Single-handedly wins ≥ 20% of swarm wins, ≥ 3 solo matches |

---

## Example Workflows

### Quick head-to-head

```bash
TankSwarmCode.Cli --bot1 Red.dll --bot2 Blue.dll --format table
```

### Balance research benchmark

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 200 --parallel 8 \
  --seed 1000 --on-timeout energy \
  --format table
```

### Cramped arena (ECM stress test)

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 200 --parallel 8 \
  --seed 1000 --on-timeout energy \
  --width 350 --height 250 \
  --format table
```

### Feed results into a Python training loop

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 500 --parallel 16 --seed 0 \
  | python train.py --stdin
```

### Export to CSV

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 200 --seed 42 \
  --format csv > results.csv
```

---

## Research Workflow

See [`AutoResearch.md`](AutoResearch.md) for the full Karpathy Loop — a hypothesis-driven cycle for balance investigation:

```
Hypothesize → CLI run → Analyze → Update hypothesis → repeat
```

**Research parameter defaults:**

| Parameter | Default | Reason |
|-----------|---------|--------|
| `--batch` | 100–200 | ≥100 for stable percentages |
| `--parallel` | 8 | Matches available CPU cores |
| `--seed` | 1000 | Fixed for reproducibility |
| `--on-timeout` | `energy` | Eliminates draws; reveals true balance |
| `--format` | `table` | Summary stats auto-included |

**Arena size effects:**
- Default (800×600): ECM tanks can hide; 25–30% draws without `energy` policy
- Cramped (350×250): buffs close-quarters combat, nerfs ECM hiding strategies

---

## Further Reading

| Chapter | Topic |
|---------|-------|
| [`Documentation/ch15-cli.md`](../Documentation/ch15-cli.md) | Full CLI reference |
| [`Documentation/ch01-overview.md`](../Documentation/ch01-overview.md) | Project overview and core concepts |
| [`Documentation/ch03-architecture.md`](../Documentation/ch03-architecture.md) | Dependency graph and runtime data flow |
| [`Documentation/ch09-builtin-tanks.md`](../Documentation/ch09-builtin-tanks.md) | Blue and Red swarm strategy details |
| [`Documentation/ch13-ecm-system.md`](../Documentation/ch13-ecm-system.md) | ECM modes, costs, and countermeasures |
| [`TankSwarmCode.SwarmTanks.Blue.Engineering/README.md`](../TankSwarmCode.SwarmTanks.Blue.Engineering/README.md) | Blue Swarm AI design reference |
| [`TankSwarmCode.SwarmTanks.Red.Engineering/README.md`](../TankSwarmCode.SwarmTanks.Red.Engineering/README.md) | Red Swarm quick-start and API reference |
