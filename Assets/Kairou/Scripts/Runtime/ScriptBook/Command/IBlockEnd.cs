namespace Kairou
{
    public interface IBlockEnd
    {
        string BlockCategory { get; }
        bool IsLoopBlock { get; }
    }
}