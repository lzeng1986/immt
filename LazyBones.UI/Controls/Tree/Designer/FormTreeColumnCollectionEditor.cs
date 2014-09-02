using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree.Designer
{
    public partial class FormTreeColumnCollectionEditor : Form
    {
        public FormTreeColumnCollectionEditor()
        {
            InitializeComponent();
        }
        public TreeList Tree { get; set; }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }
        void LoadColumns()
        {
            var ind = columnList.SelectedIndex;
            columnList.Items.Clear();
            foreach (var col in Tree.Columns)
            {
                columnList.Items.Add(col);
            }
            columnList.SelectedIndex = ind;
        }
        string GetDisplayText(object value)
        {
            if (value == null)
                return string.Empty;
            string text;
            var defaultProperty = TypeDescriptor.GetProperties(value)["Name"];
            if ((defaultProperty != null) && (defaultProperty.PropertyType == typeof(string)))
            {
                text = (string)defaultProperty.GetValue(value);
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
            defaultProperty = TypeDescriptor.GetDefaultProperty(value.GetType());
            if ((defaultProperty != null) && (defaultProperty.PropertyType == typeof(string)))
            {
                text = (string)defaultProperty.GetValue(value);
                if (!string.IsNullOrEmpty(text))
                    return text;
            }
            text = TypeDescriptor.GetConverter(value).ConvertToString(value);
            if (!string.IsNullOrEmpty(text))
                return text;
            return value.GetType().Name;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            contextMenuStrip.Items.Clear();
            var types = typeof(TreeColumn).Assembly.GetTypes()
                .Where(t => typeof(TreeColumn).IsAssignableFrom(t) && !t.IsAbstract);
            foreach (var t in types)
            {
                var item = contextMenuStrip.Items.Add(t.Name);
                item.Tag = t;
            }
            contextMenuStrip.Show(Control.MousePosition);
        }

        private void contextMenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var type = e.ClickedItem.Tag as Type;
            var col = Activator.CreateInstance(type) as TreeColumn;
            Tree.Columns.Add(col);
            LoadColumns();
        }

        private void columnList_SelectedIndexChanged(object sender, EventArgs e)
        {
            propertyGrid.SelectedObject = Tree.Columns[columnList.SelectedIndex];
        }
        private void buttonDel_Click(object sender, EventArgs e)
        {
            if (columnList.SelectedIndex != -1)
            {
                Tree.Columns.RemoveAt(columnList.SelectedIndex);
                LoadColumns();
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {
            if (columnList.SelectedIndex != -1 && columnList.SelectedIndex > 0)
            {
                var col = Tree.Columns[columnList.SelectedIndex];
                Tree.Columns.Remove(col);
                Tree.Columns.Insert(columnList.SelectedIndex - 1, col);
                LoadColumns();
                columnList.SelectedIndex--;
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (columnList.SelectedIndex != -1 && columnList.SelectedIndex < Tree.Columns.Count - 1)
            {
                var col = Tree.Columns[columnList.SelectedIndex];
                Tree.Columns.Remove(col);
                Tree.Columns.Insert(columnList.SelectedIndex + 1, col);
                LoadColumns();
                columnList.SelectedIndex++;
            }
        }

        private void propertyGrid_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
        }
    }
}
