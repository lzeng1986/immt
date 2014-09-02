using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Gantt
{
    public class GanttMouseEventArgs : MouseEventArgs
    {
        public GanttItem Item { get; private set; }
        public GanttMouseEventArgs(GanttItem item, MouseEventArgs e)
            : base(e.Button, e.Clicks, e.X, e.Y, e.Delta)
        {
            this.Item = item;
        }
    }
    public class GanttItemEventArgs : EventArgs
    {
        public GanttItem Item { get; private set; }
        public GanttItemEventArgs(GanttItem item)
        {
            Item = item;
        }
    }

    public class DragDropEventArgs : MouseEventArgs
    {
        public Point PreviousLocation { get; private set; }

        public Point StartLocation { get; private set; }

        public GanttItem Source { get; private set; }

        public GanttItem Target { get; private set; }

        public RectangleF SourceRect { get; private set; }

        public RectangleF TargetRect { get; private set; }

        public int Row { get; private set; }

        public DragDropEventArgs(Point startLocation, Point prevLocation, GanttItem source, RectangleF sourceRect, GanttItem target, RectangleF targetRect, int row, MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            this.Source = source;
            this.SourceRect = sourceRect;
            this.Target = target;
            this.TargetRect = targetRect;
            this.PreviousLocation = prevLocation;
            this.StartLocation = startLocation;
            this.Row = row;
        }
    }

    /// <summary>
    /// Provides data for ChartPaintEvent
    /// </summary>
    public class ChartPaintEventArgs : PaintEventArgs
    {
        public GanttChart Chart { get; private set; }

        public ChartPaintEventArgs(Graphics graphics, Rectangle clipRect, GanttChart chart)
            : base(graphics, clipRect)
        {
            this.Chart = chart;
        }
    }

    public class ItemPaintEventArgs : EventArgs
    {
        public GanttItem Item { get; private set; }

        public int Row { get; private set; }

        public Font Font { get; set; }

        public ItemStyle Format { get; private set; }

        public bool IsCritical { get; private set; }

        public ItemPaintEventArgs(GanttItem item, int row, bool critical, ItemStyle itemStyle)
        {
            this.Item = item;
            this.Row = row;
            this.Format = itemStyle;
            this.IsCritical = critical;
        }
    }

    /// <summary>
    /// Provides data for RelationPaintEvent
    /// </summary>
    //public class RelationPaintEventArgs : ChartPaintEventArgs
    //{
    //    /// <summary>
    //    /// Get the precedent item in the relation
    //    /// </summary>
    //    public GanttItem Precedent { get; private set; }

    //    /// <summary>
    //    /// Get the dependant item in the relation
    //    /// </summary>
    //    public GanttItem Dependant { get; private set; }

    //    /// <summary>
    //    /// Get or set the formatting to use for drawing the relation
    //    /// </summary>
    //    //public RelationFormat Format { get; set; }

    //    /// <summary>
    //    /// Initialize a new instance of RelationPaintEventArgs with the editable default font and relation paint format
    //    /// </summary>
    //    //public RelationPaintEventArgs(Graphics graphics, Rectangle clipRect, GanttChart chart, GanttItem before, GanttItem after, RelationFormat format)
    //    //    : base(graphics, clipRect, chart)
    //    //{
    //    //    this.Precedent = before;
    //    //    this.Dependant = after;
    //    //    this.Format = format;
    //    //}
    //}
}
