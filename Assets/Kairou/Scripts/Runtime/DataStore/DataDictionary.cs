using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    internal class KeyVal
    {
        [SerializeField] string _key;
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        public KeyVal(string key)
        {
            Key = key;
        }
    }

    [Serializable]
    internal class KeyVal<T> : KeyVal
    {
        [SerializeField] T _value;
        public T Value
        {
            get => _value;
            set => _value = value;
        }

        public KeyVal(string name, T value) : base(name)
        {
            Value = value;
        }
    }

    [Serializable]
    internal class DataDictionary : Dictionary<string, KeyVal>, ISerializationCallbackReceiver
    {
        [SerializeField] int _version = 1;
        public int Version
        {
            get => _version;
            set => _version = value;
        }

        [SerializeReference] List<KeyVal> _dataList = new();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _dataList.Clear();
            foreach (var kvp in this)
            {
                _dataList.Add(kvp.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();
            foreach (var data in _dataList)
            {
                this[data.Key] = data;
            }
        }
    }
}