namespace Kairou
{
    public interface IBlockStart
    {
        string BlockCategory { get; }
        bool IsLoopBlock { get; }
    }
}