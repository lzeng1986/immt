using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.List.Columns
{
    public class TextColumn : Column
    {
        protected internal override Control EditControl
        {
            get
            {
                return new TextBoxEditor();
            }
        }
        class TextBoxEditor : TextBox, IEditorControl
        {
            public object Value
            {
                get { return Text; }
                set { Text = value == null ? string.Empty : value.ToString(); }
            }

            public bool Load(ListRow item, ItemCell subItem, ListView listView)
            {
                Focus();
                return true;
            }

            public void Unload()
            {
            }

            public bool ShouldCloseEditor(Keys key)
            {
                return key == Keys.Escape || key == Keys.Enter;
            }
        }
    }
}
