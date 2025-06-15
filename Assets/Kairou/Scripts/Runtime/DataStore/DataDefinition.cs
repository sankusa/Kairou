using System;
using UnityEngine;

namespace Kairou.DataStore
{
    [Serializable]
    public class DataDefinition
    {
        [SerializeField] string _key;
        public string Key => _key;
        [SerializeField] string _typeId;
        public string TypeId => _typeId;
        [SerializeField] string _defaultValue;
        public string DefaultValue => _defaultValue;
    }
}