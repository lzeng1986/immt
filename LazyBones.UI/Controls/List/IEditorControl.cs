
using System.Windows.Forms;
namespace LazyBones.UI.Controls.List
{
    public interface IEditorControl
    {
        object Value { get; set; }

        bool Load(ListRow item, ItemCell subItem, ListView listView);

        void Unload();

        bool ShouldCloseEditor(Keys key);
    }
}
