using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public class ProcessInterface : IProcessInterface
    {
        readonly PageProcess _pageProcess;

        internal ProcessInterface(PageProcess pageProcess)
        {
            _pageProcess = pageProcess;
        }

        public int NextCommandIndex => _pageProcess.NextCommandIndex;
        public void GoToIndex(int commandIndex) => _pageProcess.GoToIndex(commandIndex);
        public void GoToEnd() => _pageProcess.GoToEnd();

        public void Pause() => _pageProcess.Pause();
        public void Resume() => _pageProcess.Resume();

        public void PushBlock(Block block) => _pageProcess.PushBlock(block);
        public Block PopBlock() => _pageProcess.PopBlock();
        public Block PeekBlock() => _pageProcess.PeekBlock();
        public bool TryPopBlock<TBlock>(out TBlock block) where TBlock : Block => _pageProcess.TryPopBlock(out block);

        public void SwitchPage(string pageId)
        {
            _pageProcess.GoToEnd();
            _pageProcess.SubsequentProcessInfo = new SubsequentProcessInfo(pageId);
        }
        public void SwitchBook(ScriptBook book, string pageId)
        {
            _pageProcess.GoToEnd();
            _pageProcess.SubsequentProcessInfo = new SubsequentProcessInfo(book, pageId);
        }
        public void SwitchBookAsNewSeries(ScriptBook book, string pageId)
        {
            _pageProcess.GoToEnd();
            _pageProcess.SubsequentProcessInfo = new SubsequentProcessInfo(true, book, pageId);
        }
        public async UniTask RunPageAsync(string pageId, CancellationToken cancellationToken)
        {
            await ProcessRunner.RunPageAsBookProcessSubSequenceAsync(_pageProcess, pageId, cancellationToken);
        }
        public async UniTask RunBookAsync(ScriptBook book, string pageId, CancellationToken cancellationToken)
        {
            await ProcessRunner.RunBookAsSeriesProcessSubSequenceAsync(_pageProcess, book, pageId, cancellationToken);
        }
        public async UniTask RunBookAsNewSeriesAsync(ScriptBook book, string pageId, CancellationToken cancellationToken)
        {
            await ProcessRunner.RunBookAsRootProcessSubSequenceAsync(_pageProcess, book, pageId, cancellationToken);
        }

        public Variable FindVariable(string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return FindVariableInternal(_pageProcess, name, targetScope, static variable => true);
        }

        public Variable<T> FindVariable<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return FindVariableInternal(_pageProcess, name, targetScope, static variable => variable is Variable<T>) as Variable<T>;
        }

        public VariableValueGetter<T> FindVariableValueGetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return new VariableValueGetter<T>(FindVariableInternal(_pageProcess, name, targetScope, static variable => variable.CanConvertTo<T>()));
        }

        public VariableValueSetter<T> FindVariableValueSetter<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return new VariableValueSetter<T>(FindVariableInternal(_pageProcess, name, targetScope, static variable => variable.CanConvertFrom<T>()));
        }

        public VariableValueAccessor<T> FindVariableValueAccessor<T>(string name, TargetVariableScope targetScope = TargetVariableScope.None)
        {
            return new VariableValueAccessor<T>(FindVariableInternal(_pageProcess, name, targetScope, static variable => variable.CanMutuallyConvert<T>()));
        }

        static Variable FindVariableInternal(PageProcess process, string name, TargetVariableScope targetScope, Func<Variable, bool> filter)
        {
            Variable variable;
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Page)
            {
                process.Variables.TryGetValue(name, out variable);
                if (variable != null && filter.Invoke(variable)) return variable;
            }
            if (targetScope == TargetVariableScope.None || targetScope == TargetVariableScope.Book)
            {
                process.BookProcess.Variables.TryGetValue(name, out variable);
                if (variable != null && filter.Invoke(variable)) return variable;
            }
            return null;
        }

        public T Resolve<T>()
        {
            return _pageProcess.BookProcess.SeriesProcess.RootProcess.ObjectResolver.Resolve<T>();
        }

        public bool TryResolve<T>(out T value)
        {
            return _pageProcess.BookProcess.SeriesProcess.RootProcess.ObjectResolver.TryResolve(out value);
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return _pageProcess.BookProcess.SeriesProcess.RootProcess.ObjectResolver.ResolveAll<T>();
        }
    }
}