using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NovelNodePlayer.Enums;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    public class BlackboardData : ScriptableObject
    {
        public string Name = string.Empty;
        public int ID = 0;
        public List<KeyValue> Strings = new();
        public List<KeyValue> Floats = new();
        public List<KeyValue> Booleans = new();

        private Dictionary<(ValueType Type, string Key), KeyValue> cache = new();

        public void PopulateCache()
        {
            foreach (var kv in Strings)
                cache[(kv.Type, kv.Key)] = kv;
            foreach (var kv in Floats)
                cache[(kv.Type, kv.Key)] = kv;
            foreach (var kv in Booleans)
                cache[(kv.Type, kv.Key)] = kv;
        }


        public KeyValue GetValueFromCacheOrSearch(ValueType type, string key)
        {
            if (cache.TryGetValue((type, key), out var cachedValue))
                return cachedValue;

            var value = Strings.FirstOrDefault(x => x.Type == type && x.Key == key)
                     ?? Floats.FirstOrDefault(x => x.Type == type && x.Key == key)
                     ?? Booleans.FirstOrDefault(x => x.Type == type && x.Key == key);

            cache[(type, key)] = value;
            return value;
        }
    }

    [System.Serializable]
    public class KeyValue
    {
        public string Key;
        public ValueData Value = new();
        public ValueType Type;

        [System.Serializable]
        public class ValueData
        {
            public string String = string.Empty;
            public float Float = 0f;
            public bool Boolean = false;
            [JsonIgnore]
            public Sprite Sprite = null;
        }
    }
}