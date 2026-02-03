using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleViking.Models
{
    /// <summary>
    /// Dictionary wrapper that Unity's JsonUtility can serialize.
    /// Converts to parallel key/value lists for JSON, restores on deserialize.
    /// </summary>
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            int count = Math.Min(keys.Count, values.Count);
            for (int i = 0; i < count; i++)
            {
                this[keys[i]] = values[i];
            }
        }
    }
}
