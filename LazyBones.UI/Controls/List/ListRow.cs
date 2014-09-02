using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;
using LazyBones.Linq;
using LazyBones.Extensions;
using System.ComponentModel;

namespace LazyBones.UI.Controls.List
{
    public class ListRow
    {
        ItemCellCollection cells;

        Color foreColor = Color.Black;
        Color borderColor = Color.Black;
        Color backColor = Color.White;
        int borderSize = 0;

        public ListRow()
        {
            cells = new ItemCellCollection(this);
        }

        public int BorderSize
        {
            get { return borderSize; }
            set { borderSize = value; }
        }

        internal ItemCellCollection Cells
        {
            get { return cells; }
        }
        public Color BackColor
        {
            get { return backColor; }
            set { backColor = value; }
        }
        public Color RowBorderColor
        {
            get { return borderColor; }
            set { borderColor = value; }
        }

        [Browsable(false)]
        public object Tag { get; set; }

        public Color ForeColor
        {
            get { return foreColor; }
            set { foreColor = value; }
        }

        public ListView Parent { get; internal set; }

        bool selected = false;
        [Browsable(false)]
        public bool Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (!selected)
                    foreach (var cell in cells)
                        cell.Selected = false;
            }
        }
    }



    public class ListRowCollection : CollectionBase, IEnumerable<ListRow>
    {
        ListView parent;
        public ListRowCollection(ListView parent)
        {
            this.parent = parent;
        }

        bool m_bSuspendEvents = false;

        public bool SuspendEvents
        {
            set { m_bSuspendEvents = value; }
            get { return m_bSuspendEvents; }
        }

        public ListView Parent
        {
            get { return parent; }
        }

        public ListRow this[int index]
        {
            get { return (ListRow)List[index]; }
            set { List[index] = value; }
        }

        public IEnumerable<ListRow> SelectedItems
        {
            get { return this.OfType<ListRow>().Where(i => i.Selected); }
        }

        public IEnumerable<int> SelectedIndicies
        {
            get
            {
                var i = 0;
                foreach (ListRow item in this)
                    if (item.Selected)
                        yield return i++;
            }
        }

        public void AddRange(IEnumerable<ListRow> items)
        {
            foreach (var item in items)
                List.Add(item);
        }

        public int Add(ListRow item)
        {
            return List.Add(item);
        }

        public void Insert(int index, ListRow item)
        {
            List.Insert(index, item);
        }

        public void Remove(ListRow item)
        {
            List.Remove(item);
        }

        public void ClearSelection()
        {
            foreach (var item in this)
                item.Selected = false;
        }
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            (value as ListRow).Parent = parent;
            parent.SuspendPaint();
            (value as ListRow).Cells.Clear();
            (value as ListRow).Cells.AddRange(Enumerable.Range(0, parent.Columns.Count).Select(c => new ItemCell()));
            parent.CalcScroll();
            parent.ResumePaint();
        }
        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            (value as ListRow).Parent = null;
            parent.SuspendPaint();
            (value as ListRow).Cells.Clear();
            parent.CalcScroll();
            parent.ResumePaint();
        }
        protected override void OnClearComplete()
        {
            parent.SuspendPaint();
            foreach (var r in this)
                r.Cells.Clear();
            parent.CalcScroll();
            parent.ResumePaint();
            base.OnClearComplete();

        }
        public void ClearSelection(ListRow itemIgnore)
        {
            foreach (var item in this.Where(i => i != itemIgnore))
                item.Selected = false;
        }

        public int GetNextSelectedItemIndex(int startIndex)
        {
            return this.Skip(startIndex).IndexOf(i => i.Selected);
        }

        public int IndexOf(ListRow item)
        {
            return this.IndexOf(item);
        }

        public new IEnumerator<ListRow> GetEnumerator()
        {
            foreach (var row in InnerList)
                yield return row as ListRow;
        }
    }
}
