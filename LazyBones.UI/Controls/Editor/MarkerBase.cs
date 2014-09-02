
namespace LazyBones.UI.Controls.Editor
{
    /// <summary>
    /// 表示标记的抽象基类
    /// </summary>
    public abstract class MarkerBase : ISegment
    {
        protected int offset = -1;
        protected int length = -1;

        public virtual int Offset
        {
            get { return offset; }
            set { offset = value; }
        }

        public virtual int Length
        {
            get { return length; }
            set { length = value; }
        }
    }
}
