using System;
using System.ComponentModel;
using System.Text;

namespace LazyBones.UI.Designers
{
    class EncodingConverter : TypeConverter
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
                try {
                    return Encoding.GetEncoding((string)value);
                }
                catch{}
            }
            return base.ConvertFrom(context, culture, value);
        }
        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (value == null || !(value is Encoding))
                return "未知编码方式";
            if (destinationType == typeof(string))
            {
                return (value as Encoding).EncodingName;
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
