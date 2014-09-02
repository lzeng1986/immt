using System.Windows.Forms;
using System.Drawing;

namespace LazyBones.UI
{
    /// <summary>
    /// 定义菜单项行为的基类
    /// </summary>
    public abstract class MenuItemBase
    {
        /// <summary>
        /// 点击时执行的方法
        /// </summary>
        public virtual void OnClick(Form mainForm,RunningContext runningContext) { }
        /// <summary>
        /// 获取菜单显示的文字
        /// </summary>
        public abstract string Text { get; }
        /// <summary>
        /// 获取菜单显示的图片
        /// </summary>
        public virtual Image Img
        {
            get { return null; }
        }
    }
}
