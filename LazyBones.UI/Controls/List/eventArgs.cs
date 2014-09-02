using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.UI.Controls.List
{
    public class ClickEventArgs : EventArgs
    {
        private int itemIndex;
        private int columnIndex;

        public ClickEventArgs(int itemindex, int columnindex)
        {
            itemIndex = itemindex;
            columnIndex = columnindex;
        }

        public int ItemIndex
        {
            get { return itemIndex; }
        }

        public int ColumnIndex
        {
            get { return columnIndex; }
        }
    }

    public class ChangedEventArgs : EventArgs
    {
        public ChangedEventArgs(ChangedTypes type, Column column, ListRow item, ItemCell subItem)
        {
            ChangedType = type;
            Column = column;
            Item = item;
            SubItem = subItem;
        }

        public Column Column { get; private set; }

        public ListRow Item { get; private set; }

        public ItemCell SubItem { get; private set; }

        public ChangedTypes ChangedType { get; private set; }
    }
    public class HotHitEventArgs : EventArgs
    {
        public HitInfo HitInfo { get; private set; }
        public HotHitEventArgs(HitInfo info)
        {
            HitInfo = info;
        }
    }
    public enum ChangedTypes
    {
        GeneralInvalidate,
        SubItemChanged,
        SubItemCollectionChanged,
        ItemChanged,
        /// <summary>
        /// Item Collection Changed
        /// </summary>
        ItemCollectionChanged,
        /// <summary>
        /// Column changed
        /// </summary>
        ColumnChanged,
        /// <summary>
        /// Column Collection Changed
        /// </summary>
        ColumnCollectionChanged,
        /// <summary>
        /// Focus Changed
        /// </summary>
        FocusedChanged,
        /// <summary>
        /// A different item is now selected
        /// </summary>
        SelectionChanged,
        /// <summary>
        /// Column state has changed
        /// </summary>
        ColumnStateChanged
    };
}
