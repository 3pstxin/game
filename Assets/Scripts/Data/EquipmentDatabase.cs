using System.Collections.Generic;
using UnityEngine;
using IdleViking.Models;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all equipment templates.
    /// </summary>
    [CreateAssetMenu(fileName = "EquipmentDatabase", menuName = "IdleViking/Equipment Database")]
    public class EquipmentDatabase : ScriptableObject
    {
        public List<EquipmentData> equipment = new List<EquipmentData>();

        private Dictionary<string, EquipmentData> _lookup;

        public EquipmentData GetEquipment(string equipmentId)
        {
            BuildLookup();
            _lookup.TryGetValue(equipmentId, out EquipmentData data);
            return data;
        }

        /// <summary>
        /// Get all craftable equipment of a given rarity or lower.
        /// </summary>
        public List<EquipmentData> GetCraftable(Rarity maxRarity = Rarity.Legendary)
        {
            return equipment.FindAll(e => e.IsCraftable && e.rarity <= maxRarity);
        }

        /// <summary>
        /// Get all equipment for a specific slot.
        /// </summary>
        public List<EquipmentData> GetBySlot(EquipmentSlot slot)
        {
            return equipment.FindAll(e => e.slot == slot);
        }

        public List<EquipmentData> GetAll() => equipment;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, EquipmentData>();
            foreach (var e in equipment)
            {
                if (e != null && !string.IsNullOrEmpty(e.equipmentId))
                    _lookup[e.equipmentId] = e;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
