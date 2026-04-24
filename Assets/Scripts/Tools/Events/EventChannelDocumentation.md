# Event Channel System (ScriptableObject-based)

## 🧠 Overview

The Event Channel system is a **decoupled communication pattern** used to allow different parts of the game to interact **without direct references**.

Instead of scripts calling each other directly, they communicate through **shared event channels**.

---

## 🎯 Purpose

This system solves the following problems:

* ❌ Tight coupling between systems
* ❌ Hard-to-maintain references
* ❌ Poor scalability
* ❌ Difficult reuse of logic

And provides:

* ✅ Decoupled architecture
* ✅ Reusable systems
* ✅ Inspector-driven configuration
* ✅ Scalable event handling

---

## 📡 Concept

An Event Channel works like a **radio broadcast system**:

* A **Sender** raises an event
* The **Event Channel** distributes it
* Multiple **Listeners** react to it

```
Sender → EventChannel → Listeners
```

---

## 🧩 Core Implementation

### Event Channel (Bool example)

```csharp
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Bool Event Channel")]
public class BoolEventChannel : ScriptableObject
{
    public event Action<bool> OnEventRaised;

    public void RaiseEvent(bool value)
    {
        OnEventRaised?.Invoke(value);
    }
}
```

---

## ⚙️ How to Use

### 1. Create an Event Channel

1. Right-click in Project window
2. Create → `Events → Bool Event Channel`
3. Name it (e.g. `BossSpawnedEvent`)

---

### 2. Raise an Event (Sender)

```csharp
public class BossSpawner : MonoBehaviour
{
    [SerializeField] private BoolEventChannel bossSpawnedEvent;

    void SpawnBoss()
    {
        // Your spawn logic...

        bossSpawnedEvent.RaiseEvent(true);
    }
}
```

---

### 3. Listen to an Event (Receiver)

```csharp
public class ExampleListener : MonoBehaviour
{
    [SerializeField] private BoolEventChannel eventChannel;

    private void OnEnable()
    {
        eventChannel.OnEventRaised += HandleEvent;
    }

    private void OnDisable()
    {
        eventChannel.OnEventRaised -= HandleEvent;
    }

    private void HandleEvent(bool value)
    {
        Debug.Log("Event received: " + value);
    }
}
```

---

## 🔁 Data Flow

1. Sender calls:

```
RaiseEvent(value)
```

2. Event Channel invokes:

```
OnEventRaised(value)
```

3. All listeners receive:

```
HandleEvent(value)
```

---

## 📦 Project Structure

Recommended folder structure for the Event Channel system:

```
Assets/
 └── Scripts/
      └── Tools/
           └── Events/
                ├── Channels/
                │    ├── BossSpawnedEvent.asset
                │    ├── MazeTickEvent.asset
                │    └── ...
                │
                ├── EventTypes/
                │    └── BoolEventChannel.cs
                │
                └── EventChannelDocumentation.md
```

---

## 🧩 Folder Responsibilities

### 📁 Channels/

Contains all **Event Channel assets**.

Each asset represents a specific event in the game:

* Boss spawn events
* Timer events
* Environment triggers

These are assigned via the Inspector and used by both senders and listeners.

---

### 📁 EventTypes/

Contains all **ScriptableObject definitions** for event channels.

Examples:

* `BoolEventChannel`
* Future extensions like `VoidEventChannel`, `IntEventChannel`, etc.

---

### 📄 EventChannelDocumentation.md

Contains documentation explaining how the system works and how to use it.

Placed at the root level for quick and easy access.

---

## 🧠 Naming Convention (Recommended)

To keep things clean and readable, use consistent naming:

### Event Channel Assets

```
BossSpawnedEvent
MazeToggleEvent
DoorGroupAEvent
```

---

## 🧠 Best Practices

* Keep scripts and assets separated
* Use descriptive names for event channels
* Reuse existing channels instead of creating duplicates
* Avoid placing unrelated assets inside the Events folder

---

## 🧠 Key Characteristics

### ✔ Shared Instance

All references point to the **same ScriptableObject asset**

### ✔ Scene Independent

Works across scenes without needing references

### ✔ Inspector-Friendly

You can connect systems without writing code

### ✔ Decoupled

Senders and receivers do not know about each other

---

## ⚠️ Important Notes

### 1. Always Unsubscribe

```csharp
private void OnDisable()
{
    eventChannel.OnEventRaised -= HandleEvent;
}
```

Failing to do this may cause:

* Memory leaks
* Null reference errors

---

### 2. Event Channels Are NOT Networked

They:

* ❌ Do NOT sync over network
* ❌ Do NOT store gameplay state

They are only used to:

* ✅ Notify systems locally

---

### 3. Do NOT Store State Here

Avoid doing this:

```csharp
public bool currentState; // ❌
```

Event Channels should only:

* Broadcast events
* Not hold game logic or data

---

## 🔥 Use Cases

### Gameplay

* Trigger doors, traps, or mechanics
* Notify enemy behaviors

### Game Flow

* Phase changes
* Boss events

### UI

* Update HUD
* Show notifications

### Systems

* Timers
* Global triggers

---

## 🚀 Extending the System

You can create other event types:

### No data event

```csharp
public event Action OnEventRaised;
```

---

### Custom data event

```csharp
public event Action<MyData> OnEventRaised;
```

---

## 🧠 When to Use This System

Use Event Channels when:

* Multiple systems need to react to the same event
* You want to avoid direct references
* You need scalable and reusable logic

---

## ❌ When NOT to Use It

Avoid using Event Channels when:

* Only one object needs to communicate with another
* A direct reference is simpler and clearer
* You need persistent state (use other systems instead)

---

## 🏁 Summary

Event Channels provide:

* A clean way to communicate between systems
* A scalable and reusable architecture
* A Unity-friendly workflow using ScriptableObjects

They are a fundamental building block for decoupled game systems.

---
