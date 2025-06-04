using System;
using UnityEngine;

namespace Kairou.Editor
{
    [Serializable]
    public class CommandSetting
    {
        [SerializeField] string _typeFullName;
        Type _type;
        public Type Type
        {
            get
            {
                if (_type == null || _type.FullName != _typeFullName)
                {
                    _type = CommandTypeCache.Find(_typeFullName);
                }
                return _type;
            }
        }

        [SerializeField] string _category;
        public string Category => _category;
    }
}