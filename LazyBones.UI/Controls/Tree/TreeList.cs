using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Security.Permissions;
using System.Windows.Forms;
using LazyBones.Extensions;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeList : UserControl
    {
        const int ItemDragSensivity = 4;
        const int DividerWidth = 9;

        List<TreeListNode> selectedNodes = new List<TreeListNode>();
        ReadOnlyCollection<TreeListNode> readonlySelectedNodes;
        IList<NodeCell> nodeCells;
        Pen treeLinePen;
        Pen markPen;
        bool isDraging;
        bool _suspendUpdate;
        bool _structureUpdating;
        bool needFullUpdate;
        bool suspendUpdate = false;
        bool fireSelectionEvent;
        NodeCellExpandState expandStateNode = new NodeCellExpandState();
        Control currentEditor;
        NodeCell currentEditorOwner;
        TreeListNode hoverRow;
        NodeCell hoverCell;
        ToolTip toolTip;

        public TreeList()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint
                | ControlStyles.UserPaint
                | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw
                | ControlStyles.Selectable
                , true);
            AutoScroll = true;
            nodeCells = new NodeCellCollection(this);
            readonlySelectedNodes = new ReadOnlyCollection<TreeListNode>(selectedNodes);

            BorderStyle = BorderStyle.FixedSingle;
            BackColor = SystemColors.Window;
            visibleNodes = new List<TreeListNode>();
            columns = new TreeColumnCollection(this);
            toolTip = new ToolTip();

            SetInput<NormalInputState>();
            ClearNodes();
            CreatePens();

            expandStateNode = new NodeCellExpandState();
            SelectionMode = TreeSelectionMode.Single;
            columns = new TreeColumnCollection(this);
        }
        protected override Size DefaultSize
        {
            get { return new Size(100, 100); }
        }
        public void RegisterExpandStateNode<TNode>()
            where TNode : NodeCellExpandState, new()
        {
            expandStateNode = new TNode();
        }
        internal void UpdateColumnHeaders()
        {
            UpdateView();
        }
        internal void UpdateColumns()
        {
            FullUpdate();
        }
        internal void UpdateColumnWidth(TreeColumn column)
        {
            FullUpdate();
            OnColumnWidthChanged(column);
        }
        internal void FullUpdate()
        {
            RefreshNodes();
            UpdateColumnsIndex();
            UpdateView();
            needFullUpdate = false;
        }
        void UpdateColumnsIndex()
        {
            var i = 0;
            foreach (var col in columns.Where(c => c.Visible))
                col.DisplayIndex = i++;
        }
        internal void UpdateView()
        {
            if (!suspendUpdate)
                Invalidate(false);
        }
        public IDisposable GetUpdateHolder()
        {
            return new UpdateHolder(this);
        }

        InputState input;
        internal void ChangeInput()
        {
            if (ModifierKeys.HasFlag(Keys.Shift))
            {
                SetInput<InputWithShift>();
            }
            else if (ModifierKeys.HasFlag(Keys.Control))
            {
                SetInput<InputWithControl>();
            }
            else
            {
                SetInput<NormalInputState>();
            }
        }
        void SetInput<TInput>()
            where TInput : InputState, new()
        {
            if (input != null && input is TInput)
                return;
            input = new TInput() { Tree = this };
        }

        internal bool ItemDragMode;
        internal Point ItemDragStart;
        List<TreeListNode> rootNodes = new List<TreeListNode>();
        [Browsable(false)]
        public List<TreeListNode> RootNodes
        {
            get { return rootNodes; }
        }
        [Browsable(false)]
        public ReadOnlyCollection<TreeListNode> SelectedNodes
        {
            get { return readonlySelectedNodes; }
        }

        internal List<TreeListNode> SelectedNodesInternal
        {
            get { return selectedNodes; }
        }

        [DefaultValue(typeof(Color), "Window")]
        public override Color BackColor
        {
            get { return base.BackColor; }
            set { base.BackColor = value; }
        }
        TreeColumnCollection columns;
        [Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(LazyBones.UI.Controls.Tree.Designer.TreeColumnCollectionEditor), typeof(UITypeEditor))]
        public TreeColumnCollection Columns
        {
            get { return columns; }
        }

        [Browsable(false)]
        public TreeListNode SelectedNode
        {
            get
            {
                if (selectedNodes.Count > 0)
                {
                    if (CurrentNode != null && CurrentNode.IsSelected)
                        return CurrentNode;
                    else
                        return selectedNodes[0];
                }
                else
                    return null;
            }
            set
            {
                if (SelectedNode == value)
                    return;
                using (GetUpdateHolder())
                {
                    if (value == null)
                    {
                        ClearSelection();
                    }
                    else
                    {
                        if (!IsMyNode(value))
                            throw new ArgumentException();

                        ClearSelection();
                        value.IsSelected = true;
                        CurrentNode = value;
                        EnsureVisible(value);
                    }
                }
            }
        }
        internal int PageRowCount
        {
            get
            {
                return Math.Max((DisplayRectangle.Height - ColumnHeaderHeight) / rowHeight, 0);
            }
        }
        BorderStyle borderStyle;
        [DefaultValue(BorderStyle.FixedSingle), Category("Appearance")]
        public BorderStyle BorderStyle
        {
            get { return borderStyle; }
            set
            {
                if (borderStyle == value)
                    return;
                borderStyle = value;
                base.UpdateStyles();
            }
        }
        bool hideSelection = false;
        [DefaultValue(false), Category("Behavior")]
        public bool HideSelection
        {
            get { return hideSelection; }
            set
            {
                hideSelection = value;
                UpdateView();
            }
        }
        double edgeSensivityTop = 0.3;
        [DefaultValue(0.3), Category("Drap"), Description("拖动时上边缘比例")]
        public double EdgeSensivityTop
        {
            get { return edgeSensivityTop; }
            set
            {
                if (value < 0 || 0.5 < value)
                    throw new ArgumentOutOfRangeException("值应在0~0.5之间");
                edgeSensivityTop = value;
            }
        }
        double edgeSensivityBottom = 0.3;
        [DefaultValue(0.3), Category("Drap"), Description("拖动时下边缘比例")]
        public double EdgeSensivityBottom
        {
            get { return edgeSensivityBottom; }
            set
            {
                if (value < 0 || 0.5 < value)
                    throw new ArgumentOutOfRangeException("值应在0~0.5之间");
                edgeSensivityBottom = value;
            }
        }
        Color dragDropMarkColor = Color.Black;
        [DefaultValue(typeof(Color), "Black"), Category("Drap"), Description("拖动时指示线的颜色")]
        public Color DragDropMarkColor
        {
            get { return dragDropMarkColor; }
            set
            {
                dragDropMarkColor = value;
                CreateMarkPen();
            }
        }
        int dragDropMarkWidth = 2;
        [DefaultValue(2), Category("Drap"), Description("拖动时指示线的宽度")]
        public int DragDropMarkWidth
        {
            get { return dragDropMarkWidth; }
            set
            {
                dragDropMarkWidth = value;
                CreateMarkPen();
            }
        }

        int indent = 16;
        [DefaultValue(16), Category("Appearance"), Description("缩进大小")]
        public int Indent
        {
            get { return indent; }
            set
            {
                indent = value;
                UpdateView();
            }
        }
        DropInfo dropPosition;
        [Browsable(false)]
        public DropInfo DropPosition
        {
            get { return dropPosition; }
            set { dropPosition = value; }
        }

        Color treeLineColor = SystemColors.ControlDark;
        [DefaultValue(typeof(Color), "ControlDark"), Category("Appearance"), Description("连接节点线条的颜色")]
        public Color TreeLineColor
        {
            get { return treeLineColor; }
            set
            {
                treeLineColor = value;
                CreateLinePen();
                UpdateView();
            }
        }


        [DefaultValue(TreeSelectionMode.Single), Category("Behavior")]
        public TreeSelectionMode SelectionMode { get; set; }


        [Category("Behavior"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Editor(typeof(NodeControlCollectionEditor), typeof(UITypeEditor))]
        public IList<NodeCell> NodeControls
        {
            get { return nodeCells; }
        }
        IEnumerable<TreeListNode> ExpandedRows
        {
            get { return rootNodes.Concat(rootNodes.SelectMany(r => r.AllExpandedChildNodes)); }
        }
        void ClearNodes()
        {
            selectedNodes.Clear();
            SelectionStart = null;
            rootNodes.Clear();
            CurrentNode = null;
        }
        void CreateNodes()
        {
            visibleNodes.AddRange(rootNodes);
            visibleNodes.AddRange(rootNodes.SelectMany(r => r.AllExpandedChildNodes));
            
            for (var i = 0; i < visibleNodes.Count; i++)
                visibleNodes[i].RowIndex = i;
            var height = visibleNodes.Count * rowHeight;
            AutoScrollMinSize = new Size(ContentWidth, height);
        }
        void RefreshNodes()
        {
            ClearNodes();
            CreateNodes();
        }
        int ContentWidth
        {
            get
            {
                var width = 0;
                if (UseColumns)
                    width = columns.Where(c => c.Visible).Sum(c => c.Width);
                else if (visibleNodes.Count > 0)
                    width = visibleNodes.Select(GetRowBounds).Max(r => r.Right);
                width = Math.Max(width, ClientSize.Width);
                return width;
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                treeLinePen.SafeDispose();
                markPen.SafeDispose();
            }
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!DesignMode)
            {
                LoadData();
                LoadRootNodes();
                UpdateView();
            }
        }
        void CreatePens()
        {
            CreateLinePen();
            CreateMarkPen();
        }
        void CreateLinePen()
        {
            treeLinePen.SafeDispose();
            treeLinePen = new Pen(treeLineColor) { DashStyle = DashStyle.Dot };
        }
        void CreateMarkPen()
        {
            markPen.SafeDispose();
            markPen = new Pen(dragDropMarkColor, dragDropMarkWidth);
        }
        protected override CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                CreateParams res = base.CreateParams;
                switch (BorderStyle)
                {
                    case BorderStyle.FixedSingle:
                        res.Style |= 0x800000;
                        break;
                    case BorderStyle.Fixed3D:
                        res.ExStyle |= 0x200;
                        break;
                }
                return res;
            }
        }
        public TreeListNode GetNodeAt(Point point)
        {
            var row = ToAbsoluteLocation(point).Y / RowHeight;
            if (0 <= row && row < visibleNodes.Count)
            {
                var info = GetNodeCellInfoAt(visibleNodes[row], point);
                if (info.Cell != null)
                    return visibleNodes[row];
            }
            return null;
        }

        Point ToAbsoluteLocation(Point viewPoint)
        {
            return new Point(viewPoint.X - AutoScrollPosition.X, viewPoint.Y - AutoScrollPosition.Y - ColumnHeaderHeight);
        }

        Point ToViewLocation(Point absolutePoint)
        {
            return new Point(absolutePoint.X + AutoScrollPosition.X, absolutePoint.Y + AutoScrollPosition.Y + ColumnHeaderHeight);
        }
        NodeCellInfo GetNodeCellInfoAt(TreeListNode node, Point point)
        {
            return GetNodeCellInfos(node).FirstOrDefault(info => info.CellBounds.Contains(point));
        }
        Rectangle GetRowBounds(TreeListNode node)
        {
            return GetNodeCellInfos(node)
                .Aggregate(Rectangle.Empty, (b, n) => b.IsEmpty ? n.CellBounds : Rectangle.Union(b, n.CellBounds));
        }
        IEnumerable<NodeCellInfo> GetNodeCellInfos(TreeListNode node)
        {
            if (node == null)
                yield break;

            int top = node.RowIndex * rowHeight;
            int left = node.Level * indent;

            Rectangle rect = new Rectangle(left, top, expandStateNode.MeasureSize(node).Width, RowHeight);
            if (ColumnHeaderVisible && Columns[0].Width < rect.Right)
                rect.Width = Columns[0].Width - left;
            yield return new NodeCellInfo(expandStateNode, rect);

            if (UseColumns)
            {
                int right = 0;
                foreach (var item in nodeCells.Select((c, i) => new { Ctrl = c, Col = columns[i] }).Where(c => c.Col.Visible))
                {
                    right += item.Col.Width;
                    var width = right - left;
                    width = Math.Max(0, width);
                    rect.X = rect.Right + 1;
                    rect.Width = width;
                    yield return new NodeCellInfo(item.Ctrl, rect);
                    left = right;
                }
            }
            else
            {
                foreach (var c in nodeCells)
                {
                    var width = c.MeasureSize(node).Width;
                    rect.X = rect.Right + 1;
                    rect.Width = width;
                    yield return new NodeCellInfo(c, rect);
                }
            }
        }
        List<TreeListNode> visibleNodes;
        internal List<TreeListNode> VisibleNodes
        {
            get { return visibleNodes; }
        }
        internal TreeListNode SelectionStart { get; set; }
        internal bool ColumnHeaderVisible
        {
            get { return UseColumns && columns.Count > 0; }
        }
        int columnHeaderHeight = 20;
        [DefaultValue(20), Category("Appearance")]
        public int ColumnHeaderHeight
        {
            get { return ColumnHeaderVisible ? columnHeaderHeight : 0; }
            set
            {
                if (ColumnHeaderVisible)
                {
                    if (value <= 0 || value > 50)
                        throw new ArgumentOutOfRangeException("值不得小于0或大于50");
                    columnHeaderHeight = value;
                    FullUpdate();
                }
            }
        }
        int rowHeight = 16;
        [DefaultValue(16), Category("Appearance")]
        public int RowHeight
        {
            get { return rowHeight; }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException("值不得小于0");
                rowHeight = value;
                FullUpdate();
            }
        }

        bool useColumns = false;
        [DefaultValue(false), Category("Behavior")]
        public bool UseColumns
        {
            get { return useColumns; }
            set
            {
                useColumns = value;
                FullUpdate();
            }
        }

        bool treeLineVisible = true;
        [DefaultValue(true), Category("Behavior"), Description("是否显示连接节点的线")]
        public bool TreeLineVisible
        {
            get { return treeLineVisible; }
            set
            {
                treeLineVisible = value;
                UpdateView();
            }
        }

        [DefaultValue(false), Category("Behavior"), Description("是否显示提示信息")]
        public bool ToolTipsVisible { get; set; }

        bool keepNodesExpanded;
        [DefaultValue(false), Category("Behavior")]
        public bool KeepNodesExpanded
        {
            get { return keepNodesExpanded; }
            set { keepNodesExpanded = value; }
        }

        public TreeListNode CurrentNode { get; internal set; }

        public void ExpandAll()
        {
            using (GetUpdateHolder())
            {
                rootNodes.ForEach(r => r.ExpandAll());
            }
        }
        public void CollapseAll()
        {
            using (GetUpdateHolder())
            {
                rootNodes.ForEach(r => r.CollapseAll());
            }
        }
        public void EnsureVisible(TreeListNode node)
        {
            if (node == null)
                throw new ArgumentNullException("node");

            if (!IsMyNode(node))
                throw new ArgumentException("node不属于该TreeList");

            var parent = node.ParentNode;
            while (parent.Level > 0)
            {
                parent.IsExpanded = true;
                parent = parent.ParentNode;
            }
            ScrollTo(node);
        }
        public void ScrollTo(int displayNodeIndex)
        {
            if (0 <= displayNodeIndex && displayNodeIndex < visibleNodes.Count)
            {
                ScrollTo(visibleNodes[displayNodeIndex]);
            }
        }
        public void ScrollTo(TreeListNode node)
        {
            ParamGuard.NotNull(node, "node");
            if (!IsMyNode(node))
                throw new ArgumentException("node不属于该TreeList");

            if (node.RowIndex < 0)
                RefreshNodes();

            if (node.RowIndex < -AutoScrollPosition.X)
                AutoScrollPosition = new Point(AutoScrollPosition.X, node.RowIndex);
            else if (node.RowIndex > -AutoScrollPosition.X + (PageRowCount - 1))
                AutoScrollPosition = new Point(AutoScrollPosition.X, -node.RowIndex + (PageRowCount - 1));
        }
        internal bool IsMyNode(TreeListNode node)
        {
            return (node != null && node.Tree == this);
        }
        public void ClearSelection()
        {
            while (selectedNodes.Count > 0)
                selectedNodes[0].IsSelected = false;
        }
        internal void SmartFullUpdate()
        {
            if (_suspendUpdate || _structureUpdating)
                needFullUpdate = true;
            else
                FullUpdate();
        }

        void AddNode(TreeListNode parent, object value, int index)
        {
            AddNode(parent, value, index, null);
        }

        void AddNode(TreeListNode parent, object value, int index, IEnumerable<ExpandedNode> expandedChildren)
        {
            var node = new TreeListNode(this, value) { ParentNode = parent };

            if (index >= 0 && index < parent.ChildNodes.Count)
                parent.ChildNodes.Insert(index, node);
            else
                parent.ChildNodes.Add(node);

            //node.IsLeaf = Model.IsLeaf(parent.Path);
            //if (!LoadOnDemand)
            //    ReadChildren(node);
            //else if (expandedChildren != null)
            //{
            //    ReadChildren(node, expandedChildren);
            //    node.IsExpanded = true;
            //}
        }

        [DefaultValue(false), Category("Behavior")]
        public bool LoadOnDemand { get; set; }

        object dataSource;
        [DefaultValue(null), Category("Data"),
        AttributeProvider(typeof(IListSource)),
        RefreshProperties(RefreshProperties.Repaint)]
        public object DataSource
        {
            get { return dataSource; }
            set
            {
                if (value != null && !(value is IList || value is IListSource))
                    throw new ArgumentException();
                if (dataSource != value)
                {
                    dataSource = value;
                    KeyField = null;
                    ParentField = null;
                    UpdateDataSource();
                }
            }
        }
        void UpdateDataSource()
        {
            UnwireDataSource();
            WireDataSource();

        }

        CurrencyManager CurrencyManager
        {
            get
            {
                if (DataSource != null && this.BindingContext != null)
                {
                    return (this.BindingContext[DataSource] as CurrencyManager);
                }
                return null;
            }
        }

        CurrencyManager dataManager;
        CurrencyManager DataManager
        {
            get { return dataManager; }
            set
            {
                if (dataManager == value)
                    return;
                if (dataManager != null)
                {
                    dataManager.ItemChanged -= new ItemChangedEventHandler(DataManager_ItemChanged);
                    dataManager.PositionChanged -= new EventHandler(DataManager_PositionChanged);
                }

                dataManager = value;

                if (dataManager != null)
                {
                    dataManager.ItemChanged += new ItemChangedEventHandler(DataManager_ItemChanged);
                    dataManager.PositionChanged += new EventHandler(DataManager_PositionChanged);
                }
            }
        }

        PropertyDescriptor keyDescriptor;
        [DefaultValue((string)null), Category("Data")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", typeof(UITypeEditor))]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design")]
        public string KeyField
        {
            get { return keyDescriptor == null ? null : keyDescriptor.DisplayName; }
            set
            {
                if (keyDescriptor != null && keyDescriptor.Name == value)
                    return;
                keyDescriptor = GetFieldProperty(value);
            }
        }

        PropertyDescriptor parentDescriptor;
        [DefaultValue((string)null), Category("Data")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", typeof(UITypeEditor))]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design")]
        public string ParentField
        {
            get { return parentDescriptor == null ? null : parentDescriptor.Name; }
            set
            {
                if (parentDescriptor != null && parentDescriptor.Name == value)
                    return;
                parentDescriptor = GetFieldProperty(value);
            }
        }

        internal PropertyDescriptor GetFieldProperty(string propertyName)
        {
            if (DataManager != null && propertyName != null)
            {
                var property = DataManager.GetItemProperties()[propertyName];
                if (property == null)
                    throw new ArgumentException("绑定数据源不存在字段:" + propertyName);
                return property;
            }
            return null;
        }

        [DefaultValue(null), Category("Data")]
        public object RootValue { get; set; }

        void DataManager_PositionChanged(object sender, EventArgs e)
        {
            ScrollTo(dataManager.Position);
        }
        void DataManager_ItemChanged(object sender, ItemChangedEventArgs e)
        {
            if (dataManager != null)
            {
                //if (e.Index == -1)
                //{
                //    SetItemsCore(dataManager.List);
                //    if (AllowSelection)
                //    {
                //        this.SelectedIndex = this.dataManager.Position;
                //    }
                //}
                //else
                //{
                //    SetItemCore(e.Index, dataManager[e.Index]);
                //}
            }
        }

        void UnwireDataSource()
        {
            if (this.dataSource is IComponent)
            {
                ((IComponent)this.dataSource).Disposed -= new EventHandler(DataSourceDisposed);
            }
            var dsInit = (this.dataSource as ISupportInitializeNotification);
            if (dsInit != null)
            {
                dsInit.Initialized -= DataSourceInitialized;
            }
            DataManager = null;
        }

        void WireDataSource()
        {
            if (this.dataSource is IComponent)
            {
                ((IComponent)this.dataSource).Disposed += new EventHandler(DataSourceDisposed);
            }
            var dsInit = (this.dataSource as ISupportInitializeNotification);
            var isDataSourceInitialized = (dsInit == null || dsInit.IsInitialized);
            if (isDataSourceInitialized)
            {
                DataManager = CurrencyManager;
            }
            else
            {
                dsInit.Initialized += DataSourceInitialized;
            }
        }

        void DataSourceDisposed(object sender, EventArgs e)
        {
            UnwireDataSource();
        }

        void DataSourceInitialized(object sender, EventArgs e)
        {
            WireDataSource();
        }

        IList DataList
        {
            get
            {
                if (dataSource == null)
                    return new object[0];
                if (dataSource is IList)
                    return dataSource as IList;
                else if (dataSource is IListSource)
                    return (dataSource as IListSource).GetList();
                else
                    throw new ArgumentException("数据源不实现IList或IListSource接口");
            }
        }

        Dictionary<object, object> dataCache = new Dictionary<object, object>();
        Dictionary<object, List<object>> childCache = new Dictionary<object, List<object>>();
        void LoadData()
        {
            dataCache.Clear();
            childCache.Clear();
            foreach (var d in DataList)
            {
                var key = GetKeyValue(d);
                try
                {
                    dataCache.Add(key, d);
                }
                catch (ArgumentException)
                {
                    throw new InvalidOperationException("数据源中存在相同键值的项目");
                }
                var parent = GetParentValue(d);
                List<object> child;
                if (childCache.TryGetValue(parent, out child))
                {
                    child.Add(d);
                }
                else
                {
                    childCache.Add(parent, new List<object> { d });
                }
            }
        }
        void LoadRootNodes()
        {
            rootNodes.Clear();
            List<object> values;
            if (childCache.TryGetValue(RootValue, out values))
            {
                foreach (var v in values)
                {
                    rootNodes.Add(new TreeListNode(this, v));
                }
            }
        }
        object GetKeyValue(object value)
        {
            if (keyDescriptor == null)
                return value;
            return keyDescriptor.GetValue(value);
        }
        object GetParentValue(object value)
        {
            if (parentDescriptor == null)
                return value;
            return parentDescriptor.GetValue(value);
        }
        internal void LoadChildNodes(TreeListNode node)
        {
            node.HasChildLoaded = true;
            node.ChildNodes.Clear();
            foreach (var d in DataList)
            {
                var parent = GetParentValue(d);
                if (object.Equals(RootValue, parent))
                {
                    rootNodes.Add(new TreeListNode(this, d));
                }
            }
        }
        protected override void OnGotFocus(EventArgs e)
        {
            DisposeEditor();
            UpdateView();
            ChangeInput();
            base.OnGotFocus(e);
        }
        protected override void OnLeave(EventArgs e)
        {
            DisposeEditor();
            UpdateView();
            base.OnLeave(e);
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Console.WriteLine("OnScroll");
            Invalidate(true);
        }

        TreeListNodeMouseEventArgs CreateMouseArgs(MouseEventArgs e)
        {
            var args = new TreeListNodeMouseEventArgs(e);
            args.AbsoluteLocation = ToAbsoluteLocation(e.Location);
            args.Node = GetNodeAt(e.Location);
            var info = GetNodeCellInfoAt(args.Node, args.AbsoluteLocation);
            args.CellBounds = info.CellBounds;
            args.Cell = info.Cell;
            return args;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (VScroll || HScroll)
                Invalidate(false);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (!Focused)
                Focus();
            //先判断是否在列头部位点击
            if (e.Button == MouseButtons.Left)
            {
                var col = GetColumnDividerAt(e.Location);
                if (col != null)
                {
                    SetInput<ResizeColumnState>();
                    return;
                }
            }

            ChangeInput();
            var args = CreateMouseArgs(e);

            if (args.Node != null && args.Cell != null)
                args.Cell.MouseDown(args);

            if (!args.Handled)
                input.MouseDown(args);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            var args = CreateMouseArgs(e);
            if (input is ResizeColumnState)
            {
                input.MouseUp(args);
            }
            else
            {
                base.OnMouseUp(e);
                if (args.Node != null && args.Cell != null)
                    args.Cell.MouseUp(args);
                if (!args.Handled)
                    input.MouseUp(args);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            var args = CreateMouseArgs(e);
            if (args.Node != null)
            {
                OnNodeMouseDoubleClick(args);
                if (args.Handled)
                    return;
            }

            if (args.Node != null && args.Cell != null)
                args.Cell.MouseDoubleClick(args);
            if (!args.Handled)
            {
                if (args.Node != null && args.Button == MouseButtons.Left)
                    args.Node.IsExpanded = !args.Node.IsExpanded;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (input.MouseMove(e))
                return;

            base.OnMouseMove(e);
            SetCursor(e);
            if (e.Location.Y <= ColumnHeaderHeight)
            {
                toolTip.Active = false;
            }
            else
            {
                UpdateToolTip(e);
                if (ItemDragMode && Dist(e.Location, ItemDragStart) > ItemDragSensivity && CurrentNode != null && CurrentNode.IsSelected)
                {
                    ItemDragMode = false;
                    toolTip.Active = false;
                    OnItemDrag(e.Button, selectedNodes.ToArray());
                }
            }
        }
        static double Dist(Point p1, Point p2)
        {
            return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
        }
        void SetCursor(MouseEventArgs e)
        {
            if (GetColumnDividerAt(e.Location) == null)
                this.Cursor = Cursors.Default;
            else
                this.Cursor = Cursors.VSplit;
        }
        TreeColumn GetColumnDividerAt(Point p)
        {
            if (p.Y > ColumnHeaderHeight)
                return null;
            var rect = new Rectangle(AutoScrollPosition.X, 0, 0, ColumnHeaderHeight);
            foreach (var col in columns.Where(c => c.Visible))
            {
                rect.X += col.Width;
                if (Rectangle.Inflate(rect, DividerWidth / 2, 0).Contains(p))
                    return col;
            }
            return null;
        }

        void UpdateToolTip(MouseEventArgs e)
        {
            if (ToolTipsVisible)
            {
                var args = CreateMouseArgs(e);
                try
                {
                    if (args.Node != null && (args.Node != hoverRow || args.Cell != hoverCell))
                    {
                        var toolTipsArgs = new ToolTipsEventArgs(args.Node, args.Cell);
                        OnToolTipsNeeded(toolTipsArgs);
                        var msg = toolTipsArgs.Handled ? toolTipsArgs.ToolTips : args.Cell.GetToolTip(args.Node);
                        if (string.IsNullOrEmpty(msg))
                            return;
                        toolTip.SetToolTip(this, msg);
                        toolTip.Active = true;
                    }
                }
                finally
                {
                    hoverCell = args.Cell;
                    hoverRow = args.Node;
                }
            }
            else
            {
                toolTip.SetToolTip(this, null);
            }
        }

        protected override void OnDragOver(DragEventArgs drgevent)
        {
            ItemDragMode = false;
            isDraging = true;
            Point pt = PointToClient(new Point(drgevent.X, drgevent.Y));
            SetDropPosition(pt);
            UpdateView();
            base.OnDragOver(drgevent);
        }
        protected override void OnDragLeave(EventArgs e)
        {
            isDraging = false;
            UpdateView();
            base.OnDragLeave(e);
        }
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            isDraging = false;
            UpdateView();
            base.OnDragDrop(drgevent);
        }
        void SetDropPosition(Point pt)
        {
            var row = GetNodeAt(pt);
            pt = ToAbsoluteLocation(pt);
            dropPosition.Row = row;
            if (row != null)
            {
                var pos = (pt.Y - row.RowIndex * RowHeight) * 1.0 / RowHeight;
                if (pos < EdgeSensivityTop)
                    dropPosition.Position = DropPos.BeforeNode;
                else if (pos > (1 - EdgeSensivityBottom))
                    dropPosition.Position = DropPos.AfterNode;
                else
                    dropPosition.Position = DropPos.InsideNode;
            }
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData.HasFlag(Keys.Up) || keyData.HasFlag(Keys.Down)
                || keyData.HasFlag(Keys.Left) || keyData.HasFlag(Keys.Right))
                return true;
            else
                return base.IsInputKey(keyData);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var context = new DrawContext
            {
                Graphics = e.Graphics,
                Font = this.Font,
                Enabled = Enabled
            };
            if (ColumnHeaderVisible)
                DrawColumnHeaders(e.Graphics);

            e.Graphics.TranslateTransform(AutoScrollPosition.X, AutoScrollPosition.Y - ColumnHeaderHeight);
            for (int rowIndex = -AutoScrollPosition.Y, i = 0; rowIndex < visibleNodes.Count && i <= PageRowCount; rowIndex++, i++)
            {
                var node = visibleNodes[rowIndex];
                context.DrawSelection = DrawSelectionMode.None;
                context.CurrentEditorOwner = currentEditorOwner;
                if (isDraging)
                {
                    if ((DropPosition.Row == node) && DropPosition.Position == DropPos.InsideNode)
                        context.DrawSelection = DrawSelectionMode.Active;
                }
                else
                {
                    if (node.IsSelected && Focused)
                        context.DrawSelection = DrawSelectionMode.Active;
                    else if (node.IsSelected && !Focused && !HideSelection)
                        context.DrawSelection = DrawSelectionMode.InActive;
                }
                context.DrawFocus = (Focused && CurrentNode == node);

                if (context.DrawSelection == DrawSelectionMode.Active || context.DrawSelection == DrawSelectionMode.InActive)
                {
                    var focusRect = new Rectangle(-AutoScrollPosition.X, rowIndex * RowHeight, ClientSize.Width, RowHeight);
                    if (context.DrawSelection == DrawSelectionMode.Active)
                    {
                        e.Graphics.FillRectangle(SystemBrushes.Highlight, focusRect);
                        context.DrawSelection = DrawSelectionMode.FullRowSelect;
                    }
                    else
                    {
                        e.Graphics.FillRectangle(SystemBrushes.InactiveBorder, focusRect);
                        context.DrawSelection = DrawSelectionMode.None;
                    }
                }

                if (TreeLineVisible)
                    DrawTreeLines(e.Graphics, node);

                DrawRow(node, context);
            }

            if (DropPosition.Row != null && isDraging)
                DrawDropMark(e.Graphics, DropPosition.Row, DropPosition.Position);

            e.Graphics.ResetTransform();
        }

        protected virtual void DrawColumnHeaders(Graphics g)//绘制列头
        {
            DrawHeaderBackground(g, new Rectangle(0, 0, ClientSize.Width, columnHeaderHeight));
            g.TranslateTransform(AutoScrollPosition.X, 0);
            var x = 0;
            foreach (var col in Columns.Where(c => c.Visible))
            {
                var rect = new Rectangle(x, 0, col.Width, columnHeaderHeight);
                x += col.Width;
                DrawHeaderBackground(g, rect);
                col.DrawHeader(g, rect, Font);
            }
            g.ResetTransform();
        }

        void DrawHeaderBackground(Graphics g, Rectangle rect)//绘制列头的背景
        {
            g.FillRectangle(SystemBrushes.Control, rect);
            g.DrawLine(SystemPens.ControlDark, rect.X, rect.Bottom - 1, rect.Right - 1, rect.Bottom - 1);
            g.DrawLine(SystemPens.ControlDark, rect.Right - 1, rect.Top - 1, rect.Right - 1, rect.Bottom - 1);
        }

        void DrawRow(TreeListNode node, DrawContext context)
        {
            foreach (var item in GetNodeCellInfos(node))
            {
                context.Bounds = item.CellBounds;
                context.Graphics.SetClip(item.CellBounds);
                try
                {
                    item.Cell.Draw(node, context);
                }
                finally
                {
                    context.Graphics.ResetClip();
                }
            }
        }

        void DrawDropMark(Graphics gr, TreeListNode row, DropPos dropPos)//绘制拖放时的横线
        {
            if (dropPos == DropPos.InsideNode)
                return;
            var rect = GetRowBounds(row);
            var right = DisplayRectangle.Right + AutoScrollPosition.X;
            var y = dropPos == DropPos.BeforeNode ? rect.Top : rect.Bottom;
            gr.DrawLine(markPen, rect.X, y, right, y);
        }

        void DrawTreeLines(Graphics g, TreeListNode node)//treeline只在第一列中显示
        {
            if (ColumnHeaderVisible)
                g.SetClip(new Rectangle(0, ColumnHeaderHeight, Columns[0].Width, AutoScrollMinSize.Height));

            var x = node.Level * indent + expandStateNode.ImageSize / 2;
            var width = expandStateNode.Width - expandStateNode.ImageSize / 2;
            var y = node.RowIndex * RowHeight;

            g.DrawLine(treeLinePen, x, y + RowHeight / 2, x + width, y + RowHeight / 2);

            if (node.RowIndex == 0)
                y += RowHeight / 2;
            var next = node.NextNode;
            if (next != null)
                g.DrawLine(treeLinePen, x, y, x, next.RowIndex * RowHeight);
            else
                g.DrawLine(treeLinePen, x, y, x, node.RowIndex * RowHeight + RowHeight / 2);

            g.ResetClip();
        }

        bool suspendSelectionEvent;
        internal bool SuspendSelectionEvent
        {
            get { return suspendSelectionEvent; }
            set
            {
                suspendSelectionEvent = value;
                if (!suspendSelectionEvent && fireSelectionEvent)
                    OnSelectionChanged();
            }
        }

        [Category("Behavior")]
        public event EventHandler SelectionChanged;
        internal void OnSelectionChanged()
        {
            if (SuspendSelectionEvent)
                fireSelectionEvent = true;
            else
            {
                fireSelectionEvent = false;
                var handle = SelectionChanged;
                if (handle != null)
                    handle(this, EventArgs.Empty);
            }
        }

        [Category("Behavior"), Description("节点折叠前发生")]
        public event EventHandler<TreeListNodeEventArgs> NodeCollapsing;
        internal void OnNodeCollapsing(TreeListNode node)
        {
            var handle = NodeCollapsing;
            if (handle != null)
                handle(this, new TreeListNodeEventArgs(node));
        }

        [Category("Behavior"), Description("节点折叠后发生")]
        public event EventHandler<TreeListNodeEventArgs> NodeCollapsed;
        internal void OnNodeCollapsed(TreeListNode node)
        {
            var handle = NodeCollapsed;
            if (handle != null)
                handle(this, new TreeListNodeEventArgs(node));
        }

        [Category("Behavior"), Description("节点展开前发生")]
        public event EventHandler<TreeListNodeEventArgs> NodeExpanding;
        internal void OnNodeExpanding(TreeListNode node)
        {
            var handle = NodeExpanding;
            if (handle != null)
                handle(this, new TreeListNodeEventArgs(node));
        }

        [Category("Behavior"), Description("节点展开后发生")]
        public event EventHandler<TreeListNodeEventArgs> NodeExpanded;
        internal void OnNodeExpanded(TreeListNode node)
        {
            var handle = NodeExpanded;
            if (handle != null)
                handle(this, new TreeListNodeEventArgs(node));
        }

        [Category("Action")]
        public event ItemDragEventHandler ItemDrag;
        private void OnItemDrag(MouseButtons buttons, object item)
        {
            var handle = ItemDrag;
            if (handle != null)
                handle(this, new ItemDragEventArgs(buttons, item));
        }

        [Category("Behavior")]
        public event EventHandler<TreeListNodeMouseEventArgs> NodeMouseDoubleClick;
        private void OnNodeMouseDoubleClick(TreeListNodeMouseEventArgs args)
        {
            var handle = NodeMouseDoubleClick;
            if (handle != null)
                handle(this, args);
        }

        [Category("Behavior")]
        public event EventHandler<TreeColumnEventArgs> ColumnWidthChanged;
        internal void OnColumnWidthChanged(TreeColumn column)
        {
            var handle = ColumnWidthChanged;
            if (handle != null)
                handle(this, new TreeColumnEventArgs(column));
        }

        public event EventHandler<ToolTipsEventArgs> ToolTipNeeded;
        void OnToolTipsNeeded(ToolTipsEventArgs args)
        {
            var handle = ToolTipNeeded;
            if (handle != null)
                handle(this, args);
        }

        internal void DisplayEditor(Control control, TreeColumnEditable owner)
        {
            if (control == null || owner == null)
                throw new ArgumentNullException();

            if (CurrentNode != null)
            {
                DisposeEditor();
                EditorContext context = new EditorContext();
                //context.Owner = owner;
                context.CurrentNode = CurrentNode;
                context.Editor = control;

                SetEditorBounds(context);

                currentEditor = control;
                //currentEditorOwner = owner;
                UpdateView();
                control.Parent = this;
                control.Focus();
                owner.UpdateEditor(control);
            }
        }

        void SetEditorBounds(EditorContext context)
        {
            var info = GetNodeCellInfos(context.CurrentNode)
                .FirstOrDefault(i => context.Owner == i.Cell && i.Cell is NodeCellEditable);
            if (!info.IsEmpty)
            {
                Point p = ToViewLocation(info.CellBounds.Location);
                int width = DisplayRectangle.Width - p.X;
                if (ColumnHeaderVisible && info.Cell.ColumnIndex < Columns.Count)
                {
                    var rect = GetColumnBounds(info.Cell.ColumnIndex);
                    width = rect.Right - AutoScrollPosition.X - p.X;
                }
                context.Bounds = new Rectangle(p.X, p.Y, width, info.CellBounds.Height);
                ((NodeCellEditable)info.Cell).SetEditorBounds(context);
            }
        }

        Rectangle GetColumnBounds(int displayIndex)
        {
            int x = 0;
            for (int i = 0; i < columns.Count; i++)
            {
                if (Columns[i].Visible)
                {
                    if (i < displayIndex)
                        x += Columns[i].Width;
                    else
                        return new Rectangle(x, 0, Columns[i].Width, 0);
                }
            }
            return Rectangle.Empty;
        }

        public void HideEditor()
        {
            if (currentEditor == null)
                return;
            this.Focus();
            DisposeEditor();
        }

        public void UpdateEditorBounds()
        {
            if (currentEditor != null)
            {
                EditorContext context = new EditorContext();
                context.Owner = currentEditorOwner;
                context.CurrentNode = CurrentNode;
                context.Editor = currentEditor;
                SetEditorBounds(context);
            }
        }

        private void DisposeEditor()
        {
            if (currentEditor != null)
                currentEditor.Parent = null;
            currentEditor.SafeDispose();
            currentEditor = null;
            currentEditorOwner = null;
        }
        struct NodeCellInfo
        {
            public NodeCell Cell;
            public Rectangle CellBounds;
            public NodeCellInfo(NodeCell cell, Rectangle cellBounds)
            {
                Cell = cell;
                CellBounds = cellBounds;
            }
            public bool IsEmpty
            {
                get { return Cell == null && CellBounds.IsEmpty; }
            }
        }
        class ExpandedNode
        {
            public object Value { get; set; }
            public List<ExpandedNode> Children { get; set; }
        }
        class UpdateHolder : IDisposable
        {
            TreeList tree;
            public UpdateHolder(TreeList tree)
            {
                this.tree = tree;
                tree.suspendUpdate = true;
            }
            public void Dispose()
            {
                tree.suspendUpdate = false;
                if (tree.needFullUpdate)
                    tree.FullUpdate();
                else
                    tree.UpdateView();
            }
        }
    }
    public struct DropInfo
    {
        public TreeListNode Row { get; set; }
        public DropPos Position { get; set; }
    }
    public enum DropPos
    {
        InsideNode,
        BeforeNode,
        AfterNode
    }
}
