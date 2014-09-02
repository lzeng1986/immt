using System.Drawing;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.TextEditor
{
    /// <summary>
    /// 水平标尺
    /// </summary>
    class HorizontalRuler : Control
    {
        TextArea textArea;

        public HorizontalRuler(TextArea textArea)
        {
            this.textArea = textArea;
            BackColor = Color.WhiteSmoke;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            int num = 0;
            for (var x = textArea.TextMargin.DisplayBounds.Left; x < textArea.TextMargin.DisplayBounds.Right; x += textArea.TextMeasurer.WideSpaceWidth)
            {
                int offset = (Height * 2) / 3;
                if (num % 5 == 0)
                {
                    offset = (Height * 4) / 5;
                }

                if (num % 10 == 0)
                {
                    offset = 1;
                }
                ++num;
                g.DrawLine(Pens.Black, (int)x, offset, (int)x, Height - offset);
            }
        }
    }
}
