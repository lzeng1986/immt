using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.Tree
{
    public class TreeColumnCollection : Collection<TreeColumn>
    {
        TreeList tree;
        public TreeColumnCollection(TreeList tree)
        {
            this.tree = tree;
        }
        protected override void ClearItems()
        {
            foreach (var item in this)
                item.SetParentInternal(null);
            base.ClearItems();
            tree.UpdateColumns();
        }
        protected override void InsertItem(int index, TreeColumn item)
        {
            base.InsertItem(index, item);
            for (var i = index; i < Count; i++)
                this[i].Index = i;
            item.SetParentInternal(tree);
            UpdateDisplayIndex();
            tree.UpdateColumns();
        }
        protected override void RemoveItem(int index)
        {
            this[index].SetParentInternal(null);
            base.RemoveItem(index);
            for (var i = index; i < Count; i++)
                this[i].Index = i;
            UpdateDisplayIndex();
            tree.UpdateColumns();
        }
        protected override void SetItem(int index, TreeColumn item)
        {
            this[index].SetParentInternal(null);
            base.SetItem(index, item);
            item.SetParentInternal(tree);
            item.Index = index;
            tree.UpdateColumns();
        }
        internal void UpdateDisplayIndex()
        {
            var ind = 0;
            for (var i = 0; i < Count; i++)
            {
                if (this[i].Visible)
                    this[i].DisplayIndex = ind++;
                else
                    this[i].DisplayIndex = -1;
            }
        }
    }
}
