using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    internal static class UniTaskExtensions
    {
        public static void ForgetWithLogException(this UniTask uniTask, string headerMessage = null)
        {
            uniTask.Forget(e =>
            {
                if(e is OperationCanceledException) return;
                Debug.LogError(string.IsNullOrEmpty(headerMessage) ? e.ToString() : headerMessage + "\n" + e.ToString());
            });
        }
    }
}