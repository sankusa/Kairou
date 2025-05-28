using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace Kairou
{
    public static class WarmupRunner
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void WarmUp()
        {
            var book = ScriptBook.CreateEmptyBook();
            var page = new Page();
            page.AddCommand(new WarmupCommand());
            book.AddPage(page);
            
            ProcessRunner.RunMainSequenceAsync(
                RootProcess.Rent(),
                book,
                static rootProcess => RootProcess.Return(rootProcess),
                default(CancellationToken))
            .Forget();
        }
    }
}