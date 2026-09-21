# CraftyNative

**The runtime engine powering Crafty.**

CraftyNative is the specialized runtime engine used by **Crafty**, a data-oriented voxel game.

It sits on top of **Vulcan**, providing the runtime systems required by Crafty while leaving the actual game logic, gameplay mechanics, and content to `Crafty.Engine`.

CraftyNative is not intended to be a general-purpose game engine or framework. It exists to provide the runtime that Crafty needs.

## Architecture

Crafty is built as a layered stack:

```text
Launcher
    ↓
Crafty.Engine
    ↓
CraftyNative
    ↓
Vulcan
    ↓
Graphics API
    ↓
GPU
```

Each layer has a specific responsibility.

### Vulcan

**Vulcan** is the low-level graphics foundation.

It provides the graphics abstraction and direct access to the underlying graphics APIs and GPU resources used by CraftyNative.

```text
Vulcan
├── Graphics device
├── Buffers
├── Textures
├── Samplers
├── Shaders
├── Pipelines
├── Command buffers
└── GPU synchronization
```

Vulcan is intentionally low-level. It does not know what Crafty gameplay is or how the game world works.

### CraftyNative

**CraftyNative is the runtime engine.**

It sits on Vulcan and provides the systems required to actually run Crafty:

* ECS
* Scene management
* World objects
* Components
* Systems
* Entity hierarchies
* Input
* Runtime management
* Rendering systems
* Texture management
* Mod loading
* Mod hot reload
* Integration with Vulcan

### Crafty.Engine

**Crafty.Engine is the actual game.**

It contains Crafty's game-specific implementation:

* Gameplay
* Player systems
* World generation
* Blocks
* Items
* Crafting
* Game mechanics
* Game-specific systems
* Game content
* Multiplayer
* Other Crafty-specific behavior

The distinction is intentional.

**Vulcan provides the graphics foundation.**

**CraftyNative runs the game.**

**Crafty.Engine is the game.**

## ECS

CraftyNative uses a data-oriented **Entity Component System**.

The runtime is built around the principle:

> **Everything is data.**

`WorldObject` represents an entity identity rather than a traditional object containing behavior.

Components contain state, while systems operate on that state.

```text
Scene
├── WorldObjects
├── Components
├── Parent/Child relationships
└── Systems
```

A world object can be composed from multiple components:

```csharp
var player = scene.CreateObject();

scene.AddComponent(player, new Transform());
scene.AddComponent(player, new Player());
```

Systems query the data they need:

```csharp
foreach (var player in scene.GetEntitiesWith<Player>())
{
    ref var transform = ref scene.GetComponent<Transform>(player);

    // Modify player data
}
```

There is no requirement for gameplay entities to inherit from engine classes.

## Scenes

`Scene` is the central runtime container.

It contains:

* World object relationships
* Component storage
* Rendering data
* Systems
* Runtime state

This gives CraftyNative a single location through which the runtime can operate on the current game state.

## Systems

Systems contain runtime behavior.

For example:

```text
Input
  ↓
PlayerCameraSystem
  ↓
Transform
  ↓
HierarchySystem
  ↓
World transform
```

Systems operate on component data rather than owning game objects.

Required runtime systems can be registered internally by CraftyNative itself.

## Hierarchies

CraftyNative supports parent/child relationships between world objects.

For example:

```text
Player
├── Model
└── Camera
```

These are still independent world objects stored in the same scene.

The hierarchy is represented through relationship data and local/world transforms rather than nested object ownership.

## Rendering

CraftyNative's rendering system is built on top of **Vulcan**.

The relationship is:

```text
Crafty.Engine
      ↓
CraftyNative rendering
      ↓
Vulcan
      ↓
Graphics API
      ↓
GPU
```

CraftyNative handles the game-runtime side of rendering, while Vulcan provides the low-level graphics functionality.

This keeps the path from Crafty's data to GPU operations direct and controllable without requiring CraftyNative to become a general-purpose graphics framework.

## Modding

Modding is a first-class part of Crafty's runtime.

CraftyNative loads mods as .NET assemblies and uses reflection to discover the required mod information.

The basic flow is:

```text
Mod DLL
  ↓
Assembly loading
  ↓
Reflection
  ↓
Mod discovery
  ↓
ModVault
  ↓
Crafty runtime
```

Reflection is primarily used during loading and discovery. Loaded mod information is retained by the runtime through `ModVault`.

### Multithreaded Loading

Mod loading can perform independent loading work concurrently, reducing startup time when multiple mods are installed.

### Hot Reload

CraftyNative supports mod hot reload.

When a mod is updated while Crafty is running:

```text
Detect update
    ↓
Pause game
    ↓
Unload old mod
    ↓
Load updated mod
    ↓
Discover/register new mod
    ↓
Finish reload
    ↓
Resume game
```

The game cannot resume while the mod reload operation is in progress.

This allows mod developers to modify their code, rebuild the mod, and continue testing without restarting Crafty.

## Launcher

The launcher is separate from both the game and the runtime engine.

```text
Launcher
    │
    └── Launch
          ↓
      Crafty.Engine
          ↓
      CraftyNative
          ↓
        Vulcan
```

The launcher acts as the menu and game-management layer.

Crafty.Engine is the actual game.

CraftyNative is the runtime underneath it.

Vulcan provides the low-level graphics foundation underneath CraftyNative.

## Design Goals

CraftyNative is designed around:

* Data-oriented architecture
* Low runtime overhead
* Fast startup
* Low memory usage
* Stable frame times
* Direct runtime control
* First-class modding
* Fast mod iteration
* Minimal unnecessary abstraction
* Crafty-specific requirements

CraftyNative does not attempt to solve every problem a general-purpose engine might encounter.

It solves the problems **Crafty actually has**.

## Development Philosophy

CraftyNative is intentionally kept understandable.

The runtime should be traceable directly through its source code:

```text
Data
 ↓
System
 ↓
Runtime
 ↓
Rendering
 ↓
Vulcan
 ↓
GPU
```

Rather than hiding the runtime behind layers of unnecessary abstraction, CraftyNative keeps the architecture explicit and specialized.

## Project Relationship

```text
                         Crafty
                            │
                    ┌───────▼───────┐
                    │ Crafty.Engine │
                    │               │
                    │  Actual game  │
                    │  Gameplay     │
                    │  World        │
                    │  Content      │
                    │  Multiplayer  │
                    └───────┬───────┘
                            │
                    ┌───────▼───────┐
                    │  CraftyNative │
                    │               │
                    │ Runtime engine│
                    │ ECS           │
                    │ Systems       │
                    │ Scene         │
                    │ Rendering     │
                    │ Input         │
                    │ Modding       │
                    └───────┬───────┘
                            │
                    ┌───────▼───────┐
                    │     Vulcan    │
                    │               │
                    │ Low-level     │
                    │ graphics      │
                    └───────┬───────┘
                            │
                       Graphics API
                            │
                           GPU
```

## Status

CraftyNative is actively developed alongside Crafty.Engine and Vulcan.

Current runtime development includes:

* ECS
* Scene management
* Components
* Systems
* Entity hierarchies
* Player movement
* Camera control
* 3D rendering
* Textures
* Mod loading
* Multithreaded mod loading
* Mod hot reload
* Vulcan integration
* SDK integration

---

**Vulcan provides the graphics foundation.**

**CraftyNative runs Crafty.**

**Crafty.Engine is Crafty.**