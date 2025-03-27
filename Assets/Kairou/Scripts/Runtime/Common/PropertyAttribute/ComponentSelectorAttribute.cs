using System;
using UnityEngine;

namespace Kairou
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class ComponentSelectorAttribute : PropertyAttribute
    {
        public ComponentSelectorAttribute() {}
    }
}