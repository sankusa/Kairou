using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public class PageProcess
    {
        static readonly ObjectPool<PageProcess> _pool = new(
            createFunc: () => new PageProcess(),
            onRent: process =>
            {

            },
            onReturn: process =>
            {
                process.Clear();
            }
        );

        public static PageProcess Rent() => _pool.Rent();
        public static void Return(PageProcess process) => _pool.Return(process);

        public ScriptBookProcess BookProcess { get; private set; }

        Page _page;

        CancellationTokenSource _cts;

        bool _isStarted;

        int _currentCommandIndex;
        public int NextCommandIndex { get; set; }

        private PageProcess() {}

        public void SetUp(ScriptBookProcess parentProcess, Page page)
        {
            if (parentProcess == null) throw new ArgumentNullException(nameof(parentProcess));
            if (page == null) throw new ArgumentNullException(nameof(page));

            BookProcess = parentProcess;
            _page = page;
            _cts = new();
        }

        internal async UniTask StartAsync(CancellationToken cancellationToken)
        {
            if (_isStarted) throw new InvalidOperationException($"{nameof(PageProcess)} is already started.");
            _isStarted = true;

            cancellationToken.ThrowIfCancellationRequested();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_cts.Token, cancellationToken);

            while (_page.Commands.HasElementAt(NextCommandIndex))
            {
                _currentCommandIndex = NextCommandIndex;
                NextCommandIndex = _currentCommandIndex + 1;

                Command command = _page.Commands[_currentCommandIndex];
                try
                {
                    linkedCts.Token.ThrowIfCancellationRequested();
                    if (command is AsyncCommand asyncCommand)
                    {
                        await asyncCommand.ExecuteAsync(this, linkedCts.Token);
                    }
                    else
                    {
                        command.Execute(this);
                    }
                }
                catch (OperationCanceledException e) when (e.CancellationToken == linkedCts.Token)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(e.Message, e, cancellationToken);
                    }
                    else if (_cts.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(e.Message, e, _cts.Token);
                    }
                    throw;
                }
                catch (OperationCanceledException)
                {
                    // Command内部の事情によりキャンセルされた場合は握り潰して処理を続行
                }
            }
        }

        public void Cancel() => _cts.Cancel();

        void Clear()
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;

            BookProcess = null;
            _page = null;

            _isStarted = false;

            _currentCommandIndex = 0;
            NextCommandIndex = 0;
        }
    }
}