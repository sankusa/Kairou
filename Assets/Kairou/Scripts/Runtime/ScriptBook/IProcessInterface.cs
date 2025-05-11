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
        void SwitchPage(string pageId);
        void SwitchBook(ScriptBook book, string pageId);
        void SwitchBookAsNewSeries(ScriptBook book, string pageId);
        UniTask RunPage(string pageId, CancellationToken cancellationToken);
        UniTask RunBook(ScriptBook scriptBook, string pageId, CancellationToken cancellationToken);
        UniTask RunBookAsNewSeries(ScriptBook scriptBook, string pageId, CancellationToken cancellationToken);

        Variable FindVariable(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        Variable<T> FindVariable<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueGetter<T> FindVariableValueGetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueSetter<T> FindVariableValueSetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);
        VariableValueAccessor<T> FindVariableValueAccessor<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None);

        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
    }
}