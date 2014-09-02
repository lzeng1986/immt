using System;
using System.ComponentModel;

namespace LazyBones.UI.Controls
{
    public class ComponentWithContainerForm : Component
    {
        protected System.Windows.Forms.Form formHost = null;
        [Browsable(false)]
        public virtual System.Windows.Forms.Form Form
        {
            get
            {
                return formHost;
            }
            set
            {
                if (formHost != null && formHost != value)
                {
                    throw new InvalidOperationException("ContainerForm属性至允许初始化一次");
                }
                formHost = value;
            }
        }

        public override ISite Site
        {
            get
            {
                return base.Site;
            }
            set
            {
                base.Site = value;
                if (value == null || !this.DesignMode)
                    return;
                var host = value.GetService(typeof(System.ComponentModel.Design.IDesignerHost)) as System.ComponentModel.Design.IDesignerHost;
                if (host == null)
                    return;
                if (host.RootComponent is System.Windows.Forms.Form)
                    formHost = host.RootComponent as System.Windows.Forms.Form;
            }
        }        

    }
}
