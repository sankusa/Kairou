using UnityEngine;

namespace Kairou
{
    public interface IDataStore
    {
        static IDataStore _instance;
        public static IDataStore Instance => _instance ??= new DataStore();

        void SetValue<T>(string key, T value);
        T GetValue<T>(string key, T defaultValue = default);
        bool HasKey(string key);
        bool TryGetValue<T>(string key, out T value);
        void RemoveKey(string key);
        void Clear();
        void FromText(string text);
        string ToText();
    }
}