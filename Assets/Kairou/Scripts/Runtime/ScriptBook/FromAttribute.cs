using System;

namespace Kairou
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class FromAttribute : Attribute
    {
        public FromAttribute(string fieldName) {}
    }
}