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
        /// <summary>
        /// Switch book. If pageId is null or empty, run entry page.
        /// </summary>
        void SwitchBook(ScriptBook book, string pageId);
        /// <summary>
        /// Switch book as new series. If pageId is null or empty, run entry page.
        /// </summary>
        void SwitchBookAsNewSeries(ScriptBook book, string pageId);
        UniTask RunPageAsync(string pageId, CancellationToken cancellationToken);
        /// <summary>
        /// Run book. If pageId is null or empty, run entry page.
        /// </summary>
        UniTask RunBookAsync(ScriptBook book, string pageId, CancellationToken cancellationToken);
        /// <summary>
        /// Run book as new series. If pageId is null or empty, run entry page.
        /// </summary>
        UniTask RunBookAsNewSeriesAsync(ScriptBook book, string pageId, CancellationToken cancellationToken);

        Variable FindVariable(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        Variable<T> FindVariable<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueGetter<T> FindVariableValueGetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueSetter<T> FindVariableValueSetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueAccessor<T> FindVariableValueAccessor<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);

        T Resolve<T>();
        bool TryResolve<T>(out T value);
        IEnumerable<T> ResolveAll<T>();
    }
}