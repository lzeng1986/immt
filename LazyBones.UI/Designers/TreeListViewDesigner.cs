using System.ComponentModel;
using System.ComponentModel.Design;

namespace LazyBones.UI.Designers
{
    class TreeListViewDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (null == actionLists)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new TreeListViewActionList(this.Component));
                }
                return actionLists;
            }
        }
    }

    class TreeListViewActionList : DesignerActionList
    {
        //private TreeListView treeList;

        private DesignerActionUIService designerActionUISvc = null;
        public TreeListViewActionList(IComponent component)
            : base(component)
        {
            //this.treeList = component as TreeListView;
            //if (treeList == null)
            //    throw new ArgumentException("设计器必须TreeListView关联");
            designerActionUISvc = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        //[AttributeProvider(typeof(IListSource))]
        //public object DataSource
        //{
        //    get
        //    {
        //        return treeList.DataSource;
        //    }
        //    set
        //    {
        //        treeList.SetPropertyValue("DataSource", value);
        //    }
        //}

        //[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string DisplayMember
        //{
        //    get
        //    {
        //        return treeList.DisplayMember;
        //    }
        //    set
        //    {
        //        treeList.SetPropertyValue("DisplayMember", value);
        //    }
        //}

        //[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string ValueMember
        //{
        //    get
        //    {
        //        return treeList.ValueMember;
        //    }
        //    set
        //    {
        //        treeList.SetPropertyValue("ValueMember", value);
        //    }
        //}
        //[Editor("System.Windows.Forms.Design.DataMemberFieldEditor, System.Design", "System.Drawing.Design.UITypeEditor, System.Drawing")]
        //public string ParentMember
        //{
        //    get
        //    {
        //        return treeList.ParentMember;
        //    }
        //    set
        //    {
        //        treeList.SetPropertyValue("ParentMember", value);
        //    }
        //}

        //[Editor(typeof(Immt.Forms.LazyBones.ObjectEditor), typeof(System.Drawing.Design.UITypeEditor))]
        //public object RootValue
        //{
        //    get
        //    {
        //        return treeList.RootValue;
        //    }
        //    set
        //    {
        //        treeList.SetPropertyValue("RootValue", value);
        //    }
        //}

        void EditNodes()
        {
            //new LazyBones.FormTreeListViewNodeEditor().ShowDialog();
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "EditNodes", "编辑节点"));
            items.Add(new DesignerActionPropertyItem("DataSource", "数据源"));
            items.Add(new DesignerActionPropertyItem("DisplayMember", "显示的数据项"));
            items.Add(new DesignerActionPropertyItem("ValueMember", "值的数据项"));
            items.Add(new DesignerActionPropertyItem("ParentMember", "父节点的数据项"));
            items.Add(new DesignerActionPropertyItem("RootValue", "根节点值"));
            return items;
        }
    }
}
