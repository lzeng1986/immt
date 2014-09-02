using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;
using LazyBones.Extensions;
using System.Drawing.Drawing2D;
using LazyBones.Linq;

namespace LazyBones.UI.Controls.List
{
    [DockingAttribute(DockingBehavior.Ask)]
    public class ListView : Control
    {
        const int ResizeArrowPadding = 3;
        public Type TTT { get; set; }
        public ListView()
        {
            InitializeComponent();
            SetStyle(
                    ControlStyles.AllPaintingInWmPaint |
                    ControlStyles.ResizeRedraw |
                    ControlStyles.Opaque |
                    ControlStyles.UserPaint |
                    ControlStyles.DoubleBuffer |
                    ControlStyles.ResizeRedraw,
                 true
                 );
            columns = new ColumnCollection(this);
            rows = new ListRowCollection(this);
            BackColor = SystemColors.ControlLightLight;
        }
        ImageList imageList;
        SmartTimer mouseMoveHoverTimer;
        SmartTimer mouseHoverTimer;
        private HScrollBar hScrollBar;
        private VScrollBar vScrollBar;
        Control hole;
        System.ComponentModel.IContainer components;

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.mouseMoveHoverTimer = new LazyBones.UI.Controls.SmartTimer(this.components);
            this.mouseHoverTimer = new LazyBones.UI.Controls.SmartTimer(this.components);
            this.hScrollBar = new System.Windows.Forms.HScrollBar();
            this.vScrollBar = new System.Windows.Forms.VScrollBar();
            hole = new Control();
            this.SuspendLayout();
            // 
            // imageList
            // 
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // mouseMoveHoverTimer
            // 
            this.mouseMoveHoverTimer.Elapsed += new System.EventHandler(this.mouseMoveHoverTimer_Elapsed);
            // 
            // mouseHoverTimer
            // 
            this.mouseHoverTimer.Elapsed += new System.EventHandler(this.mouseHoverTimer_Elapsed);
            // 
            // hScrollBar
            // 
            this.hScrollBar.Location = new System.Drawing.Point(0, 0);
            this.hScrollBar.Name = "hScrollBar";
            this.hScrollBar.Size = new System.Drawing.Size(80, 17);
            this.hScrollBar.TabIndex = 0;
            this.hScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            this.Controls.Add(hScrollBar);
            // 
            // vScrollBar
            // 
            this.vScrollBar.Location = new System.Drawing.Point(0, 0);
            this.vScrollBar.Name = "vScrollBar";
            this.vScrollBar.Size = new System.Drawing.Size(17, 80);
            this.vScrollBar.TabIndex = 0;
            this.vScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.OnScroll);
            this.Controls.Add(vScrollBar);
            // 
            // ListView
            // 
            this.Name = "ListView";
            this.Controls.Add(hole);
            this.ResumeLayout(false);

        }

        void mouseMoveHoverTimer_Elapsed(object sender, EventArgs e)
        {
            var hitInfo = GetHitInfoAt(PointToClient(Control.MousePosition));
            if (lastHitInfo != hitInfo)
            {
                if (HitHover)
                    HotHitChanged.SafeCall(this, new HotHitEventArgs(hitInfo));
                lastHitInfo = hitInfo;
            }
        }

        private void mouseHoverTimer_Elapsed(object sender, EventArgs e)
        {

        }

        public event EventHandler<ClickEventArgs> SelectedIndexChanged;

        [Category("Appearance"), DefaultValue(typeof(Color), "ControlLightLight")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }

        bool allowRowAlternate = false;
        [Category("Appearance"), DefaultValue(false), Browsable(true)]
        public bool AllowRowAlternate
        {
            get { return allowRowAlternate; }
            set
            {
                allowRowAlternate = value;
                Invalidate();
            }
        }

        Color alternateBackColor = Color.DarkGreen;
        [Category("Appearance"), DefaultValue(typeof(Color), "DarkGreen")]
        public Color AlternateBackColor
        {
            get { return alternateBackColor; }
            set
            {
                alternateBackColor = value;
                Invalidate();
            }
        }

        bool showBorder = false;
        [Category("Appearance"), DefaultValue(false)]
        public bool ShowBorder
        {
            get { return showBorder; }
            set
            {
                showBorder = value;
                Invalidate();
            }
        }

        Color hotTrackingColor = SystemColors.HotTrack;
        [DefaultValue(typeof(Color), "HotTrack"), Category("Appearance")]
        public Color HotTrackingColor
        {
            get { return hotTrackingColor; }
            set { hotTrackingColor = value; }
        }

        bool showFocusRect = false;
        [DefaultValue(false), Category("List")]
        public bool ShowFocusRect
        {
            get { return showFocusRect; }
            set { showFocusRect = value; }
        }

        Color gridColor = Color.LightGray;
        [DefaultValue(typeof(Color), "LightGray"), Category("Appearance")]
        public Color GridColor
        {
            get { return gridColor; }
            set
            {
                gridColor = (Color)value;
                Invalidate();
            }
        }

        bool showGridLine = true;
        [DefaultValue(true), Category("Appearance"), Description("指示是否显示网格线")]
        public bool ShowGridLine
        {
            get { return showGridLine; }
            set
            {
                showGridLine = value;
                Invalidate();
            }
        }

        int itemHeight = 20;
        [DefaultValue(20), Category("Appearance")]
        public int ItemHeight
        {
            get { return itemHeight; }
            set
            {
                itemHeight = value;
                Invalidate();
            }
        }

        int headerHeight = 25;
        [DefaultValue(25), Category("Appearance")]
        public int HeaderHeight
        {
            get { return headerVisible ? headerHeight : 0; }
            set
            {
                headerHeight = value;
                Invalidate();
            }
        }

        bool headerVisible = true;
        [DefaultValue(true), Category("Appearance")]
        public bool HeaderVisible
        {
            get { return headerVisible; }
            set
            {
                headerVisible = value;
                Invalidate();
            }
        }

        BorderStyle borderStyle = BorderStyle.FixedSingle;
        [DefaultValue(BorderStyle.FixedSingle), Category("Appearance")]
        public BorderStyle BorderStyle
        {
            get { return borderStyle; }
            set
            {
                borderStyle = value;
                Invalidate();
            }
        }

        ColumnCollection columns;
        [Category("List"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ColumnCollection Columns
        {
            get { return columns; }
        }

        ListRowCollection rows;
        [Category("List"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ListRowCollection Rows
        {
            get { return rows; }
        }

        Color selectionColor = SystemColors.Highlight;
        [DefaultValue(typeof(Color), "Highlight"), Category("Appearance")]
        public Color SelectionColor
        {
            get { return selectionColor; }
            set { selectionColor = value; }
        }

        Color selectionTextColor = SystemColors.HighlightText;
        [DefaultValue(typeof(Color), "HighlightText"), Category("Appearance")]
        public Color SelectionTextColor
        {
            get { return selectionTextColor; }
            set { selectionTextColor = value; }
        }

        bool fullRowSelect = false;
        [DefaultValue(false), Category("Behavior")]
        public bool FullRowSelect
        {
            get { return fullRowSelect; }
            set { fullRowSelect = value; }
        }

        bool allowMultiSelect = false;
        [DefaultValue(false), Category("Behavior")]
        public bool AllowMultiSelect
        {
            get { return allowMultiSelect; }
            set { allowMultiSelect = value; }
        }

        bool hitHover = false;
        [DefaultValue(false), Category("Behavior")]
        public bool HitHover
        {
            get { return hitHover; }
            set { hitHover = value; }
        }

        [Browsable(false)]
        public ListRow[] SelectedRows
        {
            get { return Rows.SelectedItems.ToArray(); }
        }

        [Browsable(false)]
        public ListRow SelectedRow
        {
            get { return Rows.SelectedItems.FirstOrDefault(); }
        }

        [Browsable(false)]
        public int[] SelectedIndicies
        {
            get { return this.Rows.SelectedIndicies.ToArray(); }
        }

        [Browsable(false)]
        public ItemCell this[int col, int row]
        {
            get { return rows[row].Cells[col]; }
        }

        ListRow focusedItem = null;
        [Browsable(false)]
        public ListRow FocusedItem
        {
            get { return focusedItem; }
            set
            {
                if (focusedItem != value)
                {
                    focusedItem = value;
                    Invalidate();
                    this.SelectedIndexChanged.SafeCall(this, new ClickEventArgs(Rows.IndexOf(value), -1));
                }
            }
        }

        [Browsable(false)]
        protected int VisibleRowsCount
        {
            get { return (int)Math.Ceiling((ClientSize.Height * 1.0 - HeaderHeight) / ItemHeight); }
        }

        int BorderPadding
        {
            get { return ShowBorder ? 2 : 0; }
        }

        public event EventHandler<HotHitEventArgs> HotHitChanged;

        HitInfo lastHitInfo;
        int columnIndex;
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            mouseMoveHoverTimer.Once(SystemInformation.MouseHoverTime);
            if (e.Y < HeaderHeight)
            {
                var x = e.X + hScrollBar.Value;
                var index = columns.Scan(c => c.Width, (a, b) => a + b).IndexOf(v => Math.Abs(v - x) <= ResizeArrowPadding);
                //if (index != -1)
                //    this.Cursor = Cursors.SizeWE;
                //else
                //    this.Cursor = Cursors.Default;
            }
            else
                this.Cursor = Cursors.Default;
        }
        Point mouseDownPoint;
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            var hitInfo = GetHitInfoAt(e.Location);
            if (hitInfo.Type == HitType.Item)
            {

            }
            if (e.Button == MouseButtons.Left)
            {
                mouseDownPoint = e.Location;
            }
            else
            {
                mouseDownPoint = Point.Empty;
            }
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            var hitInfo = GetHitInfoAt(e.Location);
            if (hitInfo.Type == HitType.Item)
            {

            }
        }
        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            CloseEditor();
            var hitInfo = GetHitInfoAt(e.Location);
            if (hitInfo.Type == HitType.Item)
            {
                if (!allowMultiSelect || !Control.ModifierKeys.HasFlag(Keys.Control))
                    rows.ClearSelection();
                rows[hitInfo.RowIndex].Selected = true;
                rows[hitInfo.RowIndex].Cells[hitInfo.ColIndex].Selected = true;

            }
            else
            {
                rows.ClearSelection();
            }
            Invalidate();
        }
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            var hitInfo = GetHitInfoAt(e.Location);
            if (hitInfo.Type == HitType.Item)
            {
                CloseEditor();
                ActiveEditor(hitInfo.ColIndex, hitInfo.RowIndex);
            }
        }

        public void InvalidateCell(ItemCell cell)
        {
        }
        Point currentCellAddress = new Point(-1, -1);
        public Point CurrentCellAddress
        {
            get { return currentCellAddress; }
        }
        Control currentEditor;
        ItemCell editCell;
        void ActiveEditor(int col, int row)
        {
            if (!columns[col].Editable)
                return;
            currentEditor = columns[col].EditControl;
            var editorControl = currentEditor as IEditorControl;
            if (currentEditor != null && editorControl != null)
            {
                currentEditor.Disposed += currentEditor_Disposed;
                this.Controls.Add(currentEditor);
                currentEditor.Bounds = this[col, row].CellBounds;
                editCell = this[col, row];
                if (!editorControl.Load(rows[row], editCell, this))
                {
                    currentEditor.Dispose();
                    currentEditor = null;
                    editCell = null;
                    return;
                }
                editorControl.Value = editCell.Value;
                currentEditor.Show();
            }
        }

        void currentEditor_Disposed(object sender, EventArgs e)
        {
            this.Controls.Remove(currentEditor);
        }

        public void CloseEditor()
        {
            if (currentEditor != null)
            {
                if (currentEditor is IEditorControl)
                {
                    var editorControl = currentEditor as IEditorControl;
                    editorControl.Unload();
                    editCell.Value = editorControl.Value;
                }
                currentEditor.Dispose();
                currentEditor = null;
                editCell = null;
            }
        }
        void SelectItem(int colInd, int rowInd)
        {

        }
        void UnSelectItem()
        {
        }
        Point ToWorldPoint(Point pt)
        {
            pt.Offset(hScrollBar.Value, vScrollBar.Value);
            return pt;
        }

        Point ToViewPoint(Point pt)
        {
            pt.Offset(-hScrollBar.Value, -vScrollBar.Value);
            return pt;
        }

        public HitInfo GetHitInfoAt(Point viewPoint)
        {
            var pt = ToWorldPoint(viewPoint);
            var info = new HitInfo
            {
                Type = HitType.None,
                ColIndex = -1,
                RowIndex = -1
            };

            info.ColIndex = columns.Scan(c => c.Width, (a, b) => a + b).IndexOf(w => w >= pt.X);

            var index = (pt.Y - HeaderHeight) / itemHeight;
            if (pt.Y > HeaderHeight && 0 <= index && index < rows.Count)
            {
                info.RowIndex = index;
                if (info.ColIndex != -1)
                    info.Type = HitType.Item;
            }
            else if (pt.Y <= HeaderHeight && info.ColIndex != -1)
            {
                info.Type = HitType.Head;
            }
            return info;
        }
        bool paintSuspended = false;
        public void SuspendPaint()
        {
            paintSuspended = true;
        }
        public void ResumePaint()
        {
            paintSuspended = false;
            Invalidate();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (paintSuspended)
            {
                return;
            }
            Graphics g = e.Graphics;
            g.Clear(BackColor);

            g.TranslateTransform(-hScrollBar.Value, -vScrollBar.Value);

            DrawRows(g, e.ClipRectangle);
            if (ShowGridLine)
                DrawGridLines(g, e.ClipRectangle);

            //if (!DesignMode)
            //DrawHotHit(g, e.ClipRectangle);
            if (HeaderVisible)
                DrawHeader(g, e.ClipRectangle);

            if (borderStyle == BorderStyle.Fixed3D)
            {
                ControlPaint.DrawBorder3D(g, 0, 0, Width, Height, Border3DStyle.Sunken);
            }
            else if (borderStyle == BorderStyle.FixedSingle)
            {
                g.DrawRectangle(SystemPens.ActiveBorder, 0, 0, Width - 1, Height - 1);
            }

            base.OnPaint(e);
        }
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            CalcScroll();
        }
        Size InnerClientSize
        {
            get
            {
                var heigth = hScrollBar.Visible ? Height - hScrollBar.Height : Height;
                var width = vScrollBar.Visible ? Width - vScrollBar.Width : Width;
                return new Size(Math.Max(width, 0), Math.Max(heigth, 0));
            }
        }
        Rectangle InnerClientRect
        {
            get { return new Rectangle(Point.Empty, InnerClientSize); }
        }
        internal void CalcScroll()//重新计算滚动条的位置
        {
            var totalHeight = (rows.Count + 1) * itemHeight + HeaderHeight;

            if (columns.Width > ClientSize.Width && !hScrollBar.Visible)
            {
                hScrollBar.Show();
                hScrollBar.Value = 0;
            }
            else if (columns.Width <= ClientSize.Width && hScrollBar.Visible)
            {
                hScrollBar.Hide();
            }

            if (totalHeight > ClientSize.Height && !vScrollBar.Visible)
            {
                vScrollBar.Show();
                vScrollBar.Value = 0;
            }
            else if (totalHeight <= ClientSize.Height && vScrollBar.Visible)
            {
                vScrollBar.Hide();
            }

            var rectClient = InnerClientRect;

            if (vScrollBar.Visible == true)
            {
                vScrollBar.Top = rectClient.Y;
                vScrollBar.Left = rectClient.Right;
                vScrollBar.Height = rectClient.Height;
                vScrollBar.LargeChange = rectClient.Height;
                vScrollBar.Maximum = totalHeight;

                vScrollBar.Value = Math.Min(vScrollBar.Value, vScrollBar.Maximum - vScrollBar.LargeChange);
            }

            if (hScrollBar.Visible == true)
            {
                hScrollBar.Left = rectClient.Left;
                hScrollBar.Top = rectClient.Bottom;
                hScrollBar.Width = rectClient.Width;
                hScrollBar.LargeChange = rectClient.Width;
                hScrollBar.Maximum = columns.Width;

                hScrollBar.Value = Math.Min(hScrollBar.Value, hScrollBar.Maximum - hScrollBar.LargeChange);
            }

            hole.Visible = hScrollBar.Visible && vScrollBar.Visible;
            if (hole.Visible)
            {
                hole.Bounds = new Rectangle(vScrollBar.Left, hScrollBar.Top, vScrollBar.Width, hScrollBar.Height);
                hole.BackColor = BackColor;
            }
        }

        internal void RecalcBounds()
        {
            for (var i = 0; i < columns.Count; i++)
            {
                for (var j = 0; j < rows.Count; j++)
                {

                }
            }
        }

        void DrawHeader(Graphics g, Rectangle clipRect)
        {
            var headRect = new Rectangle(0, 0, ClientSize.Width, HeaderHeight);
            if (!headRect.IntersectsWith(clipRect))
                return;
            using (var gradient = new LinearGradientBrush(headRect, SystemColors.ButtonHighlight, SystemColors.ButtonFace, LinearGradientMode.Vertical))
            {
                g.FillRectangle(gradient, headRect);
            }
            columns.Scan(c => c.Width, (a, b) => a + b)
                .Merge(columns, (left, col) => new { Rect = new Rectangle(left, 0, col.Width, HeaderHeight), Col = col })
                .Where(v => v.Rect.IntersectsWith(clipRect))
                .ForEach(v => v.Col.DrawHeader(g, v.Rect));
        }

        void DrawRows(Graphics g, Rectangle clipRect)
        {
            var startIndex = vScrollBar.Value / itemHeight;
            var y = startIndex * itemHeight + HeaderHeight;
            var rectRow = new Rectangle(0, y, Columns.Width, itemHeight);

            for (var i = startIndex; i < rows.Count && rectRow.Top < ClientRectangle.Bottom; i++)
            {
                if (clipRect.IntersectsWith(rectRow))
                    DrawRow(g, rectRow, rows[i], i);
                rectRow.Offset(0, itemHeight);
            }
        }
        void DrawHotHit(Graphics g, Rectangle clipRect)
        {
            if (this.HitHover && lastHitInfo.ColIndex != -1 && lastHitInfo.ColIndex < Columns.Count)
            {
                var x = Columns.OfType<Column>().Take(lastHitInfo.ColIndex)
                    .Aggregate(-hScrollBar.Value, (i, col) => i + col.Width);

                using (var brush = new SolidBrush(HotTrackingColor))
                {
                    g.FillRectangle(brush, ClientRectangle.X, 0, Columns[lastHitInfo.ColIndex].Width + 1, ClientRectangle.Height);
                }
            }
        }
        void DrawRow(Graphics g, Rectangle rectRow, ListRow row, int index)
        {
            var backColor = AllowRowAlternate && (index % 2) != 0 ? AlternateBackColor : BackColor;
            if (backColor != BackColor)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, rectRow);
                }
            }
            var cellStyle = new CellPaintStyle() { Font = Font };
            if (row.Selected && FullRowSelect)
            {
                using (var selectionBack = new SolidBrush(SelectionColor))
                using (var selectionFore = new SolidBrush(SelectionColor))
                {
                    g.FillRectangle(selectionBack, rectRow);
                    cellStyle.BackBrush = selectionBack;
                    cellStyle.ForeBrush = selectionFore;
                    var left = 0;
                    for (var i = 0; i < columns.Count; i++)
                    {
                        var column = columns[i];
                        var cell = row.Cells[i];
                        var rect = new Rectangle(left, rectRow.Y, column.Width, rectRow.Height);
                        cell.CellBounds = rect;
                        column.DrawCell(g, cellStyle);
                        left += column.Width;
                    }
                }
            }
            else
            {
                using (var brush = new SolidBrush(SelectionColor))
                {
                    var x = -hScrollBar.Value;

                    for (var i = 0; i < columns.Count; i++)
                    {
                        var column = columns[i];
                        var cell = row.Cells[i];
                        var rect = new Rectangle(x, rectRow.Y, column.Width, rectRow.Height);
                        cell.CellBounds = rect;
                        if (0 < rect.Right || rect.Left < ClientRectangle.Right)
                        {
                            if (cell.Selected && !FullRowSelect)
                            {
                                g.FillRectangle(brush, rect);
                            }
                            //column.DrawCell(g, cell, BackColor, ForeColor);
                        }
                        x += column.Width;
                    }
                }
            }
        }

        void DrawGridLines(Graphics g, Rectangle clipRect)
        {
            using (Pen pen = new Pen(GridColor))
            {
                var vLineBottom = -vScrollBar.Value + HeaderHeight + rows.Count * itemHeight;

                if (vLineBottom > ClientRectangle.Bottom)
                    vLineBottom = ClientRectangle.Bottom;

                var left = Math.Max(0, clipRect.Left);
                var right = columns.Width - hScrollBar.Value;
                var y = HeaderHeight + itemHeight - (vScrollBar.Value) % itemHeight;
                while (y <= vLineBottom)
                {
                    g.DrawLine(pen, left, y, right, y);
                    y += itemHeight;
                }

                var vRight = -hScrollBar.Value;
                for (int i = 0; i < columns.Count; i++)
                {
                    vRight += columns[i].Width;
                    if (0 < vRight && vRight < ClientSize.Width)
                        g.DrawLine(pen, vRight, HeaderHeight, vRight, vLineBottom);
                }
            }
        }

        void OnScroll(object sender, ScrollEventArgs e)
        {
            CloseEditor();
            CalcScroll();
            Invalidate();
        }
    }

    public struct HitInfo
    {
        public HitType Type { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }

        public override int GetHashCode()
        {
            return (int)Type ^ RowIndex ^ ColIndex ^ 87;
        }
        public override bool Equals(object obj)
        {
            if (obj is HitInfo)
            {
                return (HitInfo)obj == this;
            }
            return false;
        }
        public static bool operator ==(HitInfo info1, HitInfo info2)
        {
            return info1.Type == info2.Type && info1.ColIndex == info2.ColIndex && info1.RowIndex == info2.RowIndex;
        }
        public static bool operator !=(HitInfo info1, HitInfo info2)
        {
            return !(info1 == info2);
        }
        public override string ToString()
        {
            return string.Format("type:{0} row:{1} col:{2}", Type, RowIndex, ColIndex);
        }
    }

    public enum HitType
    {
        None,
        Head,
        Item
    }
}
