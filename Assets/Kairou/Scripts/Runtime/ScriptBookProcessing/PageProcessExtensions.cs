using System.Collections.Generic;

namespace Kairou
{
    public static class PageProcessExtensions
    {
        public static T Resolve<T>(this PageProcess process) where T : class
        {
            return process.BookProcess.RootProcess.ObjectResolver.Resolve<T>();
        }

        public static IEnumerable<T> ResolveAll<T>(this PageProcess process) where T : class
        {
            return process.BookProcess.RootProcess.ObjectResolver.ResolveAll<T>();
        }
    }
}