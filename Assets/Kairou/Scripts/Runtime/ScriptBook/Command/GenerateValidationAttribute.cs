using System;

namespace Kairou
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class GenerateValidationAttribute : Attribute
    {
        public bool AllowNull { get; set; }
    }
}