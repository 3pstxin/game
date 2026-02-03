using System;
using System.Collections.Generic;

namespace IdleViking.Models
{
    /// <summary>
    /// Runtime state for the player's equipment inventory and viking loadouts.
    /// </summary>
    [Serializable]
    public class InventoryState
    {
        public int maxCapacity = 50;
        public int nextItemId = 1;

        /// <summary>
        /// All owned equipment items (both unequipped bag items and equipped ones).
        /// </summary>
        public List<EquipmentInstance> items = new List<EquipmentInstance>();

        /// <summary>
        /// Per-viking equipment loadouts. Key = viking uniqueId.
        /// </summary>
        public List<VikingLoadout> loadouts = new List<VikingLoadout>();

        public EquipmentInstance GetItem(int itemId)
        {
            return items.Find(i => i.itemId == itemId);
        }

        /// <summary>
        /// Get all items not currently equipped by any viking.
        /// </summary>
        public List<EquipmentInstance> GetUnequipped()
        {
            var equipped = new HashSet<int>();
            foreach (var loadout in loadouts)
                loadout.CollectEquippedIds(equipped);

            return items.FindAll(i => !equipped.Contains(i.itemId));
        }

        /// <summary>
        /// Number of unequipped items (counts toward capacity).
        /// </summary>
        public int GetBagCount()
        {
            return GetUnequipped().Count;
        }

        public bool IsBagFull()
        {
            return GetBagCount() >= maxCapacity;
        }

        public VikingLoadout GetLoadout(int vikingUniqueId)
        {
            return loadouts.Find(l => l.vikingUniqueId == vikingUniqueId);
        }

        public VikingLoadout GetOrCreateLoadout(int vikingUniqueId)
        {
            var existing = GetLoadout(vikingUniqueId);
            if (existing != null) return existing;

            var loadout = new VikingLoadout { vikingUniqueId = vikingUniqueId };
            loadouts.Add(loadout);
            return loadout;
        }

        /// <summary>
        /// Add a new item to the inventory. Returns the instance.
        /// </summary>
        public EquipmentInstance AddItem(EquipmentInstance item)
        {
            item.itemId = nextItemId++;
            items.Add(item);
            return item;
        }

        public void RemoveItem(int itemId)
        {
            items.RemoveAll(i => i.itemId == itemId);
        }
    }

    /// <summary>
    /// A specific equipment item instance with rolled bonus stats.
    /// </summary>
    [Serializable]
    public class EquipmentInstance
    {
        public int itemId;
        public string equipmentDataId;

        // Rolled bonus stats (on top of base stats from EquipmentData)
        public int rolledHP;
        public int rolledATK;
        public int rolledDEF;
        public int rolledSPD;

        public int GetRolledStat(StatType stat)
        {
            switch (stat)
            {
                case StatType.HP: return rolledHP;
                case StatType.ATK: return rolledATK;
                case StatType.DEF: return rolledDEF;
                case StatType.SPD: return rolledSPD;
                default: return 0;
            }
        }
    }

    /// <summary>
    /// Equipment loadout for a single viking. One item per slot.
    /// Stores item IDs (not references) for serialization.
    /// </summary>
    [Serializable]
    public class VikingLoadout
    {
        public int vikingUniqueId;

        // -1 means empty slot
        public int weaponItemId = -1;
        public int armorItemId = -1;
        public int helmetItemId = -1;
        public int accessoryItemId = -1;

        public int GetSlot(EquipmentSlot slot)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: return weaponItemId;
                case EquipmentSlot.Armor: return armorItemId;
                case EquipmentSlot.Helmet: return helmetItemId;
                case EquipmentSlot.Accessory: return accessoryItemId;
                default: return -1;
            }
        }

        public void SetSlot(EquipmentSlot slot, int itemId)
        {
            switch (slot)
            {
                case EquipmentSlot.Weapon: weaponItemId = itemId; break;
                case EquipmentSlot.Armor: armorItemId = itemId; break;
                case EquipmentSlot.Helmet: helmetItemId = itemId; break;
                case EquipmentSlot.Accessory: accessoryItemId = itemId; break;
            }
        }

        public void ClearSlot(EquipmentSlot slot)
        {
            SetSlot(slot, -1);
        }

        /// <summary>
        /// Collect all equipped item IDs into a set.
        /// </summary>
        public void CollectEquippedIds(HashSet<int> ids)
        {
            if (weaponItemId >= 0) ids.Add(weaponItemId);
            if (armorItemId >= 0) ids.Add(armorItemId);
            if (helmetItemId >= 0) ids.Add(helmetItemId);
            if (accessoryItemId >= 0) ids.Add(accessoryItemId);
        }
    }
}
