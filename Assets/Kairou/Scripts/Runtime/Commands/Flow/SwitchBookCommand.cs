using System.Collections.Generic;
using UnityEngine;

namespace Kairou
{
    public partial class SwitchBookCommand : Command
    {
        [GenerateValidation]
        [SerializeField] BookAndPageSelector _target;

        [SerializeField] bool _chainPreload = true;

        [CommandExecute]
        void Execute(IProcessInterface process)
        {
            process.SwitchBook(_target.Book, _target.PageId);
        }

        public override string GetSummary()
        {
            return _target.GetSummary();
        }

        public override IEnumerable<ScriptBook> GetReferencingBooks()
        {
            if (_target.Book != null)
            {
                yield return _target.Book;
            }
        }

        public override void GetChainPreloadTargetBooks(ICollection<ScriptBook> books)
        {
            if (_target.Book == null) return;
            if (_chainPreload) books.Add(_target.Book);
        }
    }
}