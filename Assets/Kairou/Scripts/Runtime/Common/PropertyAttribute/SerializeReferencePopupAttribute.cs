using System;
using UnityEngine;

namespace Kairou
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class SerializeReferencePopupAttribute : PropertyAttribute
    {
        public SerializeReferencePopupAttribute() {}
    }
}