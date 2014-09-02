using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using LazyBones.Extensions;
using LazyBones.Linq;

namespace LazyBones.UI.Controls.Gantt
{
    [DockingAttribute(DockingBehavior.Ask)]
    public class GanttChart : Control
    {
        GanttSource source = new GanttSource();
        IViewport viewport = null;
        GanttItem dragSource = null;
        Point dragLastLocation = Point.Empty;
        Point dragStartLocation = Point.Empty;
        List<GanttItem> selectedItems = new List<GanttItem>(); // List of selected items
        List<GanttItem> drawItems = new List<GanttItem>();
        GanttItem itemMouseEntered = null;
        TimeScale timeScale;
        IContainer components = new Container();

        public GanttChart()
        {
            SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.Opaque |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw,
                 true
                 );
            this.viewport = new ControlViewport(this);
            AllowItemDragDrop = true;
            AccumulateRelationsOnGroup = false;
            this.Margin = Padding.Empty;
            this.Padding = Padding.Empty;
            this.timeScale = new LazyBones.UI.Controls.Gantt.TimeScale(this.components);
            itemFormat = new ItemStyle(components)
            {
                BackColor = Color.MediumSlateBlue,
                ForeColor = Color.YellowGreen,
                BorderColor = Color.Black,
                FontColor = Color.Black,
                RelationColor = Color.Black
            };
            criticalItemFormat = new ItemStyle(components)
            {
                BorderColor = Color.Maroon,
                BackColor = Color.Crimson,
                ForeColor = Color.YellowGreen
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        public void InvalidateChart()
        {
            GenerateModels();
            Invalidate();
        }

        [Browsable(false)]
        public GanttSource Source
        {
            get { return source; }
        }

        [Browsable(false)]
        public IEnumerable<GanttItem> SelectedItems
        {
            get { return selectedItems; }
        }
        [Browsable(false)]
        public GanttItem SelectedItem
        {
            get { return selectedItems.LastOrDefault(); }
        }

        ItemStyle itemFormat;
        [Category("Gantt"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ItemStyle ItemFormat
        {
            get { return itemFormat; }
        }

        ItemStyle criticalItemFormat = null;
        [Category("Gantt"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ItemStyle CriticalItemFormat
        {
            get { return criticalItemFormat; }
        }

        [DefaultValue(true), Category("Gantt")]
        public bool AllowItemDragDrop { get; set; }

        bool showRelations = true;
        [DefaultValue(true), Category("Gantt")]
        public bool ShowRelations
        {
            get { return showRelations; }
            set
            {
                showRelations = value;
                Invalidate();
            }
        }

        bool showLabels = true;
        [DefaultValue(true), Category("Gantt")]
        public bool ShowItemLabels
        {
            get { return showLabels; }
            set
            {
                showLabels = value;
                Invalidate();
            }
        }

        [DefaultValue(false), Category("Gantt")]
        public bool AccumulateRelationsOnGroup { get; set; }

        bool showSlack = false;
        [DefaultValue(false), Category("Gantt")]
        public bool ShowSlack
        {
            get { return showSlack; }
            set
            {
                showSlack = value;
                Invalidate();
            }
        }

        int barSpacing = 26;
        [DefaultValue(26), Category("Gantt")]
        public int BarSpacing
        {
            get { return barSpacing; }
            set
            {
                barSpacing = value;
                InvalidateChart();
            }
        }

        int barHeight = 20;
        [DefaultValue(20), Category("Gantt")]
        public int BarHeight
        {
            get { return barHeight; }
            set
            {
                if (value < 10)
                    throw new ArgumentOutOfRangeException("BarHeigth", "值不得小于10");
                barHeight = value;
                InvalidateChart();
            }
        }

        [DefaultValue(typeof(Font), "DialogFont"), Category("Gantt")]
        public Font HeaderFont
        {
            get { return timeScale.Font; }
            set
            {
                if (timeScale.Font == value)
                    return;
                timeScale.Font = value;
                Invalidate();
            }
        }

        [DefaultValue(20), Category("Gantt")]
        public int Header1Height
        {
            get { return timeScale.Header1Height; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("值应该大于零");
                if (timeScale.Header1Height == value)
                    return;
                timeScale.Header1Height = value;
                InvalidateChart();
            }
        }

        [DefaultValue(20), Category("Gantt")]
        public int Header2Height
        {
            get { return timeScale.Header2Height; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("值应该大于零");
                if (timeScale.Header2Height == value)
                    return;
                timeScale.Header2Height = value;
                InvalidateChart();
            }
        }

        [DefaultValue(typeof(Color), "ControlText"), Category("Gantt")]
        public Color HeaderForeColor
        {
            get { return timeScale.ForeColor; }
            set
            {
                if (timeScale.ForeColor == value)
                    return;
                timeScale.ForeColor = value;
                Invalidate();
            }
        }

        [DefaultValue(ContentAlignment.MiddleCenter), Category("Gantt")]
        public ContentAlignment Head1TextAlign
        {
            get { return timeScale.Head1TextAlign; }
            set
            {
                if (timeScale.Head1TextAlign == value)
                    return;
                timeScale.Head1TextAlign = value;
                Invalidate();
            }
        }

        [DefaultValue(ContentAlignment.MiddleCenter), Category("Gantt")]
        public ContentAlignment Head2TextAlign
        {
            get { return timeScale.Head2TextAlign; }
            set
            {
                if (timeScale.Head2TextAlign == value)
                    return;
                timeScale.Head2TextAlign = value;
                Invalidate();
            }
        }

        [DefaultValue(30), Category("Gantt")]
        public int PeriodWidth
        {
            get { return timeScale.PeriodWidth; }
            set
            {
                if (timeScale.PeriodWidth == value)
                    return;
                timeScale.PeriodWidth = Math.Max(30, value);
                InvalidateChart();
            }
        }

        [DefaultValue(TimeDisplayScale.Day), Category("Gantt")]
        public TimeDisplayScale DisplayScale
        {
            get { return timeScale.DisplayScale; }
            set
            {
                if (timeScale.DisplayScale == value)
                    return;
                timeScale.DisplayScale = value;
                InvalidateChart();
            }
        }

        DateTime startTime = DateTime.Now;
        public DateTime StartTime
        {
            get { return timeScale.StartTime; }
            set
            {
                if (timeScale.StartTime == value)
                    return;
                timeScale.StartTime = value;
                InvalidateChart();
            }
        }
        void ResetStartTime()
        {
            StartTime = DateTime.Now;
        }

        public event EventHandler<GanttItemEventArgs> ItemEnter;
        public event EventHandler<GanttItemEventArgs> ItemLeave;
        public event EventHandler<GanttMouseEventArgs> ItemMouseClick;
        public event EventHandler<GanttMouseEventArgs> ItemMouseDoubleClick;
        public event EventHandler<DragDropEventArgs> ItemDrag;
        public event EventHandler<DragDropEventArgs> ItemDrop;
        public event EventHandler<GanttItemEventArgs> ItemSelected;
        public event EventHandler<GanttItemEventArgs> ItemDeselecting;
        public event EventHandler<ItemPaintEventArgs> ItemPaint;
        public event EventHandler<MouseEventArgs> HeaderMouseClick;
        public event EventHandler<MouseEventArgs> HeaderMouseDoubleClick;

        public bool TryGetRow(GanttItem item, out int row)
        {
            row = drawItems.IndexOf(item);
            return row != -1;
        }

        public bool TryGetItem(int row, out GanttItem item)
        {
            if (0 <= row && row < drawItems.Count)
            {
                item = drawItems[row];
                return true;
            }
            else
            {
                item = null;
                return false;
            }
        }
        public void AutoModifyStartTime()
        {
            var start = source.RootItems.Where(t => t.Start != DateTime.MinValue).MinOrDefault(t => t.Start, DateTime.Now);
            StartTime = start - timeScale.GetOffset(PeriodWidth);
        }

        public void Print(PrintDocument document, float scale = 1.0f)
        {
            // save a copy of the current viewport and swap it with PrintViewport
            var rawViewport = viewport;

            float x = 0; // viewport world x, y coords
            float y = 0;
            int pageCount = 0;

            document.PrintPage += (s, e) =>
            {
                e.HasMorePages = false;
                pageCount++;

                // create a PrintViewport to navigate the world
                var printViewport = new PrintViewport(e.Graphics,
                    rawViewport.WorldWidth, rawViewport.WorldHeight,
                    e.MarginBounds.Width, e.MarginBounds.Height,
                    e.PageSettings.Margins.Left, e.PageSettings.Margins.Right);
                printViewport.Scale = scale;
                rawViewport = printViewport;

                // move the viewport
                printViewport.X = x;
                printViewport.Y = y;

                // set clip and draw
                e.Graphics.SetClip(e.MarginBounds);
                Draw(e.Graphics, e.PageBounds);

                // check if reached end of printing
                if (printViewport.ViewRectangle.Right < printViewport.WorldWidth)
                {
                    // continue horizontally
                    x += printViewport.ViewRectangle.Width;
                    e.HasMorePages = true;
                }
                else
                {
                    // reached end of worldwidth so we go down vertically once
                    x = 0;
                    if (printViewport.ViewRectangle.Bottom < printViewport.WorldHeight)
                    {
                        y += printViewport.ViewRectangle.Height;
                        e.HasMorePages = true;
                    }
                }
            };
            document.Print();

            viewport = rawViewport;
        }

        public GanttHitInfo GetHitInfo(Point location)
        {
            var row = ChartCoordToChartRow(location.Y);
            var col = GetDeviceColumnAt(location);
            var item = GetItemAt(location);
            return new GanttHitInfo(row, timeScale.GetTimeInColumn(col), item);
        }

        public void ScrollTo(DateTime datetime)
        {
            var off = timeScale.GetOffset(datetime);
            viewport.X = off;
        }
        public void ScrollTo(GanttItem item)
        {
            if (!item.DrawRect.IsEmpty)
            {
                ScrollTo(item.Start);
            }
        }
        public void BeginBillboardMode(Graphics graphics)
        {
            graphics.Transform = ControlViewport.Identity;
        }
        public void EndBillboardMode(Graphics graphics)
        {
            graphics.Transform = viewport.Projection;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Draw(e.Graphics, e.ClipRectangle);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var hitItem = GetItemAt(e.Location);
            if (itemMouseEntered != null && hitItem == null)
            {
                OnItemMouseLeave(new GanttItemEventArgs(itemMouseEntered));
                itemMouseEntered = null;
            }
            else if (itemMouseEntered == null && hitItem != null)
            {
                itemMouseEntered = hitItem;
                OnItemMouseEnter(new GanttItemEventArgs(itemMouseEntered));
            }

            // Dragging
            if (AllowItemDragDrop && dragSource != null)
            {
                var target = hitItem;
                if (target == dragSource)
                    target = null;
                //var targetRect = target == null ? RectangleF.Empty : itemHitRects[target];
                //int row = DeviceCoordToChartRow(e.Location.Y);
                //OnItemMouseDrag(new DragDropEventArgs(dragStartLocation, dragLastLocation, dragSource, itemHitRects[dragSource], target, targetRect, row, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                //dragLastLocation = e.Location;
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            if (timeScale.Contains(viewport.DeviceToWorldCoord(e.Location)))
                HeaderMouseClick.SafeCall(this, e);
            else
            {
                var hitItem = GetItemAt(e.Location);
                if (hitItem != null)
                {
                    OnItemMouseClick(hitItem, e);
                }
                else
                {
                    OnItemDeselecting(null);
                }
            }

            base.OnMouseClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (AllowItemDragDrop)
            {
                dragSource = GetItemAt(e.Location);
                if (dragSource != null)
                {
                    dragStartLocation = e.Location;
                    dragLastLocation = e.Location;
                }
            }

            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (AllowItemDragDrop && dragSource != null)
            {
                var target = GetItemAt(e.Location);
                if (target == dragSource)
                    target = null;
                //var targetRect = target == null ? RectangleF.Empty : itemHitRects[target];
                //int row = DeviceCoordToChartRow(e.Location.Y);
                //OnItemMouseDrop(new DragDropEventArgs(dragStartLocation, dragLastLocation, dragSource, itemHitRects[dragSource], target, targetRect, row, e.Button, e.Clicks, e.X, e.Y, e.Delta));
                //dragSource = null;
                //dragLastLocation = Point.Empty;
                //dragStartLocation = Point.Empty;
            }

            base.OnMouseUp(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            var item = GetItemAt(e.Location);
            if (item != null)
            {
                OnItemMouseDoubleClick(new GanttMouseEventArgs(item, e));
            }

            base.OnMouseDoubleClick(e);
        }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (Control.ModifierKeys.HasFlag(Keys.Control))
            {
                if (e.Delta > 0)
                {
                    timeScale.ZoomIn();
                }
                else
                {
                    timeScale.ZoomOut();
                }
                InvalidateChart();
            }
            base.OnMouseWheel(e);
        }
        protected virtual void OnItemMouseEnter(GanttItemEventArgs e)
        {
            ItemEnter.SafeCall(this, e);
            this.Cursor = Cursors.Hand;
            this.Invalidate();
        }

        protected virtual void OnItemMouseLeave(GanttItemEventArgs e)
        {
            ItemLeave.SafeCall(this, e);
            this.Cursor = Cursors.Default;
            this.Invalidate();
        }

        protected virtual void OnItemMouseDrag(DragDropEventArgs e)
        {
            // fire listeners
            //if (ItemDrag != null)
            //    ItemDrag(this, e);

            //// Default drag behaviors **********************************
            //if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            //{
            //    //var complete = e.Source.Complete + (float)(e.X - e.PreviousLocation.X) / (e.Source.Duration * timeScale.PeriodWidth);
            //    //source.SetComplete(e.Source, complete);
            //}
            //else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //{
            //    if (e.Target == null)
            //    {
            //        var delta = (e.PreviousLocation.X - e.StartLocation.X);
            //        overlay.DraggedRect = e.SourceRect;
            //        overlay.DraggedRect.Width += delta;
            //    }
            //    else // drop targetting (join)
            //    {
            //        overlay.DraggedRect = e.TargetRect;
            //        overlay.Row = int.MinValue;
            //    }
            //}
            //else if (e.Button == System.Windows.Forms.MouseButtons.Left)
            //{
            //    overlay.Clear();

            //    if (e.Target == null)
            //    {
            //        if (Control.ModifierKeys.HasFlag(Keys.Shift))
            //        {
            //            // insertion line
            //            overlay.Row = e.Row;
            //        }
            //        else
            //        {
            //            // displacing horizontally
            //            overlay.DraggedRect = e.SourceRect;
            //            overlay.DraggedRect.Offset((e.X - e.StartLocation.X) / timeScale.PeriodWidth * timeScale.PeriodWidth, 0);
            //        }
            //    }
            //    else // drop targetting (subitem / predecessor)
            //    {
            //        overlay.DraggedRect = e.TargetRect;
            //        overlay.Row = int.MinValue;
            //    }
            //}
            this.Invalidate();
        }

        protected virtual void OnItemMouseDrop(DragDropEventArgs e)
        {
            ItemDrop.SafeCall(this, e);

            var delta = (e.PreviousLocation.X - e.StartLocation.X) / timeScale.PeriodWidth;

            if (e.Button == MouseButtons.Left)
            {
                if (e.Target == null)
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        // insert
                        int from;
                        var sourceItem = e.Source;
                        sourceItem = sourceItem.SplitParent ?? sourceItem;
                        if (TryGetRow(sourceItem, out from))
                            source.Move(sourceItem, e.Row - from);
                    }
                    else
                    {
                        // displace horizontally
                        //var start = e.Source.Start + delta;
                        //source.SetStart(e.Source, start);
                    }
                }
                else // have drop target
                {
                    if (Control.ModifierKeys.HasFlag(Keys.Shift))
                    {
                        source.Relate(e.Target, e.Source);
                    }
                    else if (Control.ModifierKeys.HasFlag(Keys.Alt))
                    {
                        var sourceItem = e.Source;
                        sourceItem = sourceItem.SplitParent ?? sourceItem;
                        if (sourceItem.Parent == e.Target)
                        {
                            source.Ungroup(e.Target, sourceItem);
                        }
                        else
                        {
                            source.Unrelate(e.Target, sourceItem);
                        }
                    }
                    else
                    {
                        source.Group(e.Target, e.Source);
                    }
                }
            }
            else if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                //if (e.Target == null)
                //{
                //    var duration = e.Source.Duration + delta;
                //    source.SetDuration(e.Source, duration);
                //}
                //else // have target then we do a join
                //{
                //    source.Join(e.Target, e.Source);
                //}
            }

            this.Invalidate();
        }

        protected virtual void OnItemMouseClick(GanttItem item, MouseEventArgs e)
        {
            ItemMouseClick.SafeCall(this, new GanttMouseEventArgs(item, e));
            if (e.Button == MouseButtons.Left)
            {
                if (ModifierKeys.HasFlag(Keys.Control))
                {
                    if (selectedItems.Contains(item))
                    {
                        OnItemDeselecting(item);
                        selectedItems.Remove(item);
                    }
                    else
                    {
                        selectedItems.Add(item);
                        OnItemSelected(item);
                    }
                }
                else
                {
                    if (selectedItems.SingleOrDefault() != item)
                    {
                        selectedItems.Clear();
                        OnItemDeselecting(null);
                        selectedItems.Add(item);
                        OnItemSelected(item);
                    }
                }
            }
            this.Invalidate();
        }

        protected virtual void OnItemMouseDoubleClick(GanttMouseEventArgs e)
        {
            ItemMouseDoubleClick.SafeCall(this, e);

            if (e.Button == MouseButtons.Left)
            {
                e.Item.IsCollapsed = !e.Item.IsCollapsed;
                this.InvalidateChart();
            }
            else if (e.Button == MouseButtons.Middle)
            {
                ScrollTo(e.Item);
            }
        }

        protected virtual void OnItemSelected(GanttItem item)
        {
            ItemSelected.SafeCall(this, new GanttItemEventArgs(item));
        }

        protected virtual void OnItemDeselecting(GanttItem item)
        {
            ItemDeselecting.SafeCall(this, new GanttItemEventArgs(item));
        }

        GanttItem GetItemAt(Point location)
        {
            var chartcoord = viewport.DeviceToWorldCoord(location);
            if (timeScale.Contains(chartcoord))
                return null;
            var item = drawItems.FirstOrDefault(r => r.DrawRect.Contains(chartcoord));
            if (item != null && item.HasPart)
            {
                return item.Parts.FirstOrDefault(r => r.DrawRect.Contains(chartcoord));
            }
            else
                return item;
        }

        int GetDeviceColumnAt(Point location)
        {
            var worldcoord = viewport.DeviceToWorldCoord(location);

            return timeScale.GetDeviceColumnAt(worldcoord);
        }

        int DeviceCoordToChartRow(float y)
        {
            y += viewport.Y;
            var row = (int)((y - this.BarSpacing - timeScale.Header1Height) / this.BarSpacing);
            return row < 0 ? 0 : row;
        }

        int ChartCoordToChartRow(float y)
        {
            var row = (int)((y - timeScale.HeaderHeight) / this.BarSpacing);
            return row < 0 ? 0 : row;
        }

        float ChartRowToChartCoord(int row)
        {
            return row * this.BarSpacing + timeScale.HeaderHeight;
        }

        void GenerateModels()
        {
            drawItems.ForEach(i =>
                {
                    i.DrawRect = RectangleF.Empty;
                    i.SlackRect = RectangleF.Empty;
                });
            drawItems.Clear();

            var height = this.Parent == null ? this.Width : this.Parent.Height;
            var width = this.Parent == null ? this.Height : this.Parent.Width;

            var end = DateTime.MinValue;
            int row = 0;
            int y_coord = timeScale.HeaderHeight + (this.BarSpacing - this.BarHeight) / 2;
            foreach (var item in source.VisibleItems)
            {
                var itemRect = new RectangleF(timeScale.GetOffset(item.Start), y_coord, timeScale.GetOffset(item.Duration), BarHeight);
                item.DrawRect = itemRect;
                drawItems.Add(item);
                if (item.HasPart)
                {
                    foreach (var part in item.Parts)
                    {
                        var rect = new RectangleF(timeScale.GetOffset(part.Start), y_coord, timeScale.GetOffset(part.Duration), BarHeight);
                        part.DrawRect = rect;
                    }
                }

                if (this.ShowSlack)
                {
                    var slackRect = new RectangleF(timeScale.GetOffset(item.End), y_coord, timeScale.GetOffset(item.Slack), BarHeight);
                    item.SlackRect = slackRect;
                }

                if (item.End > end)
                    end = item.End;

                row++;
                y_coord += BarSpacing;
            }
            row += 5;
            viewport.WorldWidth = timeScale.GetOffset(end) + PeriodWidth;
            viewport.WorldHeight = row * this.BarSpacing + this.BarHeight;
        }

        void Draw(Graphics g, Rectangle clipRect)
        {
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            timeScale.Generate(viewport);
            g.TranslateTransform(-viewport.X, -viewport.Y);
            timeScale.DrawColumns(g);

            int row = 0;
            if (!DesignMode)
            {
                row = DrawItems(g, clipRect);
                if (this.ShowRelations)
                    DrawRelationLines(g);
                //var paintargs = new ChartPaintEventArgs(graphics, clipRect, this);
                //OnPaintOverlay(paintargs);
                //overlay.Paint(paintargs);
            }

            timeScale.DrawHeader(g);

            g.ResetTransform();

            // flush
            g.Flush();
        }

        int DrawItems(Graphics g, Rectangle clipRect)
        {
            var viewRect = viewport.ViewRectangle;
            int row = 0;
            var crit_item_set = new HashSet<GanttItem>(source.CriticalPaths.SelectMany(x => x));
            var pen = new Pen(Color.Gray);
            pen.DashStyle = DashStyle.Dot;
            var startRow = (int)viewport.Y / BarSpacing;
            var count = (int)viewport.ViewRectangle.Height / BarSpacing;
            foreach (var item in drawItems.Skip(startRow).Take(count))
            {
                var itemRect = item.DrawRect;
                var critical = crit_item_set.Contains(item);
                var format = critical ? CriticalItemFormat : ItemFormat;
                ItemPaint.SafeCall(this, new ItemPaintEventArgs(item, row, critical, format));

                if (viewRect.IntersectsWith(itemRect))
                {
                    if (item.HasPart)
                    {
                        DrawItemParts(g, format, item);
                    }
                    else
                    {
                        DrawRegularItemAndGroup(g, format, item, itemRect);
                    }
                    if (this.ShowItemLabels && !string.IsNullOrEmpty(item.Name))
                    {
                        var rect = new RectangleF(itemRect.Right, itemRect.Top, Width, itemRect.Height);
                        g.DrawString(item.Name, format.Font, format.FontBrush, rect, format.LabelFormat);
                    }
                }
                if (this.ShowSlack && item.Complete < 1.0f)
                {
                    var slackrect = item.SlackRect;
                    if (viewRect.IntersectsWith(slackrect))
                        g.FillRectangle(format.SlackBrush, slackrect);
                }
                row++;
            }

            return row;
        }

        void DrawRelationLines(Graphics graphics)//画出relation的折线
        {
            var viewRect = viewport.ViewRectangle;
            var clipRectF = new RectangleF(viewRect.X, viewRect.Y, viewRect.Width, viewRect.Height);
            foreach (var item in source.ItemsHasRelation.Where(i => !i.DrawRect.IsEmpty))
            {
                foreach (var successor in source.DirectSuccessorsOf(item).Where(i => !i.DrawRect.IsEmpty))
                {
                    var from = item.DrawRect;
                    var to = successor.DrawRect;
                    var p1 = new PointF(from.Right, from.Bottom);
                    var p2 = new PointF(from.Right, to.Top + to.Height / 2.0f);
                    var p3 = new PointF(to.Left, to.Top + to.Height / 2.0f);
                    var linerect = new RectangleF(p1.X, Math.Max(p1.Y, p3.Y), p3.X - p1.X, Math.Abs(p3.Y - p1.Y));
                    if (clipRectF.IntersectsWith(linerect))
                    {
                        graphics.DrawLines(itemFormat.RelationPen, new[] { p1, p2, p3 });
                    }
                }
            }
        }
        const int PlusMinusSize = 10;
        void DrawRegularItemAndGroup(Graphics g, ItemStyle format, GanttItem item, RectangleF itemRect)
        {
            var fill = new RectangleF(itemRect.X, itemRect.Y, itemRect.Width * (float)item.Complete, itemRect.Height);
            g.FillRectangle(format.BackBrush, itemRect);
            g.FillRectangle(format.ForeBrush, fill);
            g.DrawRectangle(format.BorderPen, itemRect);

            if (item.HasChild)
            {
                var heigth = itemRect.Height / 4;
                if (item.IsCollapsed)
                {
                    g.FillRectangle(Brushes.Black, itemRect.X, itemRect.Y + heigth * 3, itemRect.Width, heigth);
                }
                else
                {
                    g.FillRectangle(Brushes.Black, itemRect.X, itemRect.Y, itemRect.Width, heigth);
                }
            }
        }

        void DrawItemParts(Graphics g, ItemStyle format, GanttItem item)
        {
            var parts = item.Parts;
            var partRects = item.Parts.Select(t => t.DrawRect).ToArray();

            var firstRect = partRects[0];
            var lastRect = partRects[partRects.Length - 1];
            var y_coord = (firstRect.Top + firstRect.Bottom) / 2;
            var point1 = new PointF(firstRect.Right, y_coord);
            var point2 = new PointF(lastRect.Left, y_coord);
            g.DrawLine(format.SplitLinePen, point1, point2);//画连接各部分的线

            g.FillRectangles(format.BackBrush, partRects);

            g.FillRectangles(format.ForeBrush,
                partRects.Select(
                    (x, i) => new RectangleF(x.X, x.Y, x.Width * (float)parts[i].Complete, x.Height)
                ).ToArray());//画完成进度

            g.DrawRectangles(format.BorderPen, partRects);//画边框
        }
    }
}
