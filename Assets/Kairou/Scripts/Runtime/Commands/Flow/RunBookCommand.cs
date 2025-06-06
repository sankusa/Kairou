using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kairou
{
    public partial class RunBookCommand : AsyncCommand
    {
        [SerializeField] bool _switchSeries;

        [GenerateValidation]
        [SerializeField] BookAndPageSelector _target;

        [CommandExecute]
        UniTask ExecuteAsync(IProcessInterface process, CancellationToken cancellationToken)
        {
            if (_switchSeries)
            {
                return process.RunBookAsNewSeriesAsync(_target.Book, _target.PageId, cancellationToken);
            }
            else
            {
                return process.RunBookAsync(_target.Book, _target.PageId, cancellationToken);
            }
        }

        public override string GetSummary()
        {
            return (_switchSeries ? "(Switch Series) " : "") + _target.GetSummary();
        }

        public override IEnumerable<ScriptBook> GetReferencingBooks()
        {
            if (_target.Book != null)
            {
                yield return _target.Book;
            }
        }

        public override void GetPreloadTargetBooks(ICollection<ScriptBook> books)
        {
            if (_target.Book == null) return;
            books.Add(_target.Book);
        }
    }
}