using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    [DesignTimeVisible(false), ToolboxItem(false)]
    public abstract class NodeCell : Component
    {
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
                        tree.NodeControls.Remove(this);

                    if (value != null)
                        value.NodeControls.Add(this);
                }
            }
        }
        [Browsable(false)]
        public TreeColumn Column { get; internal set; }
        [Browsable(false)]
        public TreeListNode Owner { get; internal set; }
        [Browsable(false)]
        public int Width { get; internal set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IToolTipProvider ToolTipProvider { get; set; }

        int columnIndex;
        [DefaultValue(0)]
        public int ColumnIndex
        {
            get { return columnIndex; }
            set
            {
                if (columnIndex < 0)
                    throw new ArgumentOutOfRangeException("值不得小于0");
                columnIndex = value;
                if (tree != null)
                    tree.FullUpdate();
            }
        }

        internal void AssignParentInternal(TreeList parent)
        {
            this.tree = parent;
        }

        string propertyName = string.Empty;
        [DefaultValue("")]
        public string DataPropertyName
        {
            get { return propertyName; }
            set { propertyName = (value == null) ? string.Empty : value; }
        }

        public Type BindType { get; set; }

        object bindValue = null;
        public object Value
        {
            get
            {
                var pi = PropertyInfo;
                if (pi != null && pi.CanRead)
                    return pi.GetValue(bindValue, null);
                else
                    return bindValue;
            }
            set
            {
                var pi = PropertyInfo;
                if (pi != null && pi.CanWrite)
                {
                    try
                    {
                        pi.SetValue(bindValue, value, null);
                    }
                    catch (TargetInvocationException ex)
                    {
                        if (ex.InnerException != null)
                            throw new ArgumentException(ex.InnerException.Message, ex.InnerException);
                        else
                            throw new ArgumentException(ex.Message);
                    }
                }
            }
        }

        public Type PropertyType
        {
            get
            {
                if (BindType == null || string.IsNullOrEmpty(propertyName))
                    return null;
                var pi = BindType.GetProperty(propertyName);
                if (pi != null)
                    return pi.PropertyType;
                return null;
            }
        }

        PropertyInfo PropertyInfo
        {
            get
            {
                if (BindType == null || string.IsNullOrEmpty(propertyName))
                    return null;
                return BindType.GetProperty(propertyName);
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(propertyName))
                return GetType().Name;
            else
                return string.Format("{0} ({1})", GetType().Name, propertyName);
        }

        public abstract Size MeasureSize(TreeListNode node);

        public abstract void Draw(TreeListNode node, DrawContext context);

        public virtual string GetToolTip(TreeListNode node)
        {
            if (ToolTipProvider != null)
                return ToolTipProvider.GetToolTip(node);
            else
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
