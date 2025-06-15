using System.Collections.Generic;
using UnityEngine;

namespace Kairou.DataStore
{
    public class DataStorageMaker : MonoBehaviour
    {
        [SerializeField] string _storageId;
        [SerializeField] List<DataDefinition> _definitions = new();

        DataStorage _dataStorage;

        void Awake()
        {
            _dataStorage = new DataStorage();
            _dataStorage.ApplyDefinitions(_definitions);
            DataBank.Attach(_storageId, _dataStorage);
        }

        void OnDestroy()
        {
            DataBank.Detach(_dataStorage);
        }
    }
}