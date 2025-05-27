using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    [CommandInfo("Flow", "SwitchBook")]
    public partial class SwitchBookCommand : Command
    {
        [SerializeField] bool _switchSeries;

        [GenerateValidation]
        [SerializeField] BookAndPageSelector _target;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            if (_switchSeries)
            {
                process.SwitchBookAsNewSeries(_target.Book, _target.PageId);
            }
            else
            {
                process.SwitchBook(_target.Book, _target.PageId);
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