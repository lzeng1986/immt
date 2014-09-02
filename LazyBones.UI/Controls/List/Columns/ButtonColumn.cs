using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.List.Columns
{
    public class ButtonColumn : Column
    {
        public override void DrawCell(Graphics g, CellPaintStyle style)
        {
            ControlPaint.DrawButton(g, style.Cell.CellBounds, ButtonState.Normal);
            base.DrawCell(g, style);
        }
        public override void OnMouseDown(MouseEventArgs e)
        {
        }
        public override void OnMouseUp(MouseEventArgs e)
        {
        }
        public override void OnMouseClick(MouseEventArgs e)
        {
           Click.SafeCall(this);
        }
        public event EventHandler Click;
    }
}
