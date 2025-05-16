using System;

namespace Kairou
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandExecuteAttribute : Attribute {}
}