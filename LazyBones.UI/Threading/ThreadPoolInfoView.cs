using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace LazyBones.Threading
{
    class ThreadPoolInfoView : Control
    {
        const int MaxValueLength = 1000;
        const int GridSize = 20;
        int[] value1 = new int[MaxValueLength];
        int[] value2 = new int[MaxValueLength];
        int startInd = 0;
        int endInd = 0;
        int maxValue = 100;
        int shiftStep = 2;
        int valueCount = 0;
        public ThreadPoolInfoView()
        {
            BackColor = Color.Black;
            SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.AllPaintingInWmPaint, true);
            UpdateStyles();
        }
        protected override System.Drawing.Size DefaultSize
        {
            get
            {
                return new System.Drawing.Size(200, 100);
            }
        }
        protected override Size DefaultMinimumSize
        {
            get
            {
                return new System.Drawing.Size(GridSize * 2, GridSize * 2);
            }
        }
        public override Size MinimumSize
        {
            get
            {
                return base.MinimumSize;
            }
            set
            {
                if (value.Height < GridSize * 2)
                    value.Height = GridSize * 2;
                base.MinimumSize = value;
            }
        }
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);
            shiftStep = Width / maxValue;
            if (shiftStep == 0)
                shiftStep = 1;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //Draw grid
            for (var x = GridSize; x < Width; x += GridSize)
            {
                e.Graphics.DrawLine(Pens.Green, x, 0, x, Height);
            }
            for (var y = GridSize; y < Height; y += GridSize)
            {
                e.Graphics.DrawLine(Pens.Green, 0, y, Width, y);
            }

            if (valueCount < 2)
                return;

            e.Graphics.DrawLines(Pens.Red, GetPoints(value1).ToArray());
            e.Graphics.DrawLines(Pens.Yellow, GetPoints(value2).ToArray());

        }
        IEnumerable<Point> GetPoints(int[] value)
        {
            var ind = startInd;
            for (var i = 0; i < valueCount; i++)
            {
                yield return new Point(Width - i * shiftStep, GetRelativeValue(value[ind]));
                ind = startInd - i;
                if (ind < 0)
                    ind += MaxValueLength;
            }
        }
        private int GetRelativeValue(int val)
        {
            return Height - val * Height / maxValue - GridSize;
        }
        public void AddValue(int v1, int v2)
        {
            value1[endInd] = v1;
            value2[endInd] = v2;

            valueCount++;
            if (valueCount >= MaxValueLength)
            {
                valueCount = MaxValueLength;
            }
            endInd++;
            if (endInd >= MaxValueLength)
            {
                endInd -= MaxValueLength;
            }
            startInd = endInd - 1;
            if(startInd < 0)
                startInd += MaxValueLength;
            Refresh();
            //maxValue = Math.Max()
        }
    }
}
