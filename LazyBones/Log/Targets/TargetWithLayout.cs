using System.ComponentModel;
using LazyBones.Config;
using LazyBones.Log.Layouts;

namespace LazyBones.Log.Targets
{
    public abstract class TargetWithLayout : Target
    {
        [Required]
        [DefaultValue("{longDateTime}|{level}|{logger}|{message}")]
        public Layout Body { get; set; }

        public Layout Header { get; set; }

        public Layout Footer { get; set; }

        public override string ToString()
        {
            return string.Format("name:{0} type:{1} Body:{2} Header:{3} Footer:{4}",
                Name, 
                GetType(), 
                Body.LayoutText,
                Header == null ? "<empty>" : Header.LayoutText,
                Footer == null ? "<empty>" : Footer.LayoutText);
        }
    }

}
