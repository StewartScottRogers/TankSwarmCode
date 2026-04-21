# Chapter 15: Headless CLI Runner

[← Tank Energy](ch14-tank-energy.md) | [Table of Contents](TOC.md)

---

`TankSwarmCode.Cli` is a headless match runner designed for AI training, benchmarking, and automated testing. It loads swarm DLLs at runtime, runs matches without a GUI, and streams results as JSON, NDJSON, table, or CSV.

---

## Building and Running

```bash
# From the repo root — Debug
dotnet run --project TankSwarmCode.Cli/TankSwarmCode.Cli.csproj \
  --bot1 path/to/Red.dll --bot2 path/to/Blue.dll

# Release publish (single-file exe)
dotnet publish TankSwarmCode.Cli/TankSwarmCode.Cli.csproj -c Release
```

---

## Command Reference

### Match options

| Flag | Default | Description |
|------|---------|-------------|
| `--bot1 <dll>` | required | Path to the Swarm 1 DLL |
| `--bot2 <dll>` | required | Path to the Swarm 2 DLL |
| `--seed <int>` | random | RNG seed; incremented by 1 for each match in a batch |
| `--max-ticks <int>` | 5000 | Ticks before a match is declared timed-out |
| `--batch <N>` | 1 | Run N matches sequentially or in parallel |
| `--parallel <N>` | 1 | Max concurrent matches (writes are lock-serialised) |
| `--on-timeout <rule>` | `draw` | Tie-break policy when `--max-ticks` is reached |

#### `--on-timeout` values

| Value | Behaviour |
|-------|-----------|
| `draw` | No winner declared (default) |
| `energy` | Winner = swarm with the most total energy among living tanks |
| `survivors` | Winner = swarm with more living tanks; equal counts remain a draw |

---

### Arena options

| Flag | Default | Description |
|------|---------|-------------|
| `--width <double>` | 800 | Arena width in pixels |
| `--height <double>` | 600 | Arena height in pixels |

---

### Output options

| Flag | Default | Description |
|------|---------|-------------|
| `--format <fmt>` | `json` | Output format: `json`, `table`, or `csv` |
| `--summary` | off | Append aggregate stats after a batch (JSON mode only; table mode always shows it) |

### Black Box telemetry options

| Flag | Default | Description |
|------|---------|-------------|
| `--blackbox-dir <dir>` | off | Write tick-by-tick telemetry to a directory (single match only) |
| `--blackbox-tank <name>` | all tanks | Record only this tank; repeat the flag for multiple tanks |
| `--blackbox-events-only` | off | Skip ticks where nothing happened (~85% size reduction) |

`--blackbox-dir` is incompatible with `--batch N` when N > 1.

---

### Discovery

```bash
TankSwarmCode.Cli --list path/to/MySwarm.dll
```

Loads the DLL and lists every class implementing `ISwarmTank`, flagging any that lack a parameterless constructor and cannot be instantiated.

---

## Output Formats

### JSON (default)

A single match outputs one JSON object. A batch (`--batch N`) outputs **NDJSON** — one JSON line per match, suitable for streaming to `jq` or a training pipeline.

**Single-match fields:**

```json
{
  "winner_swarm_id": 1,
  "ticks": 1342,
  "timed_out": false,
  "swarm1_survivors": 3,
  "swarm2_survivors": 0,
  "swarm1_energy": 187.4,
  "swarm2_energy": 0.0,
  "swarm1_survivor_names": ["RedHammer", "RedArrow", "RedGhost"],
  "swarm2_survivor_names": [],
  "first_kill_tick": 312,
  "tanks": [
    {
      "name": "RedHammer",
      "swarm_id": 1,
      "role": "Attacker",
      "survived": true,
      "energy": 74.3,
      "destroyed_at_tick": 0,
      "damage_taken": 52.1,
      "energy_gained": 26.4
    },
    ...
  ]
}
```

**Batch NDJSON** wraps each result with a `match` index and `seed`:

```json
{"match":1,"winner_swarm_id":1,"ticks":1342,...,"seed":42}
{"match":2,"winner_swarm_id":2,"ticks":887,...,"seed":43}
```

`swarm1_survivor_names` and `swarm2_survivor_names` use the tank's `Name` property (e.g. `"RedHammer"`, `"BlueEcm"`) as set in each tank class.

---

### Table (`--format table`)

Human-readable output for a single match:

```
Winner: Red (swarm 1)  |  Ticks: 1342
Energy left: Red 187.4E  |  Blue 0.0E

  Tank                 Sw  Role            Surv    Energy   DmgTaken   EGained   Died@
  -------------------- --  -------------- ----  -------  ---------  --------  ------
  RedHammer             1  Attacker        Yes     74.3       52.1      26.4       -
  ...

  Death order:
     1. BlueStrike            (swarm 2) @ tick 312
     ...
     -  RedHammer             (swarm 1) survived  74.3E
```

For a batch, the table lists every match followed by a summary section.

---

### Summary (table mode always; JSON mode via `--summary`)

The summary shows:

- **Win counts and percentages** per swarm plus draws
- **Win quality** — average energy margin for decisive wins and timeout wins, with TO%
- **Decisive median ticks** with per-swarm breakdown and tempo note (e.g. "Red 1.4x faster")
- **Average first kill tick**, split by which swarm took the first casualty
- **Speed distribution** — matches bucketed into ≤200t / 201–1000t / 1001+t / timeout, with winner breakdown
- **Per-tank survival table** — survival rate, win-survival rate, avg ticks alive, damage taken, energy gained, and combat rate per 100 ticks
- **Insights** — automatic role labels derived from the stats (see below)
- **Win combo frequencies** — which survivor lineups repeat across matches

#### Insight labels

| Label | Criteria |
|-------|----------|
| MVP / Co-MVP | Survived in the most (or near-most) decisive wins |
| Hider | Lowest energy-gain rate; non-ECM tank with rate < 3.0/100t |
| Expendable | Present in wins but survives < 33% of them; not the top attacker |
| Top attacker | Highest energy-gain rate among non-ECM tanks |
| Glass cannon | Top attacker whose win-survival rate is still below 33% |
| Linchpin | Swarm win rate drops > 55 pp when this tank is dead |
| All-in | Survives only in wins (never survives a loss), ≥ 5 survivals |
| Survivor | Survives ≥ 5 defeats |
| Fragile | Survival rate < 10% across ≥ 20 matches |
| Primary target | First-killed > 1.5× the swarm average, ≥ 5 incidents |
| Solo carry | Single-handedly wins ≥ 20% of swarm wins, ≥ 3 solo matches |
| ECM | Tank whose `Role` is `EcmSpecialist` |

---

### CSV (`--format csv`)

Emits a header row followed by one data row per match:

```
match,winner_swarm_id,ticks,timed_out,swarm1_survivors,swarm2_survivors,swarm1_energy,swarm2_energy,first_kill_tick,seed
1,1,1342,False,3,0,187.4,0,312,42
2,2,887,False,0,2,0,94.1,201,43
```

---

## Black Box Telemetry

`--blackbox-dir` records every tank's complete state at every tick and writes the data into a directory of NDJSON files after the match. This is the primary data source for per-tank and per-swarm AI analysis.

### Output directory structure

```
<dir>/
  header.json          Match metadata (seed, winner, total ticks, arena size)
  BlueEcm.ndjson       One file per tank — all records for that tank
  BlueGuard.ndjson
  ...
  RedGhost.ndjson
  ...
  swarm1.ndjson        All swarm 1 records merged (written only when no --blackbox-tank filter)
  swarm2.ndjson        All swarm 2 records merged
```

### header.json

```json
{
  "match_id": "3f2a...",
  "seed": 3000,
  "total_ticks": 2347,
  "winner_swarm_id": 1,
  "arena_width": 800,
  "arena_height": 600
}
```

### Per-tank NDJSON files

Each line is one `TankTickRecord` — a complete snapshot for that tank at that tick:

```json
{"tick":42,"tank_name":"BlueEcm","swarm_id":1,"x":312.4,"y":198.1,"heading":47.2,"velocity":4.0,"gun_heading":52.1,"radar_heading":18.7,"energy":84.3,"is_alive":true,"active_ecm":"Off","cmd_move":50.0,"cmd_body_turn":0.0,"cmd_gun_turn":3.5,"cmd_radar_turn":45.0,"cmd_fire_power":1.5,"cmd_ecm":"Off","radar_contacts":[...],"events":[{"type":"ScannedTank","other_name":"RedGhost","value":127.3}]}
```

**State fields** — position, heading, velocity, gun/radar headings, energy, alive status, active ECM mode.

**Command fields** (`cmd_*`) — what the AI *requested* this tick, captured before the engine applied it. Useful for training: the command is the AI's decision; the subsequent state change is the outcome.

**`radar_contacts`** — the full radar picture this tank held at the end of the tick. Only contains what *this tank's* sensors observed (own scans merged with ally broadcasts). Does not include the enemy's private sensor data.

**`events`** — things that happened to this tank this tick:

| `type` | `other_name` | `value` |
|--------|-------------|---------|
| `FiredBullet` | — | bullet power |
| `HitByBullet` | shooter name | damage received |
| `BulletHit` | victim name | damage dealt |
| `HitTank` | other tank name | — |
| `HitWall` | — | — |
| `ScannedTank` | scanned tank name | distance (px) |
| `Painted` | painter name | — |
| `Died` | — | — |

### Color separation

Each per-tank file contains only what that tank knew — its own state, its own commands, and what its sensors observed. Filtering by swarm gives a complete picture of one color without leaking the other color's internal decisions. This is the intended use for training each swarm's AI independently:

```bash
# Blue perspective only
--blackbox-dir ./match_3000/ --blackbox-events-only
# → reads swarm1.ndjson  or individual Blue*.ndjson files

# Red perspective only
# → reads swarm2.ndjson  or individual Red*.ndjson files
```

### `--blackbox-events-only`

Drops all records where `events` is empty. For a typical 5000-tick, 29-tank match this reduces output from ~145,000 lines to ~15,000–25,000 lines. A single tank's file is usually 200–800 lines — small enough to fit comfortably in an LLM context window.

### Example workflows

**Analyze one tank with an LLM agent:**

```bash
TankSwarmCode.Cli \
  --bot1 Blue.dll --bot2 Red.dll --seed 3000 \
  --blackbox-dir ./match_3000/ \
  --blackbox-tank BlueEcm \
  --blackbox-events-only
# BlueEcm.ndjson + header.json written; all other files skipped
```

**Full Blue swarm analysis:**

```bash
TankSwarmCode.Cli \
  --bot1 Blue.dll --bot2 Red.dll --seed 3000 \
  --blackbox-dir ./match_3000/ \
  --blackbox-events-only
# All Blue*.ndjson + swarm1.ndjson written (Red files written too; ignore them)
```

**Two specific tanks for cross-color comparison:**

```bash
TankSwarmCode.Cli \
  --bot1 Blue.dll --bot2 Red.dll --seed 3000 \
  --blackbox-dir ./match_3000/ \
  --blackbox-tank BlueEcm \
  --blackbox-tank RedGhost \
  --blackbox-events-only
# BlueEcm.ndjson + RedGhost.ndjson + header.json only
```

See [Chapter 8: Data Models Reference](ch08-data-models.md#telemetry-models) for the full type definitions.

---

## Tank Loading

The CLI discovers tanks the same way as the GUI: it loads the DLL with `Assembly.LoadFrom`, finds all non-abstract classes that implement `ISwarmTank` **and** have a parameterless constructor, then instantiates and assigns a `SwarmId`.

Tanks without a parameterless constructor are silently skipped during a match run. The `--list` command flags them explicitly so you can detect the problem early.

---

## Parallelism

With `--parallel N`, up to N matches run concurrently using `Parallel.For`. Each match gets an independent `ArenaEngine` instance — no shared mutable state. Console writes are lock-serialised so NDJSON lines are never interleaved.

Seeds are assigned deterministically: `--seed S` gives match `i` (0-indexed) the seed `S + i`, so a batch is fully reproducible.

---

## Example Workflows

### Quick head-to-head

```bash
TankSwarmCode.Cli --bot1 Red.dll --bot2 Blue.dll --format table
```

### 100-match benchmark with summary

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 100 --parallel 8 \
  --seed 1000 --on-timeout energy \
  --format table
```

### Feed results into a Python training loop

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 500 --parallel 16 --seed 0 \
  | python train.py --stdin
```

### Export to CSV for spreadsheet analysis

```bash
TankSwarmCode.Cli \
  --bot1 Red.dll --bot2 Blue.dll \
  --batch 200 --seed 42 \
  --format csv > results.csv
```

### Capture tick-by-tick telemetry for AI analysis

```bash
TankSwarmCode.Cli \
  --bot1 Blue.dll --bot2 Red.dll --seed 3000 \
  --blackbox-dir ./match_3000/ --blackbox-events-only
```

### Discover what tanks are in a DLL

```bash
TankSwarmCode.Cli --list MySwarm.dll
```

---

[← Tank Energy](ch14-tank-energy.md) | [Table of Contents](TOC.md)
