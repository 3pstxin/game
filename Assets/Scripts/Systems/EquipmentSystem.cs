using UnityEngine;
using IdleViking.Data;
using IdleViking.Models;

namespace IdleViking.Systems
{
    /// <summary>
    /// Handles equipment crafting, equipping, unequipping, selling,
    /// and stat bonus calculation.
    /// </summary>
    public static class EquipmentSystem
    {
        /// <summary>
        /// Craft an equipment item. Spends resources, creates an instance with random rolls,
        /// adds it to inventory. Returns the instance or null on failure.
        /// </summary>
        public static EquipmentInstance TryCraft(GameState state, EquipmentDatabase db, string equipmentDataId)
        {
            var data = db.GetEquipment(equipmentDataId);
            if (data == null)
            {
                Debug.LogWarning($"[EquipmentSystem] Unknown equipment: {equipmentDataId}");
                return null;
            }

            if (!data.IsCraftable)
            {
                Debug.Log($"[EquipmentSystem] {data.displayName} is not craftable.");
                return null;
            }

            if (state.inventory.IsBagFull())
            {
                Debug.Log("[EquipmentSystem] Inventory is full.");
                return null;
            }

            if (!state.resources.Spend(data.craftCosts))
            {
                Debug.Log($"[EquipmentSystem] Can't afford to craft {data.displayName}.");
                return null;
            }

            var instance = CreateInstance(data);
            state.inventory.AddItem(instance);

            Debug.Log($"[EquipmentSystem] Crafted {data.displayName} (#{instance.itemId})");
            return instance;
        }

        /// <summary>
        /// Create an equipment instance with random bonus stat rolls.
        /// Used by crafting and loot systems.
        /// </summary>
        public static EquipmentInstance CreateInstance(EquipmentData data)
        {
            var instance = new EquipmentInstance
            {
                equipmentDataId = data.equipmentId
            };

            // Roll random bonus stats
            int rolls = data.GetRollCount();
            for (int i = 0; i < rolls; i++)
            {
                int stat = Random.Range(0, 4);
                switch (stat)
                {
                    case 0: instance.rolledHP++; break;
                    case 1: instance.rolledATK++; break;
                    case 2: instance.rolledDEF++; break;
                    case 3: instance.rolledSPD++; break;
                }
            }

            return instance;
        }

        /// <summary>
        /// Equip an item on a viking. Auto-unequips any item already in that slot.
        /// The item must be in the inventory and not equipped by another viking.
        /// </summary>
        public static bool TryEquip(GameState state, EquipmentDatabase db, int vikingUniqueId, int itemId)
        {
            var item = state.inventory.GetItem(itemId);
            if (item == null)
            {
                Debug.Log("[EquipmentSystem] Item not found.");
                return false;
            }

            var data = db.GetEquipment(item.equipmentDataId);
            if (data == null) return false;

            // Check item isn't equipped by another viking
            foreach (var loadout in state.inventory.loadouts)
            {
                if (loadout.vikingUniqueId == vikingUniqueId) continue;
                if (loadout.GetSlot(data.slot) == itemId)
                {
                    Debug.Log("[EquipmentSystem] Item is equipped by another viking.");
                    return false;
                }
            }

            var targetLoadout = state.inventory.GetOrCreateLoadout(vikingUniqueId);

            // Unequip current item in that slot (goes back to bag)
            int currentItemId = targetLoadout.GetSlot(data.slot);
            if (currentItemId >= 0)
                targetLoadout.ClearSlot(data.slot);

            targetLoadout.SetSlot(data.slot, itemId);

            Debug.Log($"[EquipmentSystem] Equipped {data.displayName} on viking #{vikingUniqueId}");
            return true;
        }

        /// <summary>
        /// Unequip an item from a viking's slot. Item goes back to bag.
        /// </summary>
        public static bool TryUnequip(GameState state, int vikingUniqueId, EquipmentSlot slot)
        {
            var loadout = state.inventory.GetLoadout(vikingUniqueId);
            if (loadout == null) return false;

            int itemId = loadout.GetSlot(slot);
            if (itemId < 0) return false;

            loadout.ClearSlot(slot);
            Debug.Log($"[EquipmentSystem] Unequipped slot {slot} from viking #{vikingUniqueId}");
            return true;
        }

        /// <summary>
        /// Sell an unequipped item for resources. Removes it from inventory.
        /// </summary>
        public static bool TrySell(GameState state, EquipmentDatabase db, int itemId)
        {
            var item = state.inventory.GetItem(itemId);
            if (item == null) return false;

            // Prevent selling equipped items
            foreach (var loadout in state.inventory.loadouts)
            {
                var equipped = new System.Collections.Generic.HashSet<int>();
                loadout.CollectEquippedIds(equipped);
                if (equipped.Contains(itemId))
                {
                    Debug.Log("[EquipmentSystem] Can't sell an equipped item.");
                    return false;
                }
            }

            var data = db.GetEquipment(item.equipmentDataId);
            if (data != null)
                state.resources.Add(data.sellResourceType, data.sellValue);

            state.inventory.RemoveItem(itemId);
            Debug.Log($"[EquipmentSystem] Sold item #{itemId}");
            return true;
        }

        /// <summary>
        /// Get the total equipment stat bonus for a viking across all equipped slots.
        /// Combines base stats from EquipmentData + rolled bonus stats.
        /// </summary>
        public static StatBlock GetEquipmentStats(GameState state, EquipmentDatabase db, int vikingUniqueId)
        {
            var stats = new StatBlock();
            var loadout = state.inventory.GetLoadout(vikingUniqueId);
            if (loadout == null) return stats;

            AddSlotStats(ref stats, state, db, loadout, EquipmentSlot.Weapon);
            AddSlotStats(ref stats, state, db, loadout, EquipmentSlot.Armor);
            AddSlotStats(ref stats, state, db, loadout, EquipmentSlot.Helmet);
            AddSlotStats(ref stats, state, db, loadout, EquipmentSlot.Accessory);

            return stats;
        }

        private static void AddSlotStats(ref StatBlock stats, GameState state,
            EquipmentDatabase db, VikingLoadout loadout, EquipmentSlot slot)
        {
            int itemId = loadout.GetSlot(slot);
            if (itemId < 0) return;

            var item = state.inventory.GetItem(itemId);
            if (item == null) return;

            var data = db.GetEquipment(item.equipmentDataId);
            if (data == null) return;

            stats.hp += data.bonusHP + item.rolledHP;
            stats.atk += data.bonusATK + item.rolledATK;
            stats.def += data.bonusDEF + item.rolledDEF;
            stats.spd += data.bonusSPD + item.rolledSPD;
        }
    }
}
