# 🕒 Timer System Documentation

## 🧠 Overview

The Timer System is a network-synchronized tool used to trigger events after a certain amount of time.

It is built using a modular architecture:

* A base **NetworkTimer** handles time tracking
* Specialized components (emitters) define behavior
* EventChannels are used to communicate with other systems

---

## 🧩 Architecture

```id="k3x7vn"
NetworkTimer → EventEmitter → EventChannel → Listeners (Doors, etc.)
```

---

## 🔧 Core Component

### NetworkTimer

Handles time using Fusion's TickTimer.

**Responsibilities:**

* Start a timer
* Pause / Resume
* Track remaining time
* Notify when completed

**Important:**

* Uses network time (TickTimer)
* Only State Authority should control it

---

## 🔄 Timer Emitters

Emitters define what happens when the timer finishes.

---

### LoopingNetworkTimerEventEmitter

Repeats a timer indefinitely and emits alternating boolean values.

**Behavior:**

* Starts a timer
* On completion:

    * Toggles internal state
    * Raises an event
    * Restarts the timer

---

## 📡 Integration with Event Channels

Timers do not directly interact with gameplay objects.

Instead, they communicate through EventChannels:

```id="t2n5qx"
Timer → EventChannel → Door
```

---

## 🧠 Use Cases

### Dynamic Maze

* A looping timer emits events every few seconds
* Doors listen to the same EventChannel
* Doors open/close in sync

---

### Phase Changes

* A timer triggers events after a delay
* Used for boss spawns or level transitions

---

## ⚠️ Networking Notes

* Timers must run on objects with NetworkObject
* Only State Authority should start or modify timers
* Events should be triggered by the authority

---

## ✅ Best Practices

* Keep timers generic
* Use emitters for behavior
* Avoid coupling timers with gameplay logic
* Use EventChannels for communication

---

## 🚀 Extensibility

You can extend the system with:

* Delayed timers
* Pattern-based timers
* Multi-channel emitters
* Conditional timers

---
