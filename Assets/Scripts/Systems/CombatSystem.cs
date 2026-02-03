using System.Collections.Generic;
using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Resolves turn-based auto-battles synchronously.
    /// Call RunBattle() to get a complete CombatLog.
    /// </summary>
    public static class CombatSystem
    {
        private const int MAX_ROUNDS = 50;

        /// <summary>
        /// Run a full battle: player party vs a list of enemies.
        /// Returns a CombatLog with every turn recorded.
        /// </summary>
        public static CombatLog RunBattle(List<Combatant> party, List<Combatant> enemies)
        {
            var log = new CombatLog();
            var allCombatants = new List<Combatant>();
            allCombatants.AddRange(party);
            allCombatants.AddRange(enemies);

            int round = 0;

            while (round < MAX_ROUNDS)
            {
                round++;

                // Sort by SPD descending. Ties: player side goes second (defender advantage)
                allCombatants.Sort((a, b) =>
                {
                    int spdCompare = b.stats.spd.CompareTo(a.stats.spd);
                    if (spdCompare != 0) return spdCompare;
                    return a.side == CombatantSide.Player ? 1 : -1;
                });

                foreach (var attacker in allCombatants)
                {
                    if (!attacker.IsAlive) continue;

                    var target = PickTarget(attacker, party, enemies);
                    if (target == null) break;

                    int damage = CalculateDamage(attacker.stats, target.stats);
                    target.currentHP -= damage;
                    if (target.currentHP < 0) target.currentHP = 0;

                    log.turns.Add(new CombatTurn
                    {
                        roundNumber = round,
                        attackerName = attacker.name,
                        defenderName = target.name,
                        damage = damage,
                        defenderHPAfter = target.currentHP,
                        defenderDied = !target.IsAlive
                    });

                    // Check for battle end after each action
                    if (AllDead(enemies))
                    {
                        log.result = CombatResult.Victory;
                        log.totalRounds = round;
                        CollectSurvivors(log, party);
                        return log;
                    }

                    if (AllDead(party))
                    {
                        log.result = CombatResult.Defeat;
                        log.totalRounds = round;
                        return log;
                    }
                }
            }

            // Hit max rounds — treat as defeat
            log.result = CombatResult.Defeat;
            log.totalRounds = round;
            return log;
        }

        /// <summary>
        /// High-level convenience: build combatants from game state, run battle, apply rewards.
        /// </summary>
        public static CombatLog Fight(GameState state, VikingDatabase vikDB,
            EquipmentDatabase eqDB, EnemyData[] enemyDatas)
        {
            var partyVikings = state.vikings.GetParty();
            if (partyVikings.Count == 0)
            {
                Debug.LogWarning("[CombatSystem] No vikings in party.");
                return null;
            }

            // Build player combatants
            var party = new List<Combatant>();
            for (int i = 0; i < partyVikings.Count; i++)
            {
                var viking = partyVikings[i];
                var vikData = vikDB.GetViking(viking.vikingDataId);
                var fullStats = VikingSystem.GetFullStats(state, viking, vikDB, eqDB);

                party.Add(new Combatant
                {
                    name = vikData != null ? vikData.displayName : $"Viking #{viking.uniqueId}",
                    side = CombatantSide.Player,
                    index = i,
                    stats = fullStats,
                    currentHP = fullStats.hp,
                    vikingUniqueId = viking.uniqueId
                });
            }

            // Build enemy combatants
            var enemies = new List<Combatant>();
            for (int i = 0; i < enemyDatas.Length; i++)
            {
                var ed = enemyDatas[i];
                enemies.Add(new Combatant
                {
                    name = ed.displayName,
                    side = CombatantSide.Enemy,
                    index = i,
                    stats = new StatBlock { hp = ed.hp, atk = ed.atk, def = ed.def, spd = ed.spd },
                    currentHP = ed.hp,
                    enemyDataId = ed.enemyId
                });
            }

            // Run the battle
            var log = RunBattle(party, enemies);

            // Apply rewards on victory
            if (log.result == CombatResult.Victory)
                ApplyRewards(state, vikDB, eqDB, log, enemyDatas);

            return log;
        }

        /// <summary>
        /// Damage formula: max(1, ATK - DEF/2).
        /// DEF halving gives diminishing returns — high ATK always deals something,
        /// stacking DEF helps but never makes you invincible.
        /// </summary>
        public static int CalculateDamage(StatBlock attacker, StatBlock defender)
        {
            int raw = attacker.atk - defender.def / 2;
            return Mathf.Max(1, raw);
        }

        /// <summary>
        /// Target selection: attack the living enemy with the lowest current HP.
        /// Focuses fire for faster kills, which is the optimal auto-battle strategy.
        /// </summary>
        private static Combatant PickTarget(Combatant attacker, List<Combatant> party, List<Combatant> enemies)
        {
            var targetPool = attacker.side == CombatantSide.Player ? enemies : party;

            Combatant best = null;
            foreach (var c in targetPool)
            {
                if (!c.IsAlive) continue;
                if (best == null || c.currentHP < best.currentHP)
                    best = c;
            }
            return best;
        }

        private static bool AllDead(List<Combatant> group)
        {
            foreach (var c in group)
            {
                if (c.IsAlive) return false;
            }
            return true;
        }

        private static void CollectSurvivors(CombatLog log, List<Combatant> party)
        {
            foreach (var c in party)
            {
                if (c.IsAlive)
                    log.survivingVikingIds.Add(c.vikingUniqueId);
            }
        }

        /// <summary>
        /// Distribute XP to surviving party members, grant resource drops,
        /// and roll for equipment loot.
        /// </summary>
        private static void ApplyRewards(GameState state, VikingDatabase vikDB,
            EquipmentDatabase eqDB, CombatLog log, EnemyData[] enemyDatas)
        {
            int totalXP = 0;

            foreach (var ed in enemyDatas)
            {
                totalXP += ed.xpReward;

                // Resource drops
                if (ed.resourceDrops != null)
                {
                    foreach (var drop in ed.resourceDrops)
                    {
                        state.resources.Add(drop.resourceType, drop.amount);
                        log.resourceRewards.Add(new ResourceReward
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
                        log.lootedEquipmentNames.Add(ed.lootDrop.displayName);
                    }
                }
            }

            log.totalXP = totalXP;

            // Distribute XP evenly to surviving party members
            if (log.survivingVikingIds.Count > 0)
            {
                int xpEach = totalXP / log.survivingVikingIds.Count;
                foreach (int vikId in log.survivingVikingIds)
                {
                    var viking = state.vikings.GetById(vikId);
                    if (viking != null)
                        VikingSystem.AddXP(viking, vikDB, xpEach);
                }
            }
        }
    }
}
