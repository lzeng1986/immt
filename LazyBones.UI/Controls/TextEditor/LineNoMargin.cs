using System.Drawing;
using System.Windows.Forms;
using LazyBones.Extensions;

namespace LazyBones.UI.Controls.TextEditor
{
    class LineNoMargin : MarginBase
    {
        static Cursor LineNoCursor = new Cursor(typeof(LineNoMargin), "LineNo.cur");
        public LineNoMargin(TextArea textArea)
            : base(textArea)
        {
        }
        public override Cursor Cursor
        {
            get { return LineNoCursor; }
        }
        public override bool Visible
        {
            get { return textArea.EditorOptions.LineNoVisible; }
        }
        //靠右居中
        const TextFormatFlags LineNoTextFormat = TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoClipping;
        //绘制行号
        public override void Paint(Graphics g, Rectangle rect)
        {
            if (rect.Width == 0 || rect.Height == 0)
                return;
            var lineNoDrawInfo = textArea.Document.TextDrawInfoManager["LineNo"];
            var fontHeight = textArea.TextMeasurer.FontHeight;
            var bgBrush = textArea.Enabled ? BrushPool.GetBrush(lineNoDrawInfo.BackColor) : SystemBrushes.InactiveBorder;
            var lineCount = (DisplayBounds.Height + textArea.TextMargin.VisibleLineDrawingRemainder) / fontHeight + 1;
            var firstLineNo = textArea.Document.GetVisibleLine(textArea.TextMargin.FirstVisibleLine);
            var font = textArea.EditorOptions.Font[lineNoDrawInfo];

            var drawRect = new Rectangle(DisplayBounds.X, DisplayBounds.Y, DisplayBounds.Width, fontHeight);
            drawRect.Offset(0, textArea.TextMargin.VisibleLineDrawingRemainder);

            for (var i = 0; i < lineCount; ++i)
            {
                if (rect.IntersectsWith(drawRect))
                {
                    g.FillRectangle(bgBrush, drawRect);
                    int n = textArea.Document.GetFirstLogicalLine(firstLineNo + i);
                    if (n < textArea.Document.TotalOfLine)
                    {
                        TextRenderer.DrawText(g, (n + 1).ToString(), font, drawRect, lineNoDrawInfo.ForeColor, LineNoTextFormat);
                    }
                }
                drawRect.Y += fontHeight;
            }
        }
        //点击左键时选择文本
        public override void MouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
                return;
            int realLineNo = textArea.TextMargin.GetLogicalLineNo(e.Y);
            if (realLineNo < 0 || textArea.Document.TotalOfLine <= realLineNo)
                return;
            var mousePos = textArea.MouseLocation;
            textArea.SelectionManager.SelectFrom.Where = WhereFrom.LineNo;
            if (Control.ModifierKeys.HasFlag(Keys.Shift))
            {
                if (textArea.SelectionManager.IsEmpty)
                    SelectSingleLine(realLineNo);
                else
                    ExtendSelection();
            }
            else
            {
                textArea.SelectionManager.ClearSelection();
                SelectSingleLine(realLineNo);
            }
            textArea.SetDesiredColumn();
        }
        void SelectSingleLine(int lineNo)//选择范围是当前行开始至下一行开始，最后一行到行的结束位置
        {
            var endLocation = textArea.SelectionManager.NextValidPosition(lineNo);
            textArea.SelectionManager.SetSelection(new TextLocation(0, lineNo), endLocation);
            textArea.Caret.Location = endLocation;
            textArea.SelectionManager.SelectionStart = new TextLocation(0, lineNo);
        }
        void ExtendSelection()
        {
            var mousePos = textArea.MouseLocation;
            var oldPos = textArea.Caret.Location;
            if (mousePos.Line < textArea.SelectionManager.SelectionStart.Line)
                textArea.Caret.Location = new TextLocation(0, mousePos.Line);
            else
                textArea.Caret.Location = textArea.SelectionManager.NextValidPosition(mousePos.Line);
            textArea.SelectionManager.ExtendSelectionTo(textArea.Caret.Location);
        }
        public override void MouseMove(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                ExtendSelection();
        }
    }
}