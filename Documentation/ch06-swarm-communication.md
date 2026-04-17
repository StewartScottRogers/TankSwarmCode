# Chapter 6: Swarm Communication

[← Tank AI Framework](ch05-tank-ai-framework.md) | [Table of Contents](TOC.md) | [Next: Radar System →](ch07-radar-system.md)

---

## Overview

Tanks that share a `SwarmId` can exchange structured messages. The engine delivers these broadcasts at the end of each tick, so every living ally sees the message before their next `OnTick` call. Swarm coordination is therefore deterministic — no message can be "missed" due to timing.

**Radio and jamming**: When a tank is running `EcmMode.Jam` or `EcmMode.JamAndSpoof`, **all radio is blocked in both directions**. The jammer cannot send messages (silently dropped before delivery), and no messages from allies can reach the jammer. This includes `RadarShare`, `Painted`, and all other message types.

---

## Automatic RadarShare

The most important communication channel requires **no code from the AI author**. Every time a tank's radar detects any tank, the base class `OnScannedTank` implementation automatically:

1. Records the contact in the scanner's `RadarMap`.
2. For enemy contacts (non-ally), creates a `RadarShare` `SwarmMessage` containing the full `RadarContact`.
3. Queues that message for delivery to every living, non-jamming ally.
4. Each ally's base `OnSwarmMessage` merges the contact into their own `RadarMap` (newer timestamp wins).

This means **all swarm members share a unified radar picture** without any manual broadcasting. A swarm with a dedicated scout tank effectively gives every member real-time enemy awareness.

---

## Automatic `Painted` Broadcast

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

Override `OnPainted` to add your own reaction (evasive manoeuvre, return fire, etc.) or to suppress the auto-broadcast and send a custom payload. Call `base.OnPainted(e)` to keep the automatic broadcast.

---

## Manual Broadcasting

To send additional messages, call `Broadcast()` from within any lifecycle method:

```csharp
Broadcast(new SwarmMessage
{
    SenderName  = Name,
    Type        = SwarmMessageType.TargetLocked,
    TargetName  = "RedAlpha",
    Position    = enemyPosition,
    Timestamp   = Arena.TickNumber
});
```

The engine queues the message and delivers it to all living, non-jamming allies at the end of the current tick.

---

## SwarmMessageType Enum

| Value | Typical Use |
|-------|-------------|
| `RadarShare` | Auto-sent by base class; contains `RadarContact` for a detected enemy |
| `Painted` | Auto-sent by base class when an enemy radar sweeps this tank; `Position` = painter's position, `TargetName` = painter's name |
| `EnemySpotted` | Manual broadcast to share a sighting without a full RadarShare |
| `TargetLocked` | Commander designates the priority target for the swarm |
| `RequestBackup` | Tank signals it needs help (e.g., low energy) |
| `FormationMove` | Commander orders a positional manoeuvre; `Position` is the rally point |
| `FallBack` | Retreat order |
| `RoleChange` | Dynamic role reassignment; `CustomData` carries the new role name |
| `EcmAlert` | Sender detected enemy jamming or ghost contacts; allies should activate `Burnthrough`; `CustomData` carries a description |
| `Custom` | Anything else; inspect `CustomData` for the payload |

---

## SwarmMessage Model

```csharp
public record SwarmMessage
{
    public string           SenderName   { get; init; }
    public SwarmMessageType Type         { get; init; }
    public long             Timestamp    { get; init; }   // Arena.TickNumber

    // Optional — set only those relevant to your message type
    public string?          TargetName   { get; init; }
    public Vector2D?        Position     { get; init; }
    public string?          CustomData   { get; init; }   // JSON or plain text
    public RadarContact?    RadarContact { get; init; }   // populated by RadarShare
}
```

---

## Receiving Messages

Override `OnSwarmMessage` to react to incoming messages. Always call `base.OnSwarmMessage(e)` first so `RadarShare` contacts are merged into `RadarMap` before your logic runs:

```csharp
public override void OnSwarmMessage(SwarmMessageEventArgs e)
{
    base.OnSwarmMessage(e);   // merge RadarShare contacts

    switch (e.Message.Type)
    {
        case SwarmMessageType.TargetLocked:
            _priorityTargetName = e.Message.TargetName;
            break;

        case SwarmMessageType.RequestBackup:
            _rallyPosition = e.Message.Position;
            break;

        case SwarmMessageType.FormationMove:
            _assignedPosition = e.Message.Position;
            break;

        case SwarmMessageType.Painted:
            // An ally was painted — e.Message.Position is the enemy scanner's location
            _knownEnemyPosition = e.Message.Position;
            break;

        case SwarmMessageType.EcmAlert:
            _burnthroughTicks = 40;   // run Burnthrough for 40 ticks
            break;
    }
}
```

---

## Message Delivery Timing

```
Tick N:
  ├─ OnTick() (all tanks, parallel)
  │     └─ Broadcast() calls queue messages
  ├─ ... physics phases ...
  ├─ ProcessRadarScans()
  │     └─ OnScannedTank() → RadarShare messages queued
  │     └─ OnPainted()     → Painted messages queued
  ├─ DeliverSwarmMessages()
  │     — from non-jamming senders only
  │     — delivered only to non-jamming, living allies
  │     — OnSwarmMessage fired for each received message
  └─ UpdateTankStates()

Tick N+1:
  └─ OnTick() — RadarMap is already updated; _priorityTargetName reflects last tick's orders
```

There is no per-tick message limit. Messages are only delivered to tanks that are **alive and not jamming** at delivery time.

---

## Coordination Patterns Used by Built-in Swarms

### Commander pattern (Blue Swarm)

`BlueCommander` scans `RadarMap` each tick, selects the lowest-energy enemy, and broadcasts `TargetLocked` with that enemy's name every 20 ticks. `BluePatrol`, `BlueSniper`, and `BlueWarden` each listen for `TargetLocked` and update their internal priority target. The whole swarm focusses fire on the same tank with zero radar duplication overhead.

### Scout broadcast pattern (Red Swarm)

`RedScout` spins its radar at full speed, relying entirely on the automatic `RadarShare` to flood the attacker tanks with fresh enemy contacts every tick. The attackers never need their own dedicated radar strategy.

### ECM alert chain

When `BlueEcmOperator` detects a ghost contact (name starts with `"Ghost-"`), it broadcasts `EcmAlert`. Any other Blue tank that implements `OnSwarmMessage` can respond by activating Burnthrough for a fixed duration, coordinating ECCM coverage across the swarm without a central controller.

### Painted counter-targeting

Any tank that receives a `Painted` message from an ally knows an enemy scanner's exact position that tick. An attacker can use `e.Message.Position` directly for a firing solution — even before its own radar has locked onto that enemy.

---

[← Tank AI Framework](ch05-tank-ai-framework.md) | [Table of Contents](TOC.md) | [Next: Radar System →](ch07-radar-system.md)
