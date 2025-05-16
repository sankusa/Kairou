namespace Kairou
{
    public class IfBlock : Block
    {
        static readonly ObjectPool<IfBlock> _pool = new(() => new IfBlock(), 0, 4);
        public static IfBlock Rent() => _pool.Rent();
        static void Return(IfBlock block)
        {
            block.Start = null;
            block.End = null;
            block.EvaluationFinished = false;
            _pool.Return(block);
        }

        public bool EvaluationFinished { get; set; }

        IfBlock() {}

        public override void Dispose()
        {
            Return(this);
        }
    }
}