using System;
using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace LazyBones.UI.Designers
{
    class EnumDropDownEditor : UITypeEditor
    {
        private EnumCheckedBoxList flagEnumCtrl;

        public EnumDropDownEditor()
        {
            flagEnumCtrl = new EnumCheckedBoxList();
            flagEnumCtrl.BorderStyle = System.Windows.Forms.BorderStyle.None;
            
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context != null && context.Instance != null && provider != null)
            {

                var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    var e = (Enum)Convert.ChangeType(value, context.PropertyDescriptor.PropertyType);
                    flagEnumCtrl.EnumValue = e;
                    edSvc.DropDownControl(flagEnumCtrl);
                    return flagEnumCtrl.EnumValue;
                }
            }
            return null;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

    }
}
