using System;

namespace LazyBones
{
    public class LastDo : IDisposable
    {
        readonly Action act;
        public LastDo(Action lastDo)
        {
            act = lastDo;
        }
        public void Dispose()
        {
            act();
        }
        public static readonly LastDo GCCollect = new LastDo(GC.Collect);
    }
}
