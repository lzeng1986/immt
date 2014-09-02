using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.List
{
    public class ItemCell
    {
        Color m_ForeColor = Color.Black;
        int m_nImageIndex = -1;
        ContentAlignment imageAlignment = ContentAlignment.MiddleLeft;
        bool m_bSelected = false;
        bool m_bForceText = false;
        Control m_Control = null;
        ListView parent = null;
        bool m_bChecked = false;

        string name = "Cell";
        [Browsable(false)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public Rectangle CellBounds { get; set; }

        public bool Checked
        {
            get { return m_bChecked; }
            set
            {
                if (m_bChecked != value)
                {
                    m_bChecked = value;

                }
            }
        }
        [Browsable(false)]
        public ListView Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        public ListRow Row { get; internal set; }

        public Column Column { get; internal set; }

        public bool ForceText
        {
            get { return m_bForceText; }
            set
            {
                if (m_bForceText != value)
                {
                    m_bForceText = value;
                }
            }
        }
        public Control Control
        {
            get { return m_Control; }
            set
            {
                if (m_Control != value)
                {
                    m_Control = value;

                    // do all other default control setups here
                    m_Control.Visible = false;
                    //m_Control.Parent = this.Parent;
                    //m_Control.MouseDown += new MouseEventHandler( this.ListView.OnMouseDownFromSubItem );

                    //if ( ChangedEvent != null )
                    //ChangedEvent( this, new ChangedEventArgs( ChangedTypes.SubItemChanged, null, null, this ) );
                }
            }
        }
        [TypeConverter(typeof(ImageKeyConverter))]
        public string ImageKey { get; set; }

        public ContentAlignment ImageAlignment
        {
            get { return imageAlignment; }
            set
            {
                if (imageAlignment != value)
                {
                    imageAlignment = value;
                }
            }
        }

        object val;
        public object Value
        {
            get { return val; }
            set
            {
                if (object.Equals(val, value))
                    return;
                val = value;
            }
        }

        public bool Selected		// sub item
        {
            get { return m_bSelected; }
            set
            {
                if (m_bSelected != value)
                {
                    m_bSelected = value;
                    //if (ChangedEvent != null)
                    //    ChangedEvent(this, new ChangedEventArgs(ChangedTypes.ItemChanged, null, null, this));
                }
            }
        }
    }

    public class ItemCellCollection : CollectionBase, IEnumerable<ItemCell>
    {
        ListRow parent;
        public ItemCellCollection(ListRow row)
        {
            this.parent = row;
        }

        protected override void OnClear()
        {
        }

        public ItemCell this[int index]
        {
            get { return (ItemCell)List[index]; }
        }

        public void AddRange(IEnumerable<ItemCell> subItems)
        {
            foreach (var item in subItems)
                List.Add(item);
        }
        public int Add(ItemCell subItem)
        {
            return List.Add(subItem);
        }
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            (value as ItemCell).Row = parent;
            (value as ItemCell).Column = parent.Parent.Columns[index];
        }
        protected override void OnRemove(int index, object value)
        {
            base.OnRemove(index, value);
            (value as ItemCell).Row = null;
            (value as ItemCell).Column = null;
        }
        public int Insert(int index, ItemCell subItem)
        {
            List.Insert(index, subItem);
            return index;
        }

        public void Remove(int nSubItemIndex)
        {
            List.RemoveAt(nSubItemIndex);
        }
        public void Remove(ItemCell subItem)
        {
            List.Remove(subItem);
        }

        public void ClearSelection()
        {
            foreach (var item in this.OfType<ItemCell>())
                item.Selected = false;
        }

        public new IEnumerator<ItemCell> GetEnumerator()
        {
            foreach (var cell in InnerList)
                yield return cell as ItemCell;
        }
    }
}
