using LazyBones.Config;

namespace LazyBones.Log.Config
{
    public class TargetAttribute : ConfigItemAttribute
    {
        public TargetAttribute(string name)
            : base(name)
        {
        }
    }
    public class FilterAttribute : ConfigItemAttribute
    {
        public FilterAttribute(string name)
            : base(name)
        {
        }
    }
    public class LayoutAttribute : ConfigItemAttribute
    {
        public LayoutAttribute(string name)
            : base(name)
        {
        }
    }
    public class RendererAttribute : ConfigItemAttribute
    {
        public RendererAttribute(string name)
            : base(name)
        {
        }
    }
}
