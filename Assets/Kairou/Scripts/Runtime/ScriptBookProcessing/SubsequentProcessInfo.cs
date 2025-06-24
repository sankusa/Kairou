using System;

namespace Kairou
{
    public readonly struct SubsequentProcessInfo
    {
        public ScriptBook Book { get; }
        public string PageId { get; }

        public bool HasPageId => string.IsNullOrEmpty(PageId) == false;

        public bool IsSubsequentPageInfo => Book == null && string.IsNullOrEmpty(PageId) == false;
        public bool IsSubsequentBookInfo => Book != null;

        public SubsequentProcessInfo(ScriptBook book, string pageId = null)
        {
            Book = book;
            PageId = pageId;
        }

        public SubsequentProcessInfo(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) throw new ArgumentNullException(nameof(pageId));

            Book = null;
            PageId = pageId;
        }
    }
}