namespace Kairou
{
    public class WhileBlock : Block
    {
        static readonly ObjectPool<WhileBlock> _pool = new(
            () => new WhileBlock(),
            initialCapacity: 0,
            maxCapacity: 4);

        public static WhileBlock Rent() => _pool.Rent();
        static void Return(WhileBlock block)
        {
            block.Start = null;
            block.End = null;
            _pool.Return(block);
        }

        WhileBlock() {}

        public override void Dispose()
        {
            Return(this);
        }
    }
}