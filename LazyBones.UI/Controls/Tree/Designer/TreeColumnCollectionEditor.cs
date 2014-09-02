using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace LazyBones.UI.Controls.Tree.Designer
{
    class TreeColumnCollectionEditor : UITypeEditor
    {
        FormTreeColumnCollectionEditor dialog;
        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                IWindowsFormsEditorService service = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
                if ((service == null) || (context.Instance == null))
                {
                    return value;
                }
                IDesignerHost host = (IDesignerHost)provider.GetService(typeof(IDesignerHost));
                if (host == null)
                {
                    return value;
                }
                if (dialog == null)
                {
                    dialog = new FormTreeColumnCollectionEditor();
                }
                dialog.Tree = (TreeList)context.Instance;
                using (var transaction = host.CreateTransaction("TreeColumnCollectionTransaction"))
                {
                    if (service.ShowDialog(dialog) == DialogResult.OK)
                    {
                        transaction.Commit();
                        return value;
                    }
                    transaction.Cancel();
                }
            }
            return value;
        }
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }
}
