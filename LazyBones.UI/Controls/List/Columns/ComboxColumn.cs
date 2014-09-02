using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.List.Columns
{
    public class ComboxColumn : Column
    {
        public override void DrawCell(Graphics g, CellPaintStyle style)
        {
            //ControlPaint.DrawComboButton
            base.DrawCell(g, style);
        }
        protected internal override Control EditControl
        {
            get
            {
                return new ComboBox();
            }
        }
        class Combox : ComboBox, IEditorControl
        {
            public Combox()
            {
            }
            public object Value
            {
                get
                {
                    return SelectedValue;
                }
                set
                {
                    SelectedValue = value;
                }
            }

            public bool Load(ListRow item, ItemCell subItem, ListView listView)
            {
                DroppedDown = true;
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
