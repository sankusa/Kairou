using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public class ScriptBookRunner
    {
        public async UniTask RunAsync(ScriptBook scriptBook, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RootProcess rootProcess = RootProcess.Rent();
            rootProcess.AddScriptBookProcess(scriptBook);
            try
            {
                await rootProcess.StartAsync(cancellationToken);
            }
            finally
            {
                RootProcess.Return(rootProcess);
            }
        }
    }
}