using System.Linq;

namespace LazyBones.UI.Controls.Tree
{
    public class TreePath
    {
        static TreePath empty = new TreePath();
        public static TreePath Empty
        {
            get { return empty; }
        }

        object[] path;
        public object[] FullPath
        {
            get { return path; }
        }

        public object LastNode
        {
            get { return path.LastOrDefault(); }
        }

        public object FirstNode
        {
            get { return path.FirstOrDefault(); }
        }

        public TreePath()
        {
            path = new object[0];
        }

        public TreePath(object node)
        {
            path = new object[] { node };
        }

        public TreePath(object[] path)
        {
            this.path = path;
        }

        public bool IsEmpty
        {
            get { return path.Length == 0; }
        }
    }
}
