using System.Collections.Generic;

namespace Kairou
{
    public class ProcessContext
    {
        static readonly ObjectPool<ProcessContext> _pool = new(
            createFunc: static () => new ProcessContext()
        );

        readonly List<IObjectResolver> _resolvers = new();
        public List<IObjectResolver> Resolvers => _resolvers;

        public static ProcessContext Rent() => _pool.Rent();

        public static void Return(ProcessContext context)
        {
            context.Clear();
            _pool.Return(context);
        }

        void Clear()
        {
            _resolvers.Clear();
        }
    }
}