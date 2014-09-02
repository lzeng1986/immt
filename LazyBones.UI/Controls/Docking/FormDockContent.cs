using System;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing;

namespace LazyBones.UI.Controls.Docking
{
    public class FormDockContent : Form, IDockContent
    {
        public FormDockContent()
        {
            dockContentHandler = DockContentHandler.CreateFrom(this);
            this.ParentChanged += FormParentChanged;
        }
        void FormParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
                this.Font = Parent.Font;
        }
        private DockContentHandler dockContentHandler;
        [Browsable(false)]
        public DockContentHandler Handler
        {
            get { return dockContentHandler; }
        }
        [Category("Docking")]
        [Description("DockContent_AllowEndUserDocking_Description")]
        [DefaultValue(true)]
        public bool AllowEndUserDocking
        {
            get { return dockContentHandler.AllowEndUserDocking; }
            set { dockContentHandler.AllowEndUserDocking = value; }
        }

        [Category("Docking")]
        [Description("获取或设置可停靠的区域")]
        [DefaultValue(DockAreas.All)]
        public DockAreas DockAreas
        {
            get { return dockContentHandler.DockAreas; }
            set { dockContentHandler.DockAreas = value; }
        }

        [Category("Docking")]
        [Description("DockContent_AutoHidePortion_Description")]
        [DefaultValue(0.25)]
        public double AutoHidePortion
        {
            get { return dockContentHandler.AutoHidePortion; }
            set { dockContentHandler.AutoHidePortion = value; }
        }

        [Localizable(true)]
        [Category("Docking")]
        [Description("DockContent_TabText_Description")]
        [DefaultValue(null)]
        public string TabText
        {
            get { return dockContentHandler.TabText; }
            set { dockContentHandler.TabText = value; }
        }

        bool ShouldSerializeTabText()
        {
            return (dockContentHandler.TabText != null);
        }

        [Category("Docking")]
        [Description("获取或设置关闭按钮是否可用")]
        [DefaultValue(true)]
        public bool CloseButtonEnabled
        {
            get { return dockContentHandler.CloseButtonEnabled; }
            set { dockContentHandler.CloseButtonEnabled = value; }
        }

        [Category("Docking")]
        [Description("获取或设置关闭按钮是否可见")]
        [DefaultValue(true)]
        public bool CloseButtonVisible
        {
            get { return dockContentHandler.CloseButtonVisible; }
            set { dockContentHandler.CloseButtonVisible = value; }
        }
        [Category("Docking")]
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockPanel DockPanel
        {
            get { return dockContentHandler.DockPanel; }
            set { dockContentHandler.DockPanel = value; }
        }

        [Category("Docking")]
        [Browsable(true)]
        [Description("获取或设置停靠方式")]
        public DockStyle DockStyle
        {
            get { return dockContentHandler.DockStyle; }
            set { dockContentHandler.DockStyle = value; }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DockGrid DockGrid
        {
            get { return dockContentHandler.DockGrid; }
            set { dockContentHandler.DockGrid = value; }
        }

        [Category("Docking")]
        [Description("DockContent_HideOnClose_Description")]
        [DefaultValue(false)]
        public bool HideOnClose
        {
            get { return dockContentHandler.HideOnClose; }
            set { dockContentHandler.HideOnClose = value; }
        }

        [Browsable(false)]
        public bool IsActivated
        {
            get { return dockContentHandler.IsActivated; }
        }

        public bool IsDockValid(DockStyle dockStyle)
        {
            return dockContentHandler.IsDockValid(dockStyle);
        }

        [Category("Docking")]
        [Description("DockContent_TabPageContextMenu_Description")]
        [DefaultValue(null)]
        public ContextMenu TabPageContextMenu
        {
            get { return dockContentHandler.TabPageContextMenu; }
            set { dockContentHandler.TabPageContextMenu = value; }
        }

        [Category("Docking")]
        [Description("DockContent_TabPageContextMenuStrip_Description")]
        [DefaultValue(null)]
        public ContextMenuStrip TabPageContextMenuStrip
        {
            get { return dockContentHandler.TabPageContextMenuStrip; }
            set { dockContentHandler.TabPageContextMenuStrip = value; }
        }

        [Localizable(true)]
        [Category("Appearance")]
        [Description("DockContent_ToolTipText_Description")]
        [DefaultValue(null)]
        public string ToolTipText
        {
            get { return dockContentHandler.ToolTipText; }
            set { dockContentHandler.ToolTipText = value; }
        }

        void IDockContent.OnActivated(EventArgs e)
        {
            OnActivated(e);
        }

        void IDockContent.OnDeactivate(EventArgs e)
        {
            OnDeactivate(e);
        }

        public new void Activate()
        {
            dockContentHandler.Activate();
        }

        public new void Show()
        {
            dockContentHandler.Show();
        }
        //在DockPanel中显示该窗体
        public void Show(DockPanel dockPanel)
        {
            dockContentHandler.Show(dockPanel);
        }

        public void FloatAt(Rectangle floatWindowBounds)
        {
            dockContentHandler.FloatAt(floatWindowBounds);
        }
        public void DockTo(DockGrid gridTo, DockStyle dockStyle, int contentIndex)
        {
            dockContentHandler.DockTo(gridTo, dockStyle, contentIndex);
        }

        public void DockTo(DockPanel panel, DockStyle dockStyle)
        {
            dockContentHandler.DockTo(panel, dockStyle);
        }
        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            dockContentHandler.Visible = this.Visible;
        }
    }
}
