using System;
using UnityEngine;

namespace Kairou
{
    [Serializable]
    public abstract class VariableKey {
        [SerializeField] string _name;
        public string Name => _name;
    }

    [Serializable]
    public class VariableKey<T> : VariableKey {}
}