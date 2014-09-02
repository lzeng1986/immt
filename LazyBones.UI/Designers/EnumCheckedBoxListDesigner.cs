using System;
using System.ComponentModel.Design;
using LazyBones.UI.Controls;
using LazyBones.Extensions;

namespace LazyBones.UI.Designers
{
    class EnumCheckedBoxListDesigner : System.Windows.Forms.Design.ControlDesigner
    {
        private DesignerActionListCollection actionLists;
        public override DesignerActionListCollection ActionLists
        {
            get
            {
                if (null == actionLists)
                {
                    actionLists = new DesignerActionListCollection();
                    actionLists.Add(new EnumCheckedBoxListActionList(this.Component));
                }
                return actionLists;
            }
        }
    }

    class EnumCheckedBoxListActionList : DesignerActionList
    {
        private EnumCheckedBoxList checkboxlist;

        private DesignerActionUIService designerActionUISvc = null;
        public EnumCheckedBoxListActionList(System.ComponentModel.IComponent component)
            : base(component)
        {
            this.checkboxlist = component as EnumCheckedBoxList;
            if (checkboxlist == null)
                throw new ArgumentException("设计器必须EnumCheckedBoxList关联");
            this.designerActionUISvc = GetService(typeof(DesignerActionUIService)) as DesignerActionUIService;
        }

        [
        System.ComponentModel.Editor(typeof(EnumSelectEditor), typeof(System.Drawing.Design.UITypeEditor)),
        System.ComponentModel.DefaultValue((Type)null),
        System.ComponentModel.TypeConverter(typeof(EnumTypeConverter))
        ]
        public Type EnumType
        {
            get
            {
                return checkboxlist.EnumType;
            }
            set
            {
                checkboxlist.SetPropertyValue("EnumType", value);
            }
        }

        public override DesignerActionItemCollection GetSortedActionItems()
        {
            var items = new DesignerActionItemCollection();
            items.Add(new DesignerActionPropertyItem("EnumType", "关联的枚举类型"));
            return items;
        }
    }
}
