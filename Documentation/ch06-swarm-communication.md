# Chapter 6: Swarm Communication

[← Tank AI Framework](ch05-tank-ai-framework.md) | [Table of Contents](TOC.md) | [Next: Radar System →](ch07-radar-system.md)

---

## Overview

Tanks that share a `SwarmId` can exchange structured messages. The engine delivers these broadcasts at the end of each tick, so every living ally sees the message before their next `OnTick` call. This makes swarm coordination deterministic — no message can be "missed" due to timing.

**Radio and jamming**: When a tank is running `EcmMode.Jam` or `EcmMode.JamAndSpoof`, **all radio is blocked in both directions**. The jammer cannot send messages (they are silently dropped before delivery), and no messages from allies reach the jammer. This includes `RadarShare`, `[PAINTED]`, and all other types.

---

## Automatic RadarShare

The most important communication channel requires **no code from the AI author**. Every time a tank's radar detects an enemy, the engine automatically:

1. Creates a `RadarShare` `SwarmMessage` containing the full `RadarContact`.
2. Delivers it to every living ally (who is not currently jamming).
3. Merges the contact into each ally's `RadarMap` (updating the entry if the ally's existing entry is older).

This means **all swarm members share a unified radar picture** without any manual broadcasting. A swarm with a dedicated scout tank effectively gives every member real-time enemy awareness.

---

## Automatic `[PAINTED]` Broadcast

The second automatic channel is triggered when a tank is swept by an enemy radar beam. The engine fires `OnPainted(PaintedEventArgs e)` on the scanned tank. The base class implementation immediately broadcasts a `Painted` message to all allies:

```csharp
Broadcast(new SwarmMessage
{
    SenderName = Name,
    Type       = SwarmMessageType.Painted,
    TargetName = e.PainterName,      // enemy scanner's name
    Position   = e.PainterPosition,  // enemy scanner's position
    Timestamp  = Arena.TickNumber
});
```

This gives the **entire swarm the enemy scanner's position** at the moment the scan occurs — no code required. The tactical implication is significant: a tank that aggressively scans with its radar reveals its own location to the target's entire swarm.

Override `OnPainted` to add your own reaction (evasive manoeuvre, return fire, etc.), or to suppress the auto-broadcast and send a custom payload instead. Call `base.OnPainted(e)` to keep the automatic broadcast.

---

## Manual Broadcasting

To send additional messages, call `Broadcast()` from within any lifecycle method:

```csharp
Broadcast(new SwarmMessage
{
    SenderName  = State.Name,
    Type        = SwarmMessageType.TargetLocked,
    TargetName  = "RedAlpha",
    Position    = enemyPosition,
    Timestamp   = Arena.CurrentTick
});
```

The engine queues the message and delivers it to all living, non-jamming allies at the end of the current tick.

---

## SwarmMessageType Enum

| Value | Typical Use |
|-------|-------------|
| `RadarShare` | Auto-sent by the engine; contains `RadarContact` |
| `Painted` | Auto-sent by the base class when an enemy radar sweeps this tank; `Position` = painter's position, `TargetName` = painter's name |
| `EnemySpotted` | Manual broadcast to share a sighting without a full radar share |
| `TargetLocked` | Commander designates the priority target for the swarm |
| `RequestBackup` | Tank signals it needs help (e.g., low energy) |
| `FormationMove` | Commander orders a positional manoeuvre |
| `FallBack` | Retreat order |
| `RoleChange` | Dynamic role reassignment (e.g., Scout promoted to Attacker) |
| `EcmAlert` | Sender has detected enemy jamming or ghost contacts; allies should activate `Burnthrough`. `CustomData` carries a human-readable description. |
| `Custom` | Anything else; inspect `CustomData` for the payload |

---

## SwarmMessage Model

```csharp
public class SwarmMessage
{
    public string            SenderName   { get; init; }
    public SwarmMessageType  Type         { get; init; }
    public long              Timestamp    { get; init; }   // Arena.CurrentTick

    // Optional fields — set only those relevant to your message type
    public string?           TargetName   { get; init; }
    public Vector2D?         Position     { get; init; }
    public string?           CustomData   { get; init; }   // JSON or plain text
    public RadarContact?     RadarContact { get; init; }   // Populated by RadarShare
}
```

---

## Receiving Messages

Override `OnSwarmMessage` to react to incoming messages:

```csharp
protected override void OnSwarmMessage(SwarmMessageEventArgs e)
{
    var msg = e.Message;

    switch (msg.Type)
    {
        case SwarmMessageType.TargetLocked:
            _priorityTargetName = msg.TargetName;
            break;

        case SwarmMessageType.RequestBackup:
            // Move toward the sender
            _rallySenderPosition = msg.Position;
            break;

        case SwarmMessageType.FormationMove:
            _assignedPosition = msg.Position;
            break;

        case SwarmMessageType.Painted:
            // An ally was painted — msg.Position is the enemy scanner's location
            _knownEnemyPosition = msg.Position;
            break;
    }
}
```

`RadarShare` messages are processed by the base class before `OnSwarmMessage` is called, so by the time your override runs, `RadarMap` is already up to date.

---

## Message Delivery Timing

```
Tick N:
  ├─ OnTick() (all tanks, parallel)
  │     └─ Broadcast() calls queue messages
  ├─ ... physics phases ...
  ├─ ProcessRadarScans()
  │     └─ OnScannedTank() → RadarShare messages queued
  │     └─ OnPainted()     → [PAINTED] messages queued
  ├─ DeliverSwarmMessages() — messages from non-jamming senders
  │                          — delivered only to non-jamming allies
  │                          — OnSwarmMessage fired for each message received
  └─ UpdateTankStates()

Tick N+1:
  └─ OnTick() — allies see updated RadarMap and can read _priorityTargetName, etc.
```

There is no per-tick message limit, but messages are only delivered to tanks that are **alive and not jamming** at delivery time.

---

## Coordination Patterns Used by Built-in Swarms

### Commander pattern (Blue Swarm)

`BlueCommander` scans and broadcasts `TargetLocked` with the lowest-energy enemy name. `BluePatrol`, `BlueSniper`, and `BlueWarden` each listen for `TargetLocked` and update their internal `_priorityTargetName`. This gives the entire swarm a shared priority target with zero radar duplication overhead.

### Scout broadcast pattern (Red Swarm)

`RedScout` spins its radar at full speed and relies on the automatic `RadarShare` to flood `RedAlpha`, `RedBravo`, `RedWolf`, and `RedFox` with fresh radar contacts each tick. The attacker tanks never need their own dedicated radar strategy.

### Backup request pattern (Red Swarm)

`RedAttacker` fires `RequestBackup` when its energy drops below 25. In a more complete implementation, allies could converge on the requester's position to protect a low-health teammate.

### Painted counter-targeting

Any tank that receives a `Painted` message from an ally knows an enemy scanner's exact position that tick. An attacker can use `msg.Position` directly for a firing solution — even before its own radar has locked onto that enemy. This is particularly valuable when the ally being painted is a defender or support tank that does not fire, turning it into a passive targeting sensor for the swarm.

---

[← Tank AI Framework](ch05-tank-ai-framework.md) | [Table of Contents](TOC.md) | [Next: Radar System →](ch07-radar-system.md)
