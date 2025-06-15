using System;
using UnityEngine;

namespace Kairou.DataStore
{
        [Serializable]
        public class DataRecord
        {
            [SerializeField] string _key;
            public string Key => _key;
            [SerializeField] string _typeId;
            public string TypeId => _typeId;
            [SerializeField] string _value;
            public string Value
            {
                get => _value;
                internal set => _value = value;
            }

            public DataRecord(string key, string type, string value)
            {
                _key = key;
                _typeId = type;
                _value = value;
            }
        }
}