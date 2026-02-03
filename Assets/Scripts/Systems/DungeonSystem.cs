using System.Collections.Generic;
using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Result of a dungeon action (start, advance, retreat).
    /// </summary>
    public class DungeonResult
    {
        public bool success;
        public string message;
        public DungeonRun run;
        public CombatLog lastCombatLog;

        public static DungeonResult Fail(string msg) =>
            new DungeonResult { success = false, message = msg };
    }

    /// <summary>
    /// Manages dungeon runs: starting, floor-by-floor combat with HP carryover,
    /// retreat, completion, energy, and reward distribution.
    /// </summary>
    public static class DungeonSystem
    {
        /// <summary>
        /// Start a new dungeon run. Spends energy, initializes party HP tracking.
        /// </summary>
        public static DungeonResult StartRun(GameState state, DungeonDatabase dungDB,
            DungeonData dungeon, VikingDatabase vikDB, EquipmentDatabase eqDB)
        {
            // Check prerequisite
            if (dungeon.prerequisiteBuilding != null)
            {
                int bldLevel = state.buildings.GetLevel(dungeon.prerequisiteBuilding.buildingId);
                if (bldLevel < dungeon.prerequisiteLevel)
                    return DungeonResult.Fail(
                        $"Requires {dungeon.prerequisiteBuilding.displayName} level {dungeon.prerequisiteLevel}.");
            }

            // Check party
            var party = state.vikings.GetParty();
            if (party.Count == 0)
                return DungeonResult.Fail("No vikings in party.");

            // Check energy
            if (!state.dungeons.SpendEnergy(dungeon.energyCost))
                return DungeonResult.Fail("Not enough energy.");

            // Initialize the run
            var run = new DungeonRun
            {
                dungeonId = dungeon.dungeonId,
                currentFloor = 0,
                totalFloors = dungeon.floorCount
            };

            // Record starting HP for each party member (full HP)
            foreach (var viking in party)
            {
                var fullStats = VikingSystem.GetFullStats(state, viking, vikDB, eqDB);
                run.partyHP[viking.uniqueId] = fullStats.hp;
            }

            Debug.Log($"[DungeonSystem] Started run: {dungeon.displayName} ({dungeon.floorCount} floors)");

            return new DungeonResult { success = true, run = run };
        }

        /// <summary>
        /// Fight the current floor. Uses carried-over HP.
        /// On victory, banks loot and advances. On defeat, run ends with no rewards.
        /// </summary>
        public static DungeonResult AdvanceFloor(GameState state, DungeonRun run,
            DungeonData dungeon, VikingDatabase vikDB, EquipmentDatabase eqDB)
        {
            if (run.IsComplete)
                return DungeonResult.Fail("Dungeon already complete.");

            var partyVikings = state.vikings.GetParty();
            if (partyVikings.Count == 0)
                return DungeonResult.Fail("No vikings in party.");

            int floor = run.currentFloor;
            float multiplier = dungeon.GetFloorMultiplier(floor);

            // Build player combatants with carried-over HP
            var partyCombatants = new List<Combatant>();
            for (int i = 0; i < partyVikings.Count; i++)
            {
                var viking = partyVikings[i];
                var fullStats = VikingSystem.GetFullStats(state, viking, vikDB, eqDB);

                int carriedHP = run.partyHP.ContainsKey(viking.uniqueId)
                    ? run.partyHP[viking.uniqueId]
                    : fullStats.hp;

                // Skip dead vikings
                if (carriedHP <= 0) continue;

                partyCombatants.Add(new Combatant
                {
                    name = vikDB.GetViking(viking.vikingDataId)?.displayName ?? $"Viking #{viking.uniqueId}",
                    side = CombatantSide.Player,
                    index = i,
                    stats = fullStats,
                    currentHP = carriedHP,
                    vikingUniqueId = viking.uniqueId
                });
            }

            if (partyCombatants.Count == 0)
                return DungeonResult.Fail("All party members are dead.");

            // Build scaled enemy combatants
            var enemyDatas = dungeon.GetFloorEnemies(floor);
            var enemyCombatants = new List<Combatant>();
            for (int i = 0; i < enemyDatas.Length; i++)
            {
                var ed = enemyDatas[i];
                enemyCombatants.Add(new Combatant
                {
                    name = ed.displayName,
                    side = CombatantSide.Enemy,
                    index = i,
                    stats = ScaleStats(ed, multiplier),
                    currentHP = Mathf.RoundToInt(ed.hp * multiplier),
                    enemyDataId = ed.enemyId
                });
            }

            // Run combat
            var log = CombatSystem.RunBattle(partyCombatants, enemyCombatants);
            run.floorLogs.Add(log);

            // Update carried HP
            foreach (var combatant in partyCombatants)
                run.partyHP[combatant.vikingUniqueId] = combatant.currentHP;

            if (log.result == CombatResult.Defeat)
            {
                Debug.Log($"[DungeonSystem] Defeated on floor {floor + 1}. Run over.");
                return new DungeonResult
                {
                    success = false,
                    message = $"Defeated on floor {floor + 1}.",
                    run = run,
                    lastCombatLog = log
                };
            }

            // Victory on this floor — bank rewards
            BankFloorRewards(state, run, log, enemyDatas);
            run.currentFloor++;

            Debug.Log($"[DungeonSystem] Cleared floor {floor + 1}/{run.totalFloors}");

            // Check dungeon completion
            if (run.IsComplete)
            {
                ApplyCompletion(state, run, dungeon, vikDB);
                Debug.Log($"[DungeonSystem] Dungeon {dungeon.displayName} completed!");
            }

            return new DungeonResult
            {
                success = true,
                message = run.IsComplete ? "Dungeon complete!" : $"Floor {floor + 1} cleared.",
                run = run,
                lastCombatLog = log
            };
        }

        /// <summary>
        /// Retreat from the dungeon. Player keeps banked loot from cleared floors
        /// but gets no completion bonus.
        /// </summary>
        public static DungeonResult Retreat(GameState state, DungeonRun run, VikingDatabase vikDB)
        {
            // Banked loot was already applied per-floor, so just distribute XP
            DistributeXP(state, run, vikDB);

            Debug.Log($"[DungeonSystem] Retreated after floor {run.currentFloor}. Banked loot kept.");

            return new DungeonResult
            {
                success = true,
                message = $"Retreated. Kept loot from {run.currentFloor} floor(s).",
                run = run
            };
        }

        /// <summary>
        /// Tick energy regeneration. Called from GameManager.Update().
        /// </summary>
        public static void TickEnergy(DungeonState dungeonState, float deltaTime)
        {
            dungeonState.RegenEnergy(deltaTime);
        }

        /// <summary>
        /// Apply offline energy regeneration.
        /// </summary>
        public static void ApplyOfflineEnergy(DungeonState dungeonState, double offlineSeconds)
        {
            dungeonState.RegenEnergy((float)offlineSeconds);
        }

        /// <summary>
        /// Check if a dungeon is unlocked for the player.
        /// </summary>
        public static bool IsUnlocked(GameState state, DungeonData dungeon)
        {
            if (dungeon.prerequisiteBuilding == null) return true;
            return state.buildings.GetLevel(dungeon.prerequisiteBuilding.buildingId)
                >= dungeon.prerequisiteLevel;
        }

        private static StatBlock ScaleStats(EnemyData ed, float multiplier)
        {
            return new StatBlock
            {
                hp = Mathf.RoundToInt(ed.hp * multiplier),
                atk = Mathf.RoundToInt(ed.atk * multiplier),
                def = Mathf.RoundToInt(ed.def * multiplier),
                spd = Mathf.RoundToInt(ed.spd * multiplier)
            };
        }

        /// <summary>
        /// Bank loot from a cleared floor. Resources are granted immediately,
        /// equipment goes to inventory, XP is accumulated for distribution later.
        /// </summary>
        private static void BankFloorRewards(GameState state, DungeonRun run,
            CombatLog log, EnemyData[] enemyDatas)
        {
            foreach (var ed in enemyDatas)
            {
                run.bankedXP += ed.xpReward;

                // Resource drops
                if (ed.resourceDrops != null)
                {
                    foreach (var drop in ed.resourceDrops)
                    {
                        state.resources.Add(drop.resourceType, drop.amount);
                        run.bankedResources.Add(new ResourceReward
                        {
                            type = drop.resourceType,
                            amount = drop.amount
                        });
                    }
                }

                // Equipment loot roll
                if (ed.lootDrop != null && Random.value <= ed.dropChance)
                {
                    if (!state.inventory.IsBagFull())
                    {
                        var item = EquipmentSystem.CreateInstance(ed.lootDrop);
                        state.inventory.AddItem(item);
                        run.bankedEquipmentNames.Add(ed.lootDrop.displayName);
                    }
                }
            }
        }

        /// <summary>
        /// Apply dungeon completion: bonus rewards, XP distribution, progress tracking.
        /// </summary>
        private static void ApplyCompletion(GameState state, DungeonRun run,
            DungeonData dungeon, VikingDatabase vikDB)
        {
            // Completion bonus resources
            if (dungeon.completionRewards != null)
            {
                foreach (var reward in dungeon.completionRewards)
                {
                    state.resources.Add(reward.resourceType, reward.amount);
                    run.bankedResources.Add(new ResourceReward
                    {
                        type = reward.resourceType,
                        amount = reward.amount
                    });
                }
            }

            // Completion bonus XP
            run.bankedXP += dungeon.completionBonusXP;

            // Distribute all banked XP
            DistributeXP(state, run, vikDB);

            // Update progress
            var progress = state.dungeons.GetOrCreateProgress(dungeon.dungeonId);
            progress.timesCompleted++;
            if (run.totalFloors > progress.highestFloorCleared)
                progress.highestFloorCleared = run.totalFloors;
        }

        /// <summary>
        /// Split banked XP among surviving party members.
        /// </summary>
        private static void DistributeXP(GameState state, DungeonRun run, VikingDatabase vikDB)
        {
            if (run.bankedXP <= 0) return;

            // Find living party members
            var survivors = new List<VikingInstance>();
            foreach (var kvp in run.partyHP)
            {
                if (kvp.Value > 0)
                {
                    var viking = state.vikings.GetById(kvp.Key);
                    if (viking != null) survivors.Add(viking);
                }
            }

            if (survivors.Count == 0) return;

            int xpEach = run.bankedXP / survivors.Count;
            foreach (var viking in survivors)
                VikingSystem.AddXP(viking, vikDB, xpEach);

            run.bankedXP = 0;
        }
    }
}
