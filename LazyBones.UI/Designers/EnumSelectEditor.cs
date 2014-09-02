using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace LazyBones.UI.Designers
{
    class EnumSelectEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {            
            if (context != null && provider != null)
            {
                var editorService = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                var designerHost = context.GetService(typeof(IDesignerHost)) as IDesignerHost;
                if (designerHost != null && editorService != null)
                {
                    var type = designerHost.GetType(designerHost.RootComponentClassName);
                    using (var form = new FormEnumTypeEditor() { Ass = type.Assembly })
                    {
                        if (editorService.ShowDialog(form) == System.Windows.Forms.DialogResult.OK)
                        {
                            return form.EnumType;
                        }
                    }
                }
            }
            return base.EditValue(context, provider, value);
        }
    }
}
