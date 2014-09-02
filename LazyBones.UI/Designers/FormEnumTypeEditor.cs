using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Threading;
using LazyBones.Linq;

namespace LazyBones.UI.Designers
{
    partial class FormEnumTypeEditor : Form
    {
        public Assembly Ass { get; set; }
        public Type EnumType { get; private set; }
        public FormEnumTypeEditor()
        {
            InitializeComponent();
            buttonOK.Enabled = false;
        }
        
        private void FormEnumTypeEditor_Load(object sender, EventArgs e)
        {
            this.treeView1.UseWaitCursor = true;
            //后台加载
            ThreadPool.QueueUserWorkItem(s =>
            {
                Thread.Sleep(500);//暂定保证窗口显示
                this.Invoke(new Action(this.LoadNodes));
            });
        }
        void LoadNodes()
        {
            var node = new TreeNode(Ass.GetName().Name);
            node.ImageIndex = 3;
            node.SelectedImageIndex = 3;
            AddSubNode(node, Ass);
            if (node.Nodes.Count > 0)
            {
                this.treeView1.Nodes.Add(node);
                this.treeView1.Update();
            }   
            foreach (var a in Ass.GetReferencedAssemblies().OrderBy(a => a.Name))
            {
                var n = new TreeNode(a.Name);
                n.ImageIndex = 0;
                n.SelectedImageIndex = 0;
                AddSubNode(n, Assembly.Load(a));
                if (n.Nodes.Count > 0)
                {
                    this.treeView1.Nodes.Add(n);
                    this.treeView1.Update();
                }   
            }
            this.treeView1.UseWaitCursor = false;
            this.treeView1.SelectedNode = node;
        }
        void AddSubNode(TreeNode node, Assembly assembly)
        {
            foreach (var g in assembly.GetExportedTypes().Where(t=>t.IsEnum).GroupBy(t => t.Namespace).OrderBy(t => t.Key))
            {
                var subNode = node.Nodes.Add(g.Key);
                subNode.ImageIndex = 1;
                subNode.SelectedImageIndex = 1;
                subNode.Nodes.AddRange(g.OrderBy(t => t.Name).Select(t => 
                    new TreeNode(t.Name) { 
                        Tag = t,
                        ImageIndex = 2, 
                        SelectedImageIndex=2,
                        ToolTipText = GetTooltipText(t)
                    }).ToArray());
            }
        }
        string GetTooltipText(Type type)
        {
            var sb = new StringBuilder();
            if (type.IsDefined(typeof(FlagsAttribute), false))
                sb.AppendLine("[FlagsAttribute]");
            var description = Attribute.GetCustomAttribute(type, typeof(DescriptionAttribute)) as DescriptionAttribute;
            if (description != null)
                sb.AppendLine(string.Format("{0}({1})",type.Name, description.Description));
            else
                sb.AppendLine(type.Name);
            sb.AppendLine("枚举值：");            
            sb.Append(
                Enum.GetValues(type).Cast<object>().Select(v =>
                {
                    var str = "\t" + v.ToString();
                    var member = type.GetMember(v.ToString());
                    if (member.Any())
                    {
                        var d = Attribute.GetCustomAttribute(member[0], typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (d != null)
                            str+= string.Format("({0})",d.Description);
                    }
                    return str;
                }).JoinToString()
            );
            return sb.ToString();
        }
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag != null)
            {
                EnumType = (Type)e.Node.Tag;
                DialogResult = DialogResult.OK;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            buttonOK.Enabled = e.Node.Tag != null;
            if (e.Node.Tag != null)
            {
                EnumType = (Type)e.Node.Tag;
            }
        }
    }
}
