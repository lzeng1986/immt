using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;
using LazyBones.Utils;

namespace LazyBones.UI.Controls.Tree
{
    [DefaultProperty("HeadText")]
    public abstract class TreeColumn : Component
    {
        StringFormat headerFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.MeasureTrailingSpaces)
        {
            LineAlignment = StringAlignment.Center,
            Alignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };
        TreeList tree;
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TreeList Tree
        {
            get { return tree; }
            set
            {
                if (value != tree)
                {
                    if (tree != null)
                        tree.Columns.Remove(this);
                    if (value != null)
                        value.Columns.Add(this);
                }
            }
        }
        //用于程序集内部直接设置tree，如果使用Tree属性设置，可能导致函数循环调用
        internal void SetParentInternal(TreeList parent)
        {
            this.tree = parent;
        }
        [Browsable(false)]
        public int Index { get; internal set; }
        [Browsable(false)]
        public int DisplayIndex { get; internal set; }
        string headText;
        [Localizable(true), Category("Appearance")]
        public string HeadText
        {
            get { return headText; }
            set
            {
                headText = value;
                if (tree != null)
                    tree.UpdateColumnHeaders();
            }
        }
        ContentAlignment headerTextAlign = ContentAlignment.MiddleCenter;
        [DefaultValue(ContentAlignment.MiddleCenter), Category("Appearance")]
        public ContentAlignment HeaderTextAlign
        {
            get { return headerTextAlign; }
            set
            {
                headerTextAlign = value;
                Helper.FillAligment(headerTextAlign, headerFormat);
                if (tree != null)
                    tree.FullUpdate();
            }
        }

        Color headerForeColor = SystemColors.WindowText;
        [DefaultValue(typeof(Color), "WindowText"), Category("Appearance")]
        public Color HeaderForeColor
        {
            get { return headerForeColor; }
            set
            {
                headerForeColor = value;
                if (tree != null)
                    tree.UpdateColumnHeaders();
            }
        }

        int width = 50;
        [DefaultValue(50), Localizable(true), Category("Appearance")]
        public virtual int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    if (value < 0)
                        throw new ArgumentOutOfRangeException("value");
                    width = value;
                    if (tree != null)
                        tree.UpdateColumnWidth(this);
                }
            }
        }

        bool visible = true;
        [DefaultValue(true), Category("Behavior")]
        public bool Visible
        {
            get { return visible; }
            set
            {
                visible = value;
                if (Tree != null)
                {
                    Tree.Columns.UpdateDisplayIndex();
                    Tree.FullUpdate();
                }
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(HeadText))
                return GetType().Name;
            else
                return HeadText;
        }

        internal void DrawHeader(Graphics g, Rectangle bounds, Font font)
        {
            using (var brush = new SolidBrush(HeaderForeColor))
                g.DrawString(HeadText, font, brush, bounds, headerFormat);
        }

        
        public object GetValue(TreeListNode node)
        {
            ParamGuard.NotNull(node, "node");
            if (dataMemberDescriptor != null)
                return dataMemberDescriptor.GetValue(node.Value);
            else
                return node.Value;
        }

        public void SetValue(TreeListNode node, object value)
        {
            ParamGuard.NotNull(node, "node");
            if (dataMemberDescriptor != null)
                dataMemberDescriptor.SetValue(node.Value, value);
            else
                node.Value = value;
        }

        [Browsable(false)]
        public object DataSource
        {
            get { return tree == null ? null : tree.DataSource; }
        }

        PropertyDescriptor dataMemberDescriptor;
        [DefaultValue((string)null), Category("Data")]
        [TypeConverter("System.Windows.Forms.Design.DataMemberFieldConverter, System.Design")]
        [Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", typeof(UITypeEditor))]
        public string DataMember
        {
            get { return dataMemberDescriptor == null ? null : dataMemberDescriptor.DisplayName; }
            set
            {
                if (dataMemberDescriptor != null && dataMemberDescriptor.Name == value)
                    return;
                dataMemberDescriptor = tree == null ? null : tree.GetFieldProperty(value);
            }
        }

        public abstract Size MeasureSize(TreeListNode node);
        public abstract void Draw(TreeListNode node, DrawContext context);

        public virtual string GetToolTip(TreeListNode node)
        {
            return string.Empty;
        }

        public virtual void MouseDown(TreeListNodeMouseEventArgs args)
        {
        }

        public virtual void MouseUp(TreeListNodeMouseEventArgs args)
        {
        }

        public virtual void MouseDoubleClick(TreeListNodeMouseEventArgs args)
        {
        }

        public virtual void KeyDown(KeyEventArgs args)
        {
        }

        public virtual void KeyUp(KeyEventArgs args)
        {
        }
    }
}
