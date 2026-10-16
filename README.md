# Crafty Project Documentation

Oct 16, 2026 · @Boba

Crafty is a data-oriented voxel game written in C# on .NET 10, built on a custom runtime (CraftyNative) and a low-level graphics layer (Vulcan). The repository holds the game, the runtime, world generation, the mod SDK and launcher-side management libraries. Current release is `26.10.8 Alpha-1`; Alpha-2 is next.

## Overview

The project is a stack of layers, each with one job:

| Layer | Role |
| --- | --- |
| Launcher | Menu and game management; starts the game |
| Crafty | The actual game: gameplay, player, blocks, items, HUD, content |
| CraftyNative | Runtime engine: ECS, scenes, input, rendering systems, textures, mod loading |
| Vulcan | Low-level graphics foundation (device, buffers, shaders, pipelines); lives outside this repo |

**Design principles**

- Everything is data: entities are identities, components hold state, systems hold behavior.
- Minimal abstraction and a traceable path from data to GPU.
- Low startup time, low memory use, stable frame times.
- Modding is first-class, with multithreaded mod loading and hot reload.
- CraftyNative is deliberately not a general-purpose engine; it solves Crafty's problems only.

**Tech stack**

- C# with .NET 10 (`net10.0`), nullable enabled, unsafe code allowed in the runtime.
- Silk.NET 2.23 (Input, Windowing with GLFW and SDL) in Crafty.
- StbImageSharp and StbTrueTypeSharp for image and font loading in CraftyNative.
- Native AOT publishing (`PublishAot`) for Crafty.
- `Microsoft.Extensions.DependencyInjection.Abstractions` in the management and generation libraries.

## Project structure

The solution (`Crafty.slnx`) has eight projects in this repo, plus two Vulcan projects referenced from an absolute path on the author's machine (`D:\My Programs\Vulcan`).

| Project | Type | Purpose |
| --- | --- | --- |
| `Crafty` | Exe (assembly `Crafty`, Native AOT) | The game: window, systems, components, HUD and inventory UI, built-in content mod, mesh building, chunk jobs, assets |
| `CraftyNative` | Library | Runtime: ECS, scene, input, window, rendering, UI framework, HUD widgets, sky, animation, physics helpers |
| `Crafty.SDK` | Library | Public mod API: `IMod`, `IModContext`, block/item registries, chunk and generation interfaces |
| `Crafty.ChunkGeneration` | Library (NuGet `0.1.0-ea`) | World, chunk and generator implementation, world save and load |
| `Crafty.GameManagement` | Library | Launcher-side service interfaces for worlds, launch data and the game (mostly stubs) |
| `Crafty.InstancesManager` | Library (NuGet `0.1.0-ea`) | Launcher-side game instance records, channels and JSON persistence |
| `Crafty.MDK` | Library | Empty mod development kit project (placeholder) |
| `Crafty.Testing` | Exe | Console harness for world generation |

**Dependency direction**

- `Crafty` references `CraftyNative`, `Crafty.SDK` and `Crafty.ChunkGeneration`.
- `CraftyNative` references Vulcan and `Crafty.ChunkGeneration`.
- `Crafty.ChunkGeneration` references `Crafty.SDK`.
- `Crafty.SDK` references nothing.

**Key folders in `Crafty`**

- `Components/`: plain data structs (`Player`, `Movement`, `Inventory`, `Hotbar`, `Damageable`, `Attacker`, `Attackable`, `FallDamage`).
- `Systems/`: behavior (player, camera, movement, collision, combat, damage, inventory, hotbar, chunk streaming, mesh updates, world time, sky light, hand view model).
- `Jobs/`: background work for chunk load, unload, mesh build and world update.
- `Core/`: registries, world manager, logger, mod context, `GameAPIs` bootstrap.
- `MainMod/`: the built-in content mod (`MainMod`, `BlocksCreator`).
- `UI/`: `HudInventory`, `HudHotbar`, `HudHearts`, `PauseMenu` and the `Inventory/` page classes.
- `Assets/`: block textures grouped by category, HUD heart sprites, sun and moon, the default skin and Roboto-Regular.

**Key folders in `CraftyNative`**

- `ECS/`, `Scenes/`: component storage, job system, scene, world objects, hierarchy.
- `UI/`: UI element tree, layout containers and renderers.
- `HUD/`: `Slot`, `SlotGrid`, `SlotStackLayout`, `Crosshair`, `HudRenderer`.
- `ThreeD/`: textures, materials, meshes, chunk batches, world mesh manager, voxel raycast, AABB.
- `Environment/`: sky, sun and moon renderers.
- `Animation/`: rigid part rigs for player, view model and camera.

## Architecture

Game state lives in a `Scene` of plain structs; systems run once per frame in registration order and mutate that state.

### Startup

1. `Program.Main` requires `--config-path <launch.json>`, deserializes it into `GameLaunchConfigDto` (one field: `SelectedWorldPath`) and registers GLFW windowing and input.
2. `MainWindow` is created. Its constructor creates a `WorldGenService` for the world folder and runs `GameAPIs.Initialize()`, which scans `Assets`, builds the block and item registries and runs the built-in `MainMod`.
3. On window activation, `MainWindow` builds the `Scene`: a player object, a camera child, a hand child, a held-block child and a player model; initializes the HUD; loads the world; and registers all systems.
4. A `JobSystem` is assigned to `SystemAPI.JobSystem` for background work.

### ECS in CraftyNative

- `WorldObject` is a `uint` id wrapper. `Scene.CreateObject()` hands out ids.
- Components are `unmanaged` structs implementing `IComponent`, stored per type in `ComponentStore<T>` (a dictionary plus an entity list). `GetComponent<T>` returns a `ref`.
- Parent links live in a dictionary. `HierarchySystem` (always registered first) sorts entities by depth and computes child world position and rotation from the parent, honoring `LockXAxis`, `LockYAxis`, `LockZAxis` and `RotateAroundParent`.
- `GameWorld.Render` runs every system, skips systems marked `IGameplaySystem` while the game is paused, then renders from the entity that has a `Camera`.
- `GameStateManager` holds the state: `Paused`, `Playing` or `Inventory`. Escape toggles pause.

### Systems registered by MainWindow

| System | Responsibility |
| --- | --- |
| `WorldTimeSystem` | Advances time of day (1800 s per day), moves the sun, sets sky colors, daylight and moonlight |
| `ChunkStreamingSystem` | Loads and unloads chunks within 8 chunks of the player, then queues mesh builds |
| `HandViewModelSystem` | Animates the first-person hand and held block |
| `PlayerSystem`, `PlayerCameraSystem` | Player state and mouse-look camera |
| `PlayerMovementSystem` | WASD movement, sprint toggle on left Ctrl, double-tap Space toggles flight, gravity 40, jump 10, speed 5 (+5 sprint) |
| `CollisionSystem` | Player against world blocks using the `Collider` AABB |
| `CombatSystem`, `DamageSystem` | Empty stubs |
| `FallDamageSystem` | Tracks highest Y for fall damage |
| `InventorySystem`, `HotbarSystem` | Sync ECS inventory and hotbar with the HUD |
| `PlayerBlockInteractionSystem` | Voxel raycast (reach 6): left click breaks (0.15 s cooldown), right click places the selected block (0.10 s cooldown), with facing state from yaw |
| `WorldMeshUpdateSystem` | Submits a job to rebuild meshes for blocks marked dirty |

### Chunk pipeline

Chunks are 32 x 416 x 32 blocks. Each block is a `BlockPlacement` (`uint` id plus `byte` state). Light is one byte per block: sky light in the high nibble, block light in the low nibble.

1. The player crosses a chunk border. `ChunkStreamingSystem` collects chunks inside a circular radius and submits a `LoadChunkJob` for each missing one, prioritized by squared distance.
2. `LoadChunkJob` calls `ChunkLoader`, which reads `chunks/<x>_<z>.chunk` or generates and saves it first, then initializes sky light.
3. When the load fence completes, `BuildChunkMeshJob` builds meshes for every 16-block section of the chunk (2 x 2 sections per layer) through `MeshBuilder` and hands them to `WorldMeshManager`.
4. Chunks that left the radius get an `UnloadChunkJob` that removes meshes and the chunk.
5. Breaking or placing a block calls `World.SetBlock`, which raises `BlockChanged`. The handler updates sky light and marks the block dirty; `WorldUpdateJob` then rebuilds the affected sections.

### Jobs

`JobSystem` starts `max(2, cores - 1)` background worker threads over a priority queue. `Submit(job, priority, fence)` queues work; a `JobFence` counts outstanding jobs so a system can poll `IsComplete` or block on `Wait()`.

### Rendering

`CraftyNative.Render` draws through Vulcan in two passes: opaque and alpha-cutout geometry first, then translucent sections (glass, ice) sorted far to near with blending on and depth writes off. Shaders are HLSL embedded in `CraftyNative.cs`. Per-vertex light is multiplied by an ambient value derived from daylight and moonlight. The sky, sun and moon have their own renderers in `Environment/`.

## UI framework and inventory HUD

The UI is a retained-mode element tree in `CraftyNative/UI` with a measure, arrange, update and draw cycle, written in C# code rather than declarative data.

### Framework elements (CraftyNative)

| Element | Role |
| --- | --- |
| `UIElement` | Base class: position, size, margin, padding, alignment, visibility, mouse and hover flags, parent, `Tag`; abstract `Measure`, `Arrange`, `Update`, `Draw` |
| `Frame`, `Page` | A `Frame` hosts one `Page`; a `Page` has a background, a title and one content element |
| `Grid` | Rows and columns in pixel, auto or star units, with spacing and row and column spans |
| `Pane` | Vertical stack with spacing |
| `TabView`, `TabItem` | Tab header with icons (3D meshes), hover tooltips after 500 ms and a `SelectionChanged` event |
| `ScrollView`, `Button`, `Seperator`, `SpriteView`, `ModelView` | Scrolling, clickable text, divider, 2D sprite, and a rotatable 3D mesh preview |
| `Slot`, `SlotGrid` (HUD) | Item cell with hover, selection, count, tooltip and click event; a wrapped grid of slots |
| `UIRenderer`, `UITextRenderer`, `SpriteRenderer`, `UI3DModelsRenderer` | Draw rectangles, text (Roboto via StbTrueType), sprites and 3D models |

### Inventory (Crafty)

`HudInventory` is a static class that owns a `TabView` (470 x 480), a hotbar `SlotGrid`, an in-hand cursor slot and two pages that implement `IInventoryLayout`: `InventoryPage` (survival) and `CreativeInventoryPage`.

- `IInventoryLayout` exposes `Storage`, `ArmorSlots`, `CraftingSlots` and `CraftingResultSlot` so `HudInventory` can sync any page with the ECS.
- Slot indices form one flat space: 0 to 8 hotbar, 9 to 35 storage, 36 to 39 armor, 40 to 43 crafting, 44 crafting result.
- `TabPage` is a scrollable 9 x 6 grid of items for a creative tab. `HudInventory.AddTab` and `SetItem` fill it from the registries.
- `SlotInteraction` implements click behavior (pick up, place, split with right click, stack to 64) and the held-item cursor; `Styles` holds colors and slot size.
- `SetCreativeMode(bool)` switches between the survival and creative pages. Creative and survival are separated only in the UI layer.

### How the inventory stays in sync with the ECS

1. The player entity carries `Inventory` (36 storage, 4 armor, 5 crafting slots) and `Hotbar` (9 slots) components; each slot is `InventorySlot { ItemId, BlockId?, Count }`.
2. When the player clicks a slot, `HudInventory.SlotChanged(index, itemId, count)` fires and `InventorySystem` queues the change.
3. On its next update `InventorySystem` writes the change into the components. In the other direction it calls `HudInventory.SetSlot` when the components change.
4. `InventorySystem.MeshResolver` and `BlockIdResolver` (set in `MainWindow`) turn a slot into an icon mesh and an item id into a block id through the registries.
5. Pressing E opens or closes the inventory and switches `GameStateManager` between `Inventory` and `Playing`, toggling raw cursor mode.

### HUD pieces

`HudHotbar` (9 slots, selection), `HudHearts` (full, half and empty heart sprites), `Crosshair` and `PauseMenu` (Resume and Quit buttons; quit saves modified chunks and deletes unchanged chunk files).

## Build, run and configuration

The game targets Windows: paths such as `Assets\Textures\...` and `\chunks` use backslashes, and Vulcan's graphics device is created through `Vulcan.DirectX`.

**Prerequisites**

- .NET 10 SDK.
- The Vulcan source tree. `CraftyNative.csproj` references `..\..\..\..\My Programs\Vulcan\Vulcan\Vulcan.csproj` and `Crafty.slnx` references `D:\My Programs\Vulcan\...`; neither is in this repo, so adjust both paths to your checkout.
- For `dotnet publish` with Native AOT, the platform C++ toolchain.

**Run the game**

1. Build `Crafty`. A post-build target copies everything under `Assets\` into the output folder.
2. Create a launch config JSON file with the world to open:

```json
{ "SelectedWorldPath": "C:\\path\\to\\saves\\MyWorld\\MyWorld.world" }
```

3. Start with `Crafty.exe --config-path launch.json`. This is also the Visual Studio launch profile. Without two arguments the game throws `Missing arg!`.

**Save format**

- A world is a folder with `<name>.world` (Deflate-compressed: name string, seed `UInt64`) and a `chunks/` folder with `<x>_<z>.chunk` files.
- A chunk file is Deflate-compressed: block count, `IsModified` flag, then `uint` id and `byte` state per block.
- Missing chunks are generated on demand from the world seed and saved. On quit, modified chunks are saved and unchanged chunk files are deleted.

**Create a test world**

`IWorldGenService.GenerateWorld(worldDir, worldName, seed, progress)` writes a 10 x 10 chunk area in parallel and reports progress. The current generator makes flat terrain: stone below y = 61, dirt up to y = 63, grass at y = 64, air above.

**Gameplay controls**

| Input | Action |
| --- | --- |
| W A S D | Move |
| Space (double tap) | Toggle flying |
| Space / Shift (flying) | Up / down |
| Left Ctrl | Toggle sprint |
| Mouse | Look |
| Left click / right click | Break / place block |
| E | Open or close inventory |
| Escape | Pause or resume; close inventory |

The player spawns flying at (0, 75, 0), in survival mode.

**Publish settings**

`Crafty.csproj` enables `PublishAot`, trimming analysis and invariant globalization; it also sets `AllowUnsafeBlocks`. JSON for the launch config uses a source-generated context (`LaunchConfigJsonContext`) to stay AOT-safe.

## Extending the project and known gaps

### Add blocks and items (mod API)

Content is added by implementing `IMod` from `Crafty.SDK`. The built-in `MainMod` (id `crafty`, version `26.10.8`) is the reference. A block gets its numeric id from its category's range (Nature 0, Stone 5,000, Wood 15,000, Ore 20,000, Building 30,000, Decoration 40,000, Technical 50,000); block id 0 is air.

```csharp
public sealed class MyMod : IMod
{
    public string Id => "mymod";
    public string Name => "My Mod";
    public string Version => "1.0.0";
    public string AssetsDirectory => "Assets";

    public void Initilaize(IModContext context)
    {
        var block = BlocksCreator.CreateOneSidedTextureBlock("mymod.ruby_block", "Ruby Block");
        context.Blocks.Register(BlockCategory.Decoration, block);
        context.Items.Register(ItemCategory.Block, new Item { Name = "Ruby Block", BlockId = "mymod.ruby_block" });
    }
}
```

The texture file is named after the block id (for example `crafty.oak_planks.png`) and sits under `Assets/Textures/<category>/`. `Initilaize` is spelled that way in the interface. BlocksCreator is a helper inside Crafty; a separate mod assembly would build its own Block and BlockModel objects from Crafty.SDK.

### Add a system or component

1. Add a `struct` implementing `IComponent` under `Crafty/Components` (it must be `unmanaged`).
2. Add a class implementing `ISystem`, or `IGameplaySystem` if it should freeze while paused.
3. Register it in `MainWindow.Window_Activated` with `Scene.Systems.Add(...)`. Order matters: systems run in list order.

### Add background work

Implement `IJob`, then `SystemAPI.JobSystem.Submit(job, priority, fence)`. Use a `JobFence` to know when a batch is done.

### Known gaps in this snapshot

- **Mod loading is documented, not present.** The README describes assembly loading, `ModVault` and hot reload, but no assembly loader exists in this code; `GameAPIs.Initialize` runs only the built-in `MainMod`. `Crafty.MDK` is an empty project.
- **Terrain is flat.** `ChunkGenerator` produces a fixed layer cake at y = 64. `TerrianGenerator` throws `NotImplementedException`, `ChunkGenerationPipeline.Generate` ends with `throw new Exception()`, and the noise classes and `BiomeGenerator` are not wired in.
- **Solution file mismatch.** `Crafty.slnx` points to `Crafty/Crafty.csproj`, but the project file is `Crafty.csproj`.
- **External paths.** Vulcan is referenced from outside the repo. `Crafty.InstancesManager` uses the `Crafty.Auto` namespace (`AutoFileIO`, `AutoJson`) that is not in this repo or its project file.
- **`Crafty.Testing` is out of date.** It calls `new WorldGenService(new(...))` and `GenerateWorld()` with no arguments, which no longer match `WorldGenService(string)` and `GenerateWorld(dir, name, seed, progress)`.
- **Launcher libraries are mostly stubs.** `IWorldService` methods in `WorldService` are empty or throw, and `IGameService` and `ILaunchDataService` have no members. `InstanceService` and `InstancesManager` hold the real code (instance records with channel `Release`, `Preview`, `Beta`, `Alpha` or `EarlyAccess`, stored in `instancesData.json`).
- **Combat is stubbed.** `CombatSystem` and `DamageSystem` do nothing; `Damageable`, `Attacker` and `FallDamage` hold data only.
- **Windows-only paths** are hardcoded in asset and chunk loading.
- **Unused overload:** `ChunkLoader.Load(seed, amount)` bulk-loads existing chunk files through a `\chunks` path built by string concatenation and is never called; only the per-chunk overload is used.
