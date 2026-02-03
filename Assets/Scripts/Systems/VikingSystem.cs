using System.Collections.Generic;
using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Handles viking recruitment, leveling, assignment, and stat calculation.
    /// </summary>
    public static class VikingSystem
    {
        public const int MAX_PARTY_SIZE = 4;

        /// <summary>
        /// Recruit a new viking. Spends recruitment costs.
        /// Returns the new instance or null if failed.
        /// </summary>
        public static VikingInstance TryRecruit(GameState state, VikingDatabase db, string vikingDataId)
        {
            var data = db.GetViking(vikingDataId);
            if (data == null)
            {
                Debug.LogWarning($"[VikingSystem] Unknown viking: {vikingDataId}");
                return null;
            }

            if (data.recruitCosts != null && data.recruitCosts.Length > 0)
            {
                if (!state.resources.Spend(data.recruitCosts))
                {
                    Debug.Log($"[VikingSystem] Can't afford to recruit {data.displayName}");
                    return null;
                }
            }

            var instance = state.vikings.Add(vikingDataId);
            Debug.Log($"[VikingSystem] Recruited {data.displayName} (#{instance.uniqueId})");
            return instance;
        }

        /// <summary>
        /// Add XP to a viking. Auto-levels up if threshold is reached.
        /// Returns true if a level-up occurred.
        /// </summary>
        public static bool AddXP(VikingInstance viking, VikingDatabase db, int xpAmount)
        {
            var data = db.GetViking(viking.vikingDataId);
            if (data == null) return false;

            viking.xp += xpAmount;
            bool leveledUp = false;

            // Allow multiple level-ups in one call
            while (true)
            {
                int xpNeeded = data.GetXPForLevel(viking.level + 1);
                if (xpNeeded <= 0 || viking.xp < xpNeeded)
                    break;

                viking.xp -= xpNeeded;
                viking.level++;
                leveledUp = true;
                Debug.Log($"[VikingSystem] Viking #{viking.uniqueId} leveled up to {viking.level}");
            }

            return leveledUp;
        }

        /// <summary>
        /// Assign a viking to a building for production bonuses.
        /// Unassigns from previous assignment first.
        /// </summary>
        public static bool AssignToBuilding(GameState state, VikingInstance viking, string buildingId)
        {
            if (state.buildings.GetBuilding(buildingId) == null)
            {
                Debug.Log($"[VikingSystem] Building {buildingId} not built.");
                return false;
            }

            Unassign(viking);
            viking.assignment = VikingAssignment.Building;
            viking.assignedBuildingId = buildingId;
            return true;
        }

        /// <summary>
        /// Assign a viking to the combat party.
        /// </summary>
        public static bool AssignToParty(GameState state, VikingInstance viking)
        {
            var party = state.vikings.GetParty();
            if (party.Count >= MAX_PARTY_SIZE)
            {
                Debug.Log("[VikingSystem] Party is full.");
                return false;
            }

            Unassign(viking);
            viking.assignment = VikingAssignment.Party;
            viking.assignedBuildingId = null;
            return true;
        }

        /// <summary>
        /// Remove a viking from its current assignment.
        /// </summary>
        public static void Unassign(VikingInstance viking)
        {
            viking.assignment = VikingAssignment.Idle;
            viking.assignedBuildingId = null;
        }

        /// <summary>
        /// Get base stats only (no equipment). Useful for stat screen breakdown.
        /// </summary>
        public static StatBlock GetBaseStats(VikingInstance viking, VikingDatabase db)
        {
            var data = db.GetViking(viking.vikingDataId);
            if (data == null) return new StatBlock();

            return new StatBlock
            {
                hp = data.GetStat(StatType.HP, viking.level),
                atk = data.GetStat(StatType.ATK, viking.level),
                def = data.GetStat(StatType.DEF, viking.level),
                spd = data.GetStat(StatType.SPD, viking.level)
            };
        }

        /// <summary>
        /// Get full effective stats: base + equipment.
        /// This is what the combat system should use.
        /// </summary>
        public static StatBlock GetFullStats(GameState state, VikingInstance viking,
            VikingDatabase vikDB, EquipmentDatabase eqDB)
        {
            var baseStats = GetBaseStats(viking, vikDB);

            if (eqDB == null) return baseStats;

            var eqStats = EquipmentSystem.GetEquipmentStats(state, eqDB, viking.uniqueId);
            return baseStats.Add(eqStats);
        }

        /// <summary>
        /// Calculate the total workforce production bonus for a building
        /// from all vikings assigned to it.
        /// Returns a multiplier (e.g. 0.15 = +15%).
        /// </summary>
        public static double GetWorkforceBonus(GameState state, VikingDatabase db, string buildingId)
        {
            var assigned = state.vikings.GetAssignedTo(buildingId);
            double total = 0;

            foreach (var viking in assigned)
            {
                var data = db.GetViking(viking.vikingDataId);
                if (data == null) continue;
                total += data.workforceBonus * viking.level;
            }

            return total;
        }
    }

    /// <summary>
    /// Snapshot of all four combat stats. Used by combat system and UI.
    /// </summary>
    public struct StatBlock
    {
        public int hp;
        public int atk;
        public int def;
        public int spd;

        public StatBlock Add(StatBlock other)
        {
            return new StatBlock
            {
                hp = hp + other.hp,
                atk = atk + other.atk,
                def = def + other.def,
                spd = spd + other.spd
            };
        }
    }
}
