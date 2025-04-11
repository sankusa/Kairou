using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public class DataStore : IDataStore
    {
        [SerializeField] DataDictionary _dataDic = new();
        internal DataDictionary DataDic => _dataDic;

        public void SetValue<T>(string key, T value)
        {
            if (_dataDic.TryGetValue(key, out var data) == false)
            {
                _dataDic[key] = new KeyVal<T>(key, value);
                return;
            }
            
            if (data is not KeyVal<T> typed)
                throw new InvalidOperationException($"Key '{key}' already exists with a different type.");

            typed.Value = value;
            return;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_dataDic.TryGetValue(key, out var data) == false) return defaultValue;
            if (data is not KeyVal<T> typed)
                throw new InvalidOperationException($"Key '{key}' already exists with a different type.");

            return typed.Value;
        }

        public bool HasKey(string key)
        {
            return _dataDic.ContainsKey(key);
        }

        public bool TryGetValue<T>(string key, out T value)
        {
            if (_dataDic.TryGetValue(key, out var data) == false)
            {
                value = default;
                return false;
            }
            if (data is not KeyVal<T> typed) throw new InvalidOperationException($"Key '{key}' already exists with a different type.");

            value = typed.Value;
            return true;
        }

        public void RemoveKey(string key)
        {
            _dataDic.Remove(key);
        }

        public void Clear()
        {
            _dataDic.Clear();
        }

        public void FromText(string text)
        {
            if (text.StartsWith("{\"_version\":1"))
            {
                JsonUtility.FromJsonOverwrite(text, _dataDic);
            }
            else
            {
                throw new InvalidOperationException($"Invalid text format: {text}");
            }
        }

        public string ToText()
        {
            return JsonUtility.ToJson(_dataDic);
        }
    }
}