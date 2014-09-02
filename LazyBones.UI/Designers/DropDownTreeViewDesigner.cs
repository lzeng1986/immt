using System.ComponentModel;
using System.ComponentModel.Design;

namespace LazyBones.UI.Designers
{
    class DropDownTreeViewDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (null == actionLists)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new DropDownTreeViewActionList(this.Component));
                }
                return actionLists;
            }
        }
    }
    class DropDownTreeViewActionList : DesignerActionList
    {
        //private DropDownTreeView dropDownTreeView;

        private DesignerActionUIService designerActionUISvc = null;
        public DropDownTreeViewActionList(IComponent component)
            : base(component)
        {
            //this.dropDownTreeView = component as LazyBones.UI.DropDownTreeView;
            //if (dropDownTreeView == null)
            //    throw new ArgumentException("设计器必须DropDownTreeView关联");
            this.designerActionUISvc = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        void EditTreeView()
        {
            //var form = new FormTreeViewEditor() { EditControl = dropDownTreeView.TreeView };
            //if(form.ShowDialog() == System.Windows.Forms.DialogResult.OK){
            //    dropDownTreeView.SetPropertyValue("TreeView", form.EditControl);
            //}
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();
            items.Add(new DesignerActionMethodItem(this, "EditTreeView", "编辑TreeView"));
            return items;
        }
    }
}
