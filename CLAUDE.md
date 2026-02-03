# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Idle/Management/RPG game for Android (portrait) built with Unity 2D (LTS) and C#. Solo developer project. Pixel art visual style (16x16 or 32x32). Namespace: `IdleViking`.

## Architecture

**Data flow:** `ScriptableObject (static data) + Model (runtime state) → System (logic) → UI (display)`

- **Data** (`Assets/Scripts/Data/`): ScriptableObject definitions. Each has a corresponding Database ScriptableObject (registry with lookup).
- **Models** (`Assets/Scripts/Models/`): `[Serializable]` C# classes holding runtime state. All live inside `GameState` root object, serialized to JSON via `SaveSystem`.
- **Systems** (`Assets/Scripts/Systems/`): Static classes with pure logic. No MonoBehaviour. Operate on GameState + ScriptableObject data passed as parameters.
- **Core** (`Assets/Scripts/Core/`): GameManager (singleton, bootstrap + game loop), SaveSystem (JSON to persistentDataPath), TimeManager (offline time delta).
- **UI** (`Assets/Scripts/UI/`): Display-only controllers. Never hold game state.

**GameManager** is the single entry point. It owns `GameState` and all database references (`[SerializeField]`). It ticks systems in `Update()`, handles offline gains at startup, and auto-saves periodically + on pause/quit.

**Serialization:** Unity's `JsonUtility`. Dictionaries use `SerializableDictionary<TKey, TValue>` wrapper in Utils/. Timestamps use `DateTime.UtcNow.ToString("o")` (ISO 8601 roundtrip).

## Systems

| System | Data | Model | Logic | Purpose |
|---|---|---|---|---|
| Resource | `ResourceProducerData`, `ResourceDatabase` | `ResourceState`, `ProducerState` | `ResourceSystem` | Idle per-second production with building + workforce bonuses, offline catch-up |
| Building | `BuildingData`, `BuildingDatabase` | `BuildingState` | `BuildingSystem` | Construct/upgrade buildings, multi-resource costs, prerequisites, linked producers, global bonuses |
| Viking | `VikingData`, `VikingDatabase` | `VikingState` | `VikingSystem` | Recruit, level, assign to buildings (workforce) or party (combat), stat calculation |
| Equipment | `EquipmentData`, `EquipmentDatabase` | `InventoryState` | `EquipmentSystem` | Craft/loot items with random rolls, equip to vikings (4 slots), sell |
| Combat | `EnemyData`, `EnemyDatabase` | `CombatModels` | `CombatSystem` | Synchronous turn-based auto-battle. `dmg = max(1, ATK - DEF/2)`. SPD turn order |
| Dungeon | `DungeonData`, `DungeonDatabase` | `DungeonState` | `DungeonSystem` | Multi-floor PvE with HP carryover, enemy scaling, energy cost, retreat mechanic |
| Farm | `FarmPlotData`, `FarmDatabase` | `FarmState` | `FarmSystem` | Timer-based crops (one-shot) and animals (repeating), offline auto-harvest |

## Key Patterns

- **Production rate formula:** `baseRate * (1 + buildingBonus + workforceBonus)` where building bonus is global per resource type and workforce bonus is per-building from assigned vikings
- **Building→Producer sync:** buildings with a `linkedProducer` auto-create/level the producer. Don't upgrade producers directly.
- **Viking stats:** `baseStat + floor(growth * (level-1) * rarityMultiplier)`. Equipment adds flat bonuses. `GetFullStats()` = base + equipment.
- **Combat is instant:** `CombatSystem.RunBattle()` resolves fully and returns a `CombatLog`. UI replays it.
- **Dungeon HP carryover:** `DungeonRun` is transient (not saved). Party HP persists across floors within a run.
- **Farm timers use UTC timestamps**, not frame-based timers. `ProcessOffline()` handles catch-up for animals (multi-cycle) and auto-harvest for crops.

## Conventions

- One class per file, file name matches class name
- Separate data, logic, and UI strictly
- Prefer ScriptableObjects for all static/configurable data
- Comments only where logic isn't self-evident
- New systems follow the pattern: define ScriptableObject → create Database → create Model state class → write System logic → add state to GameState → add database to GameManager → wire UI
- All databases assigned to GameManager via `[SerializeField]` in the Inspector
