using System.Threading;
using Cysharp.Threading.Tasks;

namespace Kairou
{
    public interface IPreloadable
    {
        UniTask PreloadAsync();
        void Release();
    }
}