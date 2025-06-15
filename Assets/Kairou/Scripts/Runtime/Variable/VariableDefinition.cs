using System;
using UnityEngine;

namespace Kairou
{
    public enum DataBankLinkType
    {
        None = 0,
        DefaultStorage = 1,
        SelectStorage = 2,
    }

    [Serializable]
    public abstract class VariableDefinition
    {
        [SerializeField] string _name;
        public string Name => _name;

        [SerializeField] DataBankLinkType _dataBankLink;
        public DataBankLinkType DataBankLink => _dataBankLink;

        [SerializeField] string _storageKey;
        public string StorageKey => _storageKey;

        [SerializeField] string _dataKey;
        public string DataKey => _dataKey;

        public abstract Type TargetType { get; }

        public abstract Variable CreateVariable();
    }

    [Serializable]
    public class VariableDefinition<T> : VariableDefinition
    {
        [SerializeField] T _defaultValue;
        public T DefaultValue => _defaultValue;

        public override Type TargetType => typeof(T);

        public override Variable CreateVariable()
        {
            return Variable<T>.Rent(this);
        }
    }
}