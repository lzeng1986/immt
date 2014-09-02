using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing;

namespace LazyBones.UI.Controls.Docking
{
    [TypeConverter(typeof(DockPanelSkinConverter))]
    public class DockPanelSkin
    {
        Gradient stripGradient = new Gradient();
        public Gradient StripGradient
        {
            get { return stripGradient; }
            set { stripGradient = value; }
        }

        Gradient tabGradient = new Gradient();
        public Gradient TabGradient
        {
            get { return tabGradient; }
            set { tabGradient = value; }
        }

        TabGradient activeCaptionGradient = new TabGradient();
        public TabGradient ActiveCaptionGradient
        {
            get { return activeCaptionGradient; }
            set { activeCaptionGradient = value; }
        }

        TabGradient inactiveCaptionGradient = new TabGradient();
        public TabGradient InactiveCaptionGradient
        {
            get { return inactiveCaptionGradient; }
            set { inactiveCaptionGradient = value; }
        }

        TabGradient activeTabGradient = new TabGradient();
        public TabGradient ActiveTabGradient
        {
            get { return activeTabGradient; }
            set { activeTabGradient = value; }
        }

        TabGradient inactiveTabGradient = new TabGradient();
        public TabGradient InactiveTabGradient
        {
            get { return inactiveTabGradient; }
            set { inactiveTabGradient = value; }
        }

        Font stripFont = SystemFonts.MenuFont;
        [DefaultValue(typeof(Font), "MenuFont")]
        public Font StripFont
        {
            get { return stripFont; }
            set { stripFont = value; }
        }
    }

    [TypeConverter(typeof(TabGradientConverter))]
    public class TabGradient : Gradient
    {
        private Color m_textColor = SystemColors.ControlText;

        /// <summary>
        /// 获取或设置文字颜色
        /// </summary>
        [DefaultValue(typeof(SystemColors), "ControlText")]
        public Color TextColor
        {
            get { return m_textColor; }
            set { m_textColor = value; }
        }
    }

    [TypeConverter(typeof(GradientConverter))]
    public class Gradient
    {
        Color startColor = SystemColors.Control;
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color StartColor
        {
            get { return startColor; }
            set { startColor = value; }
        }

        Color endColor = SystemColors.Control;
        [DefaultValue(typeof(SystemColors), "Control")]
        public Color EndColor
        {
            get { return endColor; }
            set { endColor = value; }
        }

        LinearGradientMode mode = LinearGradientMode.Horizontal;
        [DefaultValue(LinearGradientMode.Horizontal)]
        public LinearGradientMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public LinearGradientBrush GetBrush(Rectangle rect)
        {
            return new LinearGradientBrush(rect, startColor, endColor, mode);
        }
    }

    public class DockPanelSkinConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(DockPanelSkin))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is DockPanelSkin)
            {
                return "DockPanelSkin";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class GradientConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Gradient))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(String) && value is Gradient)
            {
                return "DockPanelGradient";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public class TabGradientConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(TabGradient))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            TabGradient val = value as TabGradient;
            if (destinationType == typeof(String) && val != null)
            {
                return "DockPaneTabGradient";
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
