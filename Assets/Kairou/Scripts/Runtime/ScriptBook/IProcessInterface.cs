using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public interface IProcessInterface
    {
        int NextCommandIndex { get; }
        void GoToIndex(int commandIndex);
        void GoToEnd();

        void PushBlock(Block block);
        Block PopBlock();
        Block PeekBlock();
        bool TryPopBlock<TBlock>(out TBlock block) where TBlock : Block;

        void SwitchPage(string pageId);
        void SwitchBook(ScriptBook book, string pageId);
        void SwitchBookAsNewSeries(ScriptBook book, string pageId);
        UniTask RunPageAsync(string pageId, CancellationToken cancellationToken);
        UniTask RunBookAsync(ScriptBook book, string pageId, CancellationToken cancellationToken);
        UniTask RunBookAsNewSeriesAsync(ScriptBook book, string pageId, CancellationToken cancellationToken);

        Variable FindVariable(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        Variable<T> FindVariable<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueGetter<T> FindVariableValueGetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueSetter<T> FindVariableValueSetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueAccessor<T> FindVariableValueAccessor<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);

        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
    }
}