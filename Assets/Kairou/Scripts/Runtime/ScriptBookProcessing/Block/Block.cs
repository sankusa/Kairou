using System;

namespace Kairou
{
    public abstract class Block : IDisposable
    {
        public Command Start { get; set; }
        public int StartIndex => Start.Index;
        public Command End { get; set; }
        public int EndIndex
        {
            get
            {
                return End == null ? Start.ParentPage.Commands.Count : End.Index;
            }
        }

        public void SetStartAndEnd(IBlockStart blockStart)
        {
            Start = blockStart as Command;
            int endIndex = Start.ParentPage.FindBlockEndIndex(blockStart);
            End = endIndex == -1 ? null : Start.ParentPage.Commands[endIndex];
        }

        public virtual void Dispose() {}
    }
}