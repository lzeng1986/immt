using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using LazyBones.Extensions;
using LazyBones.Linq;
using LazyBones.UI.Controls.List.Columns;

namespace LazyBones.UI.Controls.List
{
    public enum ColumnStates
    {
        None,
        Pressed,
        HotTrace
    }
    [ToolboxItem(false), DesignTimeVisible(false)]
    public abstract class Column : Component
    {
        public Column()
        {
        }

        ListView parent = null;

        [Browsable(false)]
        public ListView Parent
        {
            get { return parent; }
            internal set { parent = value; }
        }

        [Browsable(false)]
        public int Index { get; internal set; }

        [Browsable(false)]
        public virtual object CellDefaultValue
        {
            get { return "DefaultText"; }
        }

        ContentAlignment textAlignment = ContentAlignment.MiddleLeft;
        [Browsable(true), Category("Appearance")]
        public ContentAlignment ContentAlign
        {
            get { return textAlignment; }
            set
            {
                if (textAlignment != value)
                {
                    textAlignment = value;
                    Invalidate();
                }
            }
        }

        ContentAlignment headTextAlignment = ContentAlignment.MiddleLeft;
        [Browsable(true), Category("Appearance")]
        public ContentAlignment HeadTextAlign
        {
            get { return headTextAlignment; }
            set
            {
                if (headTextAlignment != value)
                {
                    headTextAlignment = value;
                    Invalidate();
                }
            }
        }

        int width = 100;
        [Category("Appearance"), DefaultValue(100)]
        public int Width
        {
            get { return width; }
            set
            {
                if (width != value)
                {
                    width = value;
                    Invalidate();
                }
            }
        }

        bool editable = true;
        [DefaultValue(true), Category("Action")]
        public bool Editable
        {
            get { return editable; }
            set { editable = value; }
        }

        Font font = SystemFonts.DefaultFont;
        [DefaultValue(typeof(Font), "DefaultFont"), Category("Appearance")]
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        string headText = "Column";
        [Browsable(true), Category("Appearance"), DefaultValue("Column")]
        public string HeadText
        {
            get { return headText; }
            set
            {
                if (headText != value)
                {
                    headText = value;
                    Invalidate();
                }
            }
        }

        Color backColor = Color.Transparent;
        [DefaultValue(typeof(Color), "Transparent"), Category("Appearance")]
        public Color BackColor
        {
            get { return backColor; }
            set
            {
                if (backColor != value)
                {
                    backColor = value;
                    Invalidate();
                }
            }
        }

        Color foreColor = SystemColors.WindowText;
        [DefaultValue(typeof(Color), "WindowText"), Category("Appearance")]
        public Color ForeColor
        {
            get { return foreColor; }
            set
            {
                if (foreColor != value)
                {
                    foreColor = value;
                    Invalidate();
                }
            }
        }

        void Invalidate()
        {
            if (parent != null)
                parent.Invalidate();
        }
        protected virtual Type ValueType
        {
            get { return typeof(string); }
        }
        internal protected virtual Control EditControl
        {
            get { return null; }
        }
        [Category("Data")]
        public string Format { get; set; }

        public virtual void DrawHeader(Graphics g, Rectangle bounds)
        {
            if (parent == null)
                return;
            g.DrawRectangle(SystemPens.ActiveBorder, bounds);
            using (var textBrush = new SolidBrush(parent.ForeColor))
            using (var sf = new StringFormat().FillAligment(headTextAlignment))
            {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(headText, parent.Font, textBrush, bounds, sf);
            }
        }
        string GetCellText(ItemCell cell)
        {
            var val = cell.Value ?? CellDefaultValue;
            if (val == null)
                return string.Empty;
            var formatter = val as IFormattable;
            return formatter == null ? val.ToString() : formatter.ToString(Format, null);
        }
        public virtual void DrawCell(Graphics g, CellPaintStyle style)
        {
            if (backColor != parent.BackColor)
            {
                using (var brush = new SolidBrush(backColor))
                {
                    g.FillRectangle(brush, style.Cell.CellBounds);
                }
            }
            DrawCellText(g, style);
        }
        protected void DrawCellText(Graphics g, CellPaintStyle style)
        {
            using (var sf = new StringFormat().FillAligment(textAlignment))
            {
                sf.Trimming = StringTrimming.EllipsisCharacter;
                sf.FormatFlags = StringFormatFlags.NoWrap;
                g.DrawString(GetCellText(style.Cell), style.Font, style.ForeBrush, style.Cell.CellBounds, sf);
            }
        }
        public virtual void OnMouseEnter()
        {
        }
        public virtual void OnMouseLeave()
        {
        }
        public virtual void OnMouseDown(MouseEventArgs e)
        {
        }
        public virtual void OnMouseUp(MouseEventArgs e)
        {
        }
        public virtual void OnMouseClick(MouseEventArgs e)
        {
        }
        public virtual void OnMouseDoubleClick(MouseEventArgs e)
        {
        }
    }

    [EditorAttribute(typeof(ColumnsEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public class ColumnCollection : CollectionBase, IEnumerable<Column>
    {
        ListView listView;
        public ColumnCollection(ListView listView)
        {
            this.listView = listView;
        }
        public Column this[int index]
        {
            get { return (Column)List[index]; }
        }

        public Column this[string columnName]
        {
            get { return this.FirstOrDefault(c => c.Site != null && c.Site.Name == columnName); }
        }

        public int GetColumnIndex(string columnName)
        {
            return this.IndexOf(c => c.Site != null && c.Site.Name == columnName);
        }
        public int Width
        {
            get { return this.Sum(c => c.Width); }
        }

        public int GetSpanSize(string columnName, int columnsSpanned)
        {
            return this.SkipWhile(c => c.Site != null && c.Site.Name == columnName)
                .Take(columnsSpanned)
                .Sum(c => c.Width);
        }

        public void Add(Column newColumn)
        {
            newColumn.Parent = listView;
            List.Add(newColumn);
        }
        protected override void OnInsertComplete(int index, object value)
        {
            base.OnInsertComplete(index, value);
            (value as Column).Parent = listView;
            Modify(index);
            listView.SuspendPaint();
            foreach (var row in listView.Rows)
                row.Cells.Insert(index, new ItemCell());
            listView.CalcScroll();
            listView.ResumePaint();
        }
        protected override void OnRemoveComplete(int index, object value)
        {
            base.OnRemoveComplete(index, value);
            (value as Column).Parent = null;
            Modify(index);
            listView.SuspendPaint();
            foreach (var row in listView.Rows)
                row.Cells.RemoveAt(index);
            listView.CalcScroll();
            listView.ResumePaint();
        }
        void Modify(int fromIndex)
        {
            var ind = fromIndex;
            foreach (var col in this.Skip(fromIndex))
            {
                col.Index = ind;
                ind++;
            }
        }
        protected override void OnClearComplete()
        {
            base.OnClearComplete();
            listView.SuspendPaint();
            foreach (var row in listView.Rows)
                row.Cells.Clear();
            listView.CalcScroll();
            listView.ResumePaint();
        }
        public void AddRange(IEnumerable<Column> columns)
        {
            foreach (var c in columns)
                List.Add(c);
        }

        public int IndexOf(Column column)
        {
            return List.IndexOf(column);
        }

        public new IEnumerator<Column> GetEnumerator()
        {
            foreach (var col in InnerList)
                yield return col as Column;
        }
    }

    class ColumnsEditor : CollectionEditor
    {
        public ColumnsEditor()
            : base(typeof(ColumnCollection))
        {
        }
        protected override CollectionEditor.CollectionForm CreateCollectionForm()
        {
            return base.CreateCollectionForm();
        }
        protected override Type[] CreateNewItemTypes()
        {
            //var designerHost = Context.GetService(typeof(IDesignerHost)) as IDesignerHost;
            //if (designerHost != null)
            //{
            //    var type = designerHost.GetType(designerHost.RootComponentClassName);
            //    var callAssembly = type.Assembly;
            //    var types = callAssembly.GetTypes().Concat(
            //        callAssembly.GetReferencedAssemblies().SelectMany(a => Assembly.Load(a).GetTypes())
            //        )
            //        .Where(t => t.BaseType == typeof(ListColumn))
            //        //.Where(typeof(ListColumn).IsAssignableFrom)
            //        .ToArray();
            //    foreach (var t in types)
            //        MessageBox.Show(t.ToString());
            //    return types;
            //}
            //else
            //    return base.CreateNewItemTypes();
            return new[] { 
                typeof(TextColumn),
                typeof(ButtonColumn),
                typeof(ImageColumn),
                typeof(DropDownColumn),
                typeof(ComboxColumn)
            };
        }
    }


}
