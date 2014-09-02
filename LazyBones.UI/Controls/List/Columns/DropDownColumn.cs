using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms.VisualStyles;

namespace LazyBones.UI.Controls.List.Columns
{
    public class DropDownColumn : Column
    {
        public override void DrawCell(Graphics g, CellPaintStyle style)
        {
            var cell = style.Cell;
            var rectDropDownButton = new Rectangle(
                cell.CellBounds.Right - 19,
                cell.CellBounds.Top + 1,
                18,
                cell.CellBounds.Height - 2
                );

            ComboBoxRenderer.DrawTextBox(g, cell.CellBounds, ComboBoxState.Normal);
            ComboBoxRenderer.DrawDropDownButton(g, rectDropDownButton, ComboBoxState.Normal);
            base.DrawCell(g, style);
        }
        protected internal override Control EditControl
        {
            get
            {
                return new DropDown();
            }
        }
        public override void OnMouseDown(MouseEventArgs e)
        {

        }

        class DropDown : DropDownRichTextBox, IEditorControl
        {
            public new object Value
            {
                get { return Text; }
                set { Text = (string)value; }
            }

            public new bool Load(ListRow item, ItemCell subItem, ListView listView)
            {
                OpenDropDown();
                return true;
            }

            public void Unload()
            {

            }

            public bool ShouldCloseEditor(Keys key)
            {
                return key == Keys.Escape;
            }
        }
    }
}
