using System;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel;

namespace LazyBones.UI.Designers
{
    class ObjectEditor : UITypeEditor
    {
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object objValue)
        {
            if (((context != null) && (context.Instance != null)) && (provider != null))
            {
                var edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc == null)
                {
                    return objValue;
                }
                try
                {
                    var dialog = new FormObjectEditor() { EditValue = objValue };
                    if (edSvc.ShowDialog(dialog) == System.Windows.Forms.DialogResult.OK)
                    {
                        objValue = dialog.EditValue;
                    }
                }
                catch
                {
                }
            }
            return objValue;
        }

        public override UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                return UITypeEditorEditStyle.Modal;
            }
            return base.GetEditStyle(context);
        }

    }

    public class ObjectConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
                return true;
            return base.CanConvertFrom(context, sourceType);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
                return true;
            return base.CanConvertTo(context, destinationType);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value is string)
            {
                return Type.GetType((string)value, true);
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value == null || !(value is Type))
                return "(未添加绑定)";
            if ((value as Type).IsEnum && destinationType == typeof(string))
            {
                return (value as Type).FullName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
