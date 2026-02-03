using System;
using System.Collections.Generic;

namespace IdleViking.Models
{
    /// <summary>
    /// Runtime state for all resource producers the player owns.
    /// </summary>
    [Serializable]
    public class ProducerState
    {
        public List<ProducerInstance> producers = new List<ProducerInstance>();

        public ProducerInstance GetProducer(string producerId)
        {
            return producers.Find(p => p.producerId == producerId);
        }

        public ProducerInstance AddProducer(string producerId)
        {
            var existing = GetProducer(producerId);
            if (existing != null) return existing;

            var instance = new ProducerInstance { producerId = producerId, level = 1 };
            producers.Add(instance);
            return instance;
        }
    }

    [Serializable]
    public class ProducerInstance
    {
        public string producerId;
        public int level = 1;
    }
}
