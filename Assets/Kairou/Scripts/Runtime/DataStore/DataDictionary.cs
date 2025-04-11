using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    internal class DataDictionary : Dictionary<string, DataDictionary.Data>, ISerializationCallbackReceiver
    {
        [Serializable]
        internal class Data
        {
            [SerializeField] string _name;
            public string Name
            {
                get => _name;
                set => _name = value;
            }

            public Data(string name)
            {
                Name = name;
            }
        }

        [Serializable]
        internal class Data<T> : Data
        {
            [SerializeField] T _value;
            public T Value
            {
                get => _value;
                set => _value = value;
            }

            public Data(string name, T value) : base(name)
            {
                Value = value;
            }
        }

        [SerializeField] int _version = 1;
        public int Version
        {
            get => _version;
            set => _version = value;
        }

        [SerializeReference] List<Data> _dataList = new();

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
                this[data.Name] = data;
            }
        }
    }
}