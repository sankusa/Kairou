using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [Serializable]
        private class KeyValue
        {
            [SerializeField] TKey _key;
            public TKey Key => _key;
            [SerializeField] TValue _value;
            public TValue Value => _value;

            public KeyValue(TKey key, TValue value)
            {
                _key = key;
                _value = value;
            }
        }

        [SerializeField] List<KeyValue> _keys = new();

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            foreach (var (key, value) in this)
            {
                _keys.Add(new KeyValue(key, value));
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            foreach (var keyValue in _keys)
            {
                this[keyValue.Key] = keyValue.Value;
            }
        }
    }

    [Serializable]
    public class ReadonlySerializableDictionary<TKey, TValue> : ReadOnlyDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        readonly Func<TValue, TKey> _keyGenerator;
        [SerializeField] List<TValue> _values = new();

        public ReadonlySerializableDictionary(Func<TValue, TKey> keyGenerator) : base(new Dictionary<TKey, TValue>())
        {
            _keyGenerator = keyGenerator;
        }

        public void OnBeforeSerialize() {}

        public void OnAfterDeserialize()
        {
            Dictionary.Clear();
            foreach (var value in _values)
            {
                var key = _keyGenerator(value);
                if (key == null) continue;
                Dictionary[key] = value;
            }
        }
    }
}