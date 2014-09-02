using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace LazyBones.UI.Controls.DataGridViews
{
    public class DataGridViewTreeListColumn : DataGridViewColumn
    {
        public DataGridViewTreeListColumn() 
        :base(new DataGridViewTreeListCell())
        {
            ShowLines = true;
        }
        public override object Clone()
        {
            var col = base.Clone() as DataGridViewTreeListColumn;
            col.ShowLines = ShowLines;
            return col;
        }
        [Category("TreeList"),DefaultValue(true)]
        public bool ShowLines { get; set; }
    }

    class DataGridViewTreeListCell : DataGridViewTextBoxCell
    {
        public int Level { get; set; }
        protected override void Paint(System.Drawing.Graphics graphics, System.Drawing.Rectangle clipBounds, System.Drawing.Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
        }
    }
}
