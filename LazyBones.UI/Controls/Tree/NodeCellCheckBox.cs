using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace LazyBones.UI.Controls.Tree
{
    public class NodeCellCheckBox : NodeCell
    {
        public const int ImageSize = 13;

        static Image CheckImg = ControlRes.check;
        static Image UncheckImg = ControlRes.uncheck;
        static Image IndeterminateImg = ControlRes.unknown;

        [DefaultValue(false)]
        public bool ThreeState { get; set; }

        [DefaultValue(true)]
        public bool EditEnabled { get; set; }

        public NodeCellCheckBox()
            : this(string.Empty)
        {
        }

        public NodeCellCheckBox(string propertyName)
        {
            DataPropertyName = propertyName;
            ThreeState = false;
            EditEnabled = true;
            Width = 13;
        }

        protected virtual CheckState CheckState
        {
            get
            {
                object obj = Value;
                if (obj is CheckState)
                    return (CheckState)obj;
                else if (obj is bool)
                    return (bool)obj ? CheckState.Checked : CheckState.Unchecked;
                else
                    return CheckState.Unchecked;
            }
            set
            {
                var type = PropertyType;
                if (type == typeof(CheckState))
                {
                    Value = value;
                    OnCheckStateChanged();
                }
                else if (type == typeof(bool))
                {
                    Value = value != CheckState.Unchecked;
                    OnCheckStateChanged();
                }
            }
        }

        public override void MouseDown(TreeListNodeMouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left && EditEnabled)
            {
                var state = CheckState;
                state = GetNewState(state);
                CheckState = state;
                args.Handled = true;
            }
        }

        public override void MouseDoubleClick(TreeListNodeMouseEventArgs args)
        {
            args.Handled = true;
        }

        CheckState GetNewState(CheckState state)    //定义checkbox状态变换
        {
            if (state == CheckState.Indeterminate)
                return CheckState.Unchecked;
            else if (state == CheckState.Unchecked)
                return CheckState.Checked;
            else
                return ThreeState ? CheckState.Indeterminate : CheckState.Unchecked;
        }

        public override void KeyDown(KeyEventArgs args)
        {
            if (args.KeyCode == Keys.Space && EditEnabled)
            {
                using (Tree.GetUpdateHolder())
                {
                    if (Tree.CurrentNode != null)
                    {
                        //CheckState value = GetNewState(GetCheckState(Parent.CurrentNode));
                        //foreach (var node in Parent.SelectedNodes)
                        //     SetCheckState(node, value);
                    }
                }
                args.Handled = true;
            }
        }

        public event EventHandler<TreePathEventArgs> CheckStateChanged;
        protected void OnCheckStateChanged(TreePathEventArgs args)
        {
            if (CheckStateChanged != null)
                CheckStateChanged(this, args);
        }

        protected void OnCheckStateChanged()
        {
            OnCheckStateChanged(new TreePathEventArgs(Owner.Path));
        }
        public override Size MeasureSize(TreeListNode node)
        {
            return new Size(Width, Width);
        }

        public override void Draw(TreeListNode node, DrawContext context)
        {
            Rectangle r = context.Bounds;
            int dy = (r.Height - ImageSize) / 2;
            var state = CheckState;
            Image img;
            if (state == CheckState.Indeterminate)
                img = IndeterminateImg;
            else if (state == CheckState.Checked)
                img = CheckImg;
            else
                img = UncheckImg;
            context.Graphics.DrawImage(img, r.X, r.Y + dy, ImageSize, ImageSize);
        }
    }
}
