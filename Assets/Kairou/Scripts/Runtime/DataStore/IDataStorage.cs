using System.Collections.Generic;
using UnityEngine;

namespace Kairou.DataStore
{
    public interface IDataStorage
    {
        T Get<T>(string key, bool validateTypeId = true);
        void Set<T>(string key, T value, bool validateTypeId = true);
        bool HasKey(string key);
        bool ValidateTypeId<T>(string key);
        bool HasKeyAndMatchedTypeId<T>(string key);
        bool Delete<T>(string key, bool validateTypeId = true);
        bool Delete(string key);
        bool TryGet<T>(string key, out T value, bool validateTypeId = true);
    }

    public interface IDataRecordsProvider
    {
        IEnumerable<DataRecord> GetRecords();
    }
}