using System;
using UnityEngine;

namespace Kairou.Tests.Runtime
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message)
        {
        }
    }
}