using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    internal static class UniTaskExtensions
    {
        public static void ForgetWithLogException(this UniTask uniTask)
        {
            uniTask.Forget(e =>
            {
                if(e is OperationCanceledException) return;
                Debug.LogException(e);
            });
        }
    }
}