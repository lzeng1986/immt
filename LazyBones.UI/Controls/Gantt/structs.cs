using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using LazyBones.Extensions;
using System.Collections;

namespace LazyBones.UI.Controls.Gantt
{
    [TypeConverter(typeof(Converter))]
    public class ItemStyle : Component
    {
        internal ItemStyle(IContainer container)
        {
            container.Add(this);
        }
        internal Pen BorderPen = new Pen(Color.Black);
        [NotifyParentProperty(true)]
        public Color BorderColor
        {
            get { return BorderPen.Color; }
            set
            {
                if (BorderPen.Color == value)
                    return;
                BorderPen.Color = value;
            }
        }

        internal SolidBrush BackBrush = new SolidBrush(Color.MediumSlateBlue);
        [NotifyParentProperty(true)]
        public Color BackColor
        {
            get { return BackBrush.Color; }
            set
            {
                if (BackBrush.Color == value)
                    return;
                BackBrush.Color = value;
            }
        }

        internal SolidBrush ForeBrush = new SolidBrush(Color.YellowGreen);
        [NotifyParentProperty(true)]
        public Color ForeColor
        {
            get { return ForeBrush.Color; }
            set
            {
                if (ForeBrush.Color == value)
                    return;
                ForeBrush.Color = value;
            }
        }

        internal SolidBrush FontBrush = new SolidBrush(Color.Black);
        [NotifyParentProperty(true)]
        public Color FontColor
        {
            get { return FontBrush.Color; }
            set
            {
                if (FontBrush.Color == value)
                    return;
                FontBrush.Color = value;
            }
        }

        Font font = SystemFonts.DefaultFont;
        [NotifyParentProperty(true)]
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        internal Pen SplitLinePen = new Pen(Color.Black) { DashStyle = DashStyle.DashDot };
        [NotifyParentProperty(true)]
        public Color SplitLineColor
        {
            get { return SplitLinePen.Color; }
            set
            {
                if (SplitLinePen.Color == value)
                    return;
                SplitLinePen.Color = value;
            }
        }

        internal Pen RelationPen = new Pen(Color.Black);
        [NotifyParentProperty(true)]
        public Color RelationColor
        {
            get { return RelationPen.Color; }
            set
            {
                if (RelationPen.Color == value)
                    return;
                RelationPen.Color = value;
            }
        }

        internal StringFormat LabelFormat = new StringFormat
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Near
        };

        internal Brush SlackBrush = new HatchBrush(HatchStyle.LightDownwardDiagonal, Color.Gray, Color.Transparent);

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                BorderPen.Dispose();
                BackBrush.Dispose();
                ForeBrush.Dispose();
                SplitLinePen.Dispose();
                RelationPen.Dispose();
                FontBrush.Dispose();
                LabelFormat.Dispose();
                SlackBrush.Dispose();
            }
        }
    }

    public struct GanttHitInfo
    {
        public int Row { get; set; }
        public DateTime DateTime { get; set; }
        public GanttItem Task { get; set; }
        public GanttHitInfo(int row, DateTime dateTime, GanttItem task)
            : this()
        {
            Row = row;
            DateTime = dateTime;
            Task = task;
        }
    }

    class HeaderInfo
    {
        public RectangleF H1Rect;
        public RectangleF H2Rect;
        public List<RectangleF> H1LabelRects = new List<RectangleF>();
        public List<RectangleF> H2LabelRects = new List<RectangleF>();
        public List<RectangleF> Columns = new List<RectangleF>();
        public List<DateTime> H2Dates = new List<DateTime>();
        public List<DateTime> H1Dates = new List<DateTime>();

        public void Clear()
        {
            H1LabelRects.Clear();
            H2LabelRects.Clear();
            Columns.Clear();
            H2Dates.Clear();
            H1Dates.Clear();
        }
    }
    class Converter : ExpandableObjectConverter
    {
        public override object ConvertTo(
                 ITypeDescriptorContext context,
                 CultureInfo culture,
                 object value,
                 Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return "";
            }

            return base.ConvertTo(
                context,
                culture,
                value,
                destinationType);
        }
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return false;
        }
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            //return context.Instance;
            return base.CreateInstance(context, propertyValues);
        }
    }

    public class TaskPaintStyle
    {

    }
}
