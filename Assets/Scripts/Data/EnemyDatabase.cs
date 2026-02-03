using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Data
{
    /// <summary>
    /// Registry of all enemy types.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyDatabase", menuName = "IdleViking/Enemy Database")]
    public class EnemyDatabase : ScriptableObject
    {
        public List<EnemyData> enemies = new List<EnemyData>();

        private Dictionary<string, EnemyData> _lookup;

        public EnemyData GetEnemy(string enemyId)
        {
            BuildLookup();
            _lookup.TryGetValue(enemyId, out EnemyData data);
            return data;
        }

        public List<EnemyData> GetAll() => enemies;

        private void BuildLookup()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<string, EnemyData>();
            foreach (var e in enemies)
            {
                if (e != null && !string.IsNullOrEmpty(e.enemyId))
                    _lookup[e.enemyId] = e;
            }
        }

        private void OnEnable()
        {
            _lookup = null;
        }
    }
}
