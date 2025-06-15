using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou.DataStore
{
    [Serializable]
    public class DataStorage : IDataStorage, IDataRecordsProvider, ISerializationCallbackReceiver
    {
        [SerializeField] List<DataRecord> _data = new();
        Dictionary<string, DataRecord> _dic = new();

        public IEnumerable<DataRecord> GetRecords() => _dic.Values;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _data.Clear();
            foreach (var kvp in _dic)
            {
                _data.Add(kvp.Value);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _dic.Clear();
            foreach (var record in _data)
            {
                _dic[record.Key] = record;
            }
        }

        public T Get<T>(string key, bool validateTypeId)
        {
            var type = DataTypeCache<T>.Type;
            var record = _dic[key];
            if (validateTypeId && record.TypeId != type.TypeId) throw new InvalidOperationException(MakeTypeMismatchErrorMessage(type.TypeId, record.TypeId));
            return type.FromString(record.Value);
        }

        public void Set<T>(string key, T value, bool validateTypeId = true)
        {
            var type = DataTypeCache<T>.Type;
            if (validateTypeId) ThrowIfHasKeyAndTypeIdMismatched(key, type);
            _dic[key] = new DataRecord(key, type.TypeId, type.ToString(value));
        }

        public bool HasKey(string key)
        {
            return _dic.ContainsKey(key);
        }

        public bool ValidateTypeId<T>(string key)
        {
            return _dic[key].TypeId == DataTypeCache<T>.Type.TypeId;
        }

        public bool HasKeyAndMatchedTypeId<T>(string key)
        {
            return _dic.TryGetValue(key, out var record) && record.TypeId == DataTypeCache<T>.Type.TypeId;
        }

        public bool Delete<T>(string key, bool validateTypeId = true)
        {
            if (validateTypeId) ThrowIfHasKeyAndTypeIdMismatched(key, DataTypeCache<T>.Type);
            return _dic.Remove(key);
        }

        public bool Delete(string key)
        {
            return _dic.Remove(key);
        }

        public bool TryGet<T>(string key, out T value, bool validateTypeId = true)
        {
            var type = DataTypeCache<T>.Type;
            bool result = _dic.TryGetValue(key, out var record);
            if (result == false)
            {
                value = default;
                return false;
            }
            if (validateTypeId && record.TypeId != type.TypeId) throw new InvalidOperationException(MakeTypeMismatchErrorMessage(type.TypeId, record.TypeId));
            value = type.FromString(record.Value);
            return true;
        }

        void ThrowIfHasKeyAndTypeIdMismatched(string key, DataType type)
        {
            if (_dic.TryGetValue(key, out var record) && record.TypeId != type.TypeId)
            {
                throw new InvalidOperationException(MakeTypeMismatchErrorMessage(type.TypeId, record.TypeId));
            }
        }
        static string MakeTypeMismatchErrorMessage(string expected, string actual) => $"TypeId mismatch. Expected: {expected}, Actual: {actual}.";

        public void ApplyDefinitions(IReadOnlyList<DataDefinition> definitions, bool allowTypeIdChange = false, bool deleteExtraKeys = false)
        {
            var newDic = new Dictionary<string, DataRecord>(definitions.Count);
            for (int i = 0; i < definitions.Count; i++)
            {
                var definition = definitions[i];
                if (newDic.ContainsKey(definition.Key)) throw new ArgumentException($"Key '{definition.Key}' already exists.");
                newDic[definition.Key] = new DataRecord(definition.Key, definition.TypeId, definition.DefaultValue);
            }

            foreach (var kvp in _dic)
            {
                if (newDic.TryGetValue(kvp.Key, out var newDicRecord))
                {
                    if (allowTypeIdChange == false && newDicRecord.TypeId != kvp.Value.TypeId) throw new InvalidOperationException(MakeTypeMismatchErrorMessage(newDicRecord.TypeId, kvp.Value.TypeId));
                    var type = DataTypeCache.Get(newDicRecord.TypeId);
                    newDicRecord.Value = type.Normalize(kvp.Value.Value);
                }
                else if (deleteExtraKeys == false)
                {
                    newDic[kvp.Key] = kvp.Value;
                }
            }
            _dic = newDic;
        }
    }
}