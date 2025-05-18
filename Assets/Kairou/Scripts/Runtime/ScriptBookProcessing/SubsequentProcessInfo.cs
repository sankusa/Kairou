using System;

namespace Kairou
{
    public readonly struct SubsequentProcessInfo
    {
        public bool SwitchSeries { get; }
        public ScriptBook Book { get; }
        public string PageId { get; }

        public bool HasPageId => string.IsNullOrEmpty(PageId) == false;

        public bool IsSubsequentPageInfo => SwitchSeries == false && Book == null && string.IsNullOrEmpty(PageId) == false;
        public bool IsSubsequentBookInfo => SwitchSeries == false && Book != null;
        public bool IsSubsequentSeriesInfo => SwitchSeries == true && Book != null;

        public SubsequentProcessInfo(bool switchSeries, ScriptBook book, string pageId = null)
        {
            SwitchSeries = switchSeries;
            Book = book;
            PageId = pageId;
        }

        public SubsequentProcessInfo(ScriptBook book, string pageId = null)
        {
            SwitchSeries = false;
            Book = book;
            PageId = pageId;
        }

        public SubsequentProcessInfo(string pageId)
        {
            if (string.IsNullOrEmpty(pageId)) throw new ArgumentNullException(nameof(pageId));

            SwitchSeries = false;
            Book = null;
            PageId = pageId;
        }
    }
}