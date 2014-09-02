using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;

namespace LazyBones.UI.Controls.Editor
{
    //表示光标
    public class Caret : IDisposable
    {
        TextArea textArea;
        Point caretPosition = Point.Empty;
        Size caretSize = Size.Empty;
        ICaretDrawer caretDrawer;
        static bool caretCreated = false;

        public Caret(TextArea textArea)
        {
            this.textArea = textArea;
            textArea.GotFocus += painter_GotFocus;
            textArea.LostFocus += painter_LostFocus;
            caretDrawer = new ManagedCaret(this, textArea);
        }
        void painter_LostFocus(object sender, EventArgs e)
        {
            visible = true;
            if (!textArea.TextEditor.IsInUpdate)
            {
                CreateCaret();
                UpdateCaretPosition();
            }
        }
        void painter_GotFocus(object sender, EventArgs e)
        {
            visible = false;
            DisposeCaret();
        }
        public Point ScreenPosition
        {
            get
            {
                var xpos = textArea.TextMeasurer.GetDrawingXPos(this.line, this.column);
                return new Point(textArea.TextMargin.DisplayBounds.X + xpos,
                                 textArea.TextMargin.DisplayBounds.Y
                                 + (textArea.Document.GetVisibleLine(this.line)) * textArea.TextMeasurer.FontHeight
                                 - textArea.TextMargin.TextArea.VisualLocation.Y);
            }
        }
        void CreateCaret()
        {
            switch (caretMode)
            {
                case CaretMode.Insert:
                    caretCreated = caretDrawer.SetSize(1, textArea.TextMeasurer.FontHeight);
                    break;
                case CaretMode.Overwrite:
                    caretCreated = caretDrawer.SetSize(textArea.TextMeasurer.SpaceWidth, textArea.TextMeasurer.FontHeight);
                    break;
            }
            if (caretPosition.X < 0)
            {
                ValidateCaretPos();
                caretPosition = ScreenPosition;
            }
            caretDrawer.SetLocation(caretPosition.X, caretPosition.Y);
            caretDrawer.Show();
        }

        public void RecreateCaret()
        {
            DisposeCaret();
            if (visible)
                CreateCaret();
        }
        void DisposeCaret()
        {
            if (caretCreated)
            {
                caretCreated = false;
                caretDrawer.Hide();
                caretDrawer.Dispose();
            }
        }
        bool disposed = false;
        public void Dispose()
        {
            if (disposed)
                return;
            textArea.GotFocus -= painter_GotFocus;
            textArea.LostFocus -= painter_LostFocus;
            disposed = true;
        }
        public void ValidateCaretPos()
        {
            line = Math.Max(0, line);
            line = Math.Min(textArea.Document.TotalOfLine - 1, line);
            column = Math.Max(0, column);
            column = Math.Min(column, textArea.Document.GetLineByLineNo(line).Length);
        }
        public TextLocation ValidatePosition(TextLocation position)
        {
            int line = Math.Max(0, Math.Min(textArea.Document.TotalOfLine - 1, position.Line));
            int column = Math.Max(0, position.Column);
            column = Math.Min(column, textArea.Document.GetLineByLineNo(line).Length);
            return new TextLocation(column, line);
        }
        bool visible = true;
        int line = 0;
        int oldLine = -1;
        public int Line
        {
            get { return line; }
            set
            {
                if (line == value)
                    return;
                line = value;
                UpdateCaretPosition();
            }
        }
        int column = 0;
        public int Column
        {
            get { return column; }
            set
            {
                if (column == value)
                    return;
                column = value;
                UpdateCaretPosition();
            }
        }
        TextLocation location = TextLocation.Zero;
        public TextLocation Location
        {
            get { return location; }
            set
            {
                if (location == value)
                    return;
                location = value;
                line = value.Line;
                column = value.Column;
                UpdateCaretPosition();
            }
        }
        public int DesiredColumn { get; set; }
        CaretMode caretMode;
        public CaretMode Mode
        {
            get { return caretMode; }
            set
            {
                if (caretMode == value)
                    return;
                caretMode = value;
                OnCaretModeChanged();
            }
        }

        public int Offset
        {
            get { return textArea.Document.LocationToOffset(location); }
        }
        bool outstandingUpdate;
        public void UpdateCaretPosition()
        {
            textArea.UpdateLine(oldLine);
            if (line != oldLine)
                textArea.UpdateLine(line);

            oldLine = line;

            if (!visible || textArea.TextEditor.IsInUpdate)
            {
                outstandingUpdate = true;
                return;
            }

            outstandingUpdate = false;
            ValidateCaretPos();
            int xpos = textArea.TextMeasurer.GetDrawingXPos(this.line, this.column);
            Point pos = ScreenPosition;
            if (xpos >= 0)
            {
                CreateCaret();
                if (!caretDrawer.SetLocation(pos.X, pos.Y))
                {
                    caretDrawer.Dispose();
                    caretCreated = false;
                    UpdateCaretPosition();
                }
            }
            else
            {
                caretDrawer.Dispose();
            }

            caretPosition = pos;
        }
        internal void Paint(Graphics g)
        {
            caretDrawer.Paint(g);
        }
        public event EventHandler CaretLocationChanged;
        void OnCaretLocationChanged()
        {
            if (CaretLocationChanged != null)
                CaretLocationChanged(this, EventArgs.Empty);
        }
        void OnCaretModeChanged()
        {
            switch (caretMode)
            {
                case CaretMode.Insert:
                    caretSize.Width = 2;
                    break;
                case CaretMode.Overwrite:
                    caretSize.Width = textArea.TextMeasurer.SpaceWidth;
                    break;
            }
            caretDrawer.SetSize(caretSize.Width, caretSize.Height);
        }
    }
    public interface ICaretDrawer : IDisposable
    {
        bool SetSize(int width, int height);
        void Show();
        void Hide();
        bool SetLocation(int x, int y);
        void Paint(Graphics g);
    }
    class ManagedCaret : ICaretDrawer
    {
        Timer timer;
        bool blink = true;
        Rectangle bounds = Rectangle.Empty;
        TextArea textArea;
        Caret parentCaret;
        bool visible;

        public ManagedCaret(Caret caret, TextArea textArea)
        {
            this.textArea = textArea;
            this.parentCaret = caret;
            timer = new Timer { Interval = SystemInformation.CaretBlinkTime };
            timer.Tick += CaretTimerTick;
        }

        void CaretTimerTick(object sender, EventArgs e)
        {
            blink = !blink;
            if (visible)
                textArea.UpdateLine(parentCaret.Line);
        }

        public bool SetSize(int width, int height)
        {
            bounds.Width = width;
            bounds.Height = height;
            timer.Start();
            return true;
        }
        public bool SetLocation(int x, int y)
        {
            bounds.X = x;
            bounds.Y = y;
            return true;
        }
        public void Paint(Graphics g)
        {
            if (visible && blink)
                g.DrawRectangle(Pens.Black, bounds);
        }
        public void Dispose()
        {
            visible = false;
            timer.Dispose();
        }

        public void Show()
        {
            visible = true;
        }

        public void Hide()
        {
            visible = false;
        }
    }
    public enum CaretMode
    {
        Insert,
        Overwrite
    }
}
