using System;
using UnityEngine;

namespace Kairou
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HiddenCommandAttribute : Attribute {}
}