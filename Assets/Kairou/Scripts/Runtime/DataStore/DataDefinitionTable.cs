using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kairou.DataStore
{
    [CreateAssetMenu(fileName = nameof(DataDefinitionTable), menuName = nameof(Kairou) + "/" + nameof(DataDefinitionTable))]
    public class DataDefinitionTable : ScriptableObject
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SetUpDefaultStorage()
        {
            var table = Resources.Load<DataDefinitionTable>(nameof(DataDefinitionTable));
            if (table == null) return;
            var storage = new DataStorage();
            storage.ApplyDefinitions(table._definitions);
            DataBank.Attach("Default", storage, true);
        }

        [SerializeField] List<DataDefinition> _definitions = new();
    }
}