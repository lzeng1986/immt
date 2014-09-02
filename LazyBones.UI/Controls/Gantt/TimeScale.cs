using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using LazyBones.Extensions;
using LazyBones.Linq;

namespace LazyBones.UI.Controls.Gantt
{
    [TypeConverter(typeof(Converter)), DesignTimeVisible(false), Browsable(false)]
    class TimeScale : Component
    {
        static readonly SortedDictionary<DayOfWeek, string> ShortDays = new SortedDictionary<DayOfWeek, string>
        {
            {DayOfWeek.Sunday, "日"},
            {DayOfWeek.Monday, "一"},
            {DayOfWeek.Tuesday, "二"},
            {DayOfWeek.Wednesday, "三"},
            {DayOfWeek.Thursday, "四"},
            {DayOfWeek.Friday, "五"},
            {DayOfWeek.Saturday, "六"}
        };

        public TimeScale(IContainer container)
        {
            container.Add(this);
            UpdatePerHourWidth();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                borderPen.Dispose();
                fontBrush.Dispose();
                head1Format.Dispose();
                head2Format.Dispose();
            }
        }

        Font font = SystemFonts.DialogFont;
        public Font Font
        {
            get { return font; }
            set { font = value; }
        }

        public int HeaderHeight
        {
            get { return header1Height + header2Height; }
        }
        int header1Height = 20;
        public int Header1Height
        {
            get { return header1Height; }
            set { header1Height = value; }
        }

        int header2Height = 20;
        public int Header2Height
        {
            get { return header2Height; }
            set { header2Height = value; }
        }

        Pen borderPen = new Pen(SystemColors.ActiveBorder);

        SolidBrush fontBrush = new SolidBrush(SystemColors.ControlText);
        public Color ForeColor
        {
            get { return fontBrush.Color; }
            set { fontBrush.Color = value; }
        }

        StringFormat head1Format = new StringFormat().FillAligment(ContentAlignment.MiddleCenter);
        public ContentAlignment Head1TextAlign
        {
            get { return head1Format.ToContentAlign(); }
            set { head1Format.FillAligment(value); }
        }

        StringFormat head2Format = new StringFormat().FillAligment(ContentAlignment.MiddleCenter);
        public ContentAlignment Head2TextAlign
        {
            get { return head2Format.ToContentAlign(); }
            set { head2Format.FillAligment(value); }
        }

        int periodWidth = 30;
        public int PeriodWidth
        {
            get { return periodWidth; }
            set
            {
                periodWidth = value;
                UpdatePerHourWidth();
            }
        }

        TimeDisplayScale timeDisplayScale = TimeDisplayScale.Day;
        public TimeDisplayScale DisplayScale
        {
            get { return timeDisplayScale; }
            set
            {
                timeDisplayScale = value;
                UpdatePerHourWidth();
            }
        }
        float perHourWidth;//一个小时的现实宽度（像素）
        void UpdatePerHourWidth()
        {
            switch (timeDisplayScale)
            {
                case TimeDisplayScale.Hour:
                    perHourWidth = periodWidth;
                    break;
                case TimeDisplayScale.Day:
                    perHourWidth = periodWidth / 24f;
                    break;
                case TimeDisplayScale.Month:
                    perHourWidth = periodWidth / 720f;//720 = 24*30
                    break;
            }
            perHourWidth *= zoom;
        }
        float zoom = 1f;
        public void ZoomIn()
        {
            zoom *= 1.1f;
            UpdatePerHourWidth();
        }
        public void ZoomOut()
        {
            if (30 >= periodWidth * zoom * 0.9)
                return;
            zoom *= 0.9f;
            UpdatePerHourWidth();
        }
        DateTime startTime = DateTime.Now;
        public DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        HeaderInfo headerInfo = new HeaderInfo();
        internal void Generate(IViewport viewport)
        {
            headerInfo.H1Rect = new RectangleF(viewport.X, viewport.Y, viewport.ViewRectangle.Width, Header1Height);
            headerInfo.H2Rect = new RectangleF(viewport.X, viewport.Y + Header1Height, viewport.ViewRectangle.Width, Header2Height);

            headerInfo.Clear();

            //head2绘制的是最小时间刻度，head1绘制比最小时间刻度大一级的刻度
            //生成head2及column
            //时间刻度始终以小时为单位，只是在显示的时候使用不同的现实比例

            GenerateHeader2(viewport);
            GenerateHeader1(viewport);
        }
        void GenerateHeader2(IViewport viewport)
        {
            var curDate = CalculateViewStartTime(viewport);
            var left = (curDate - startTime).TotalHours * perHourWidth;

            var h2LabelTop = viewport.Y + Header1Height;
            var columnsTop = h2LabelTop + Header2Height;

            while (left < viewport.ViewRectangle.Right)
            {
                var width = GetColumnWidth(curDate);
                headerInfo.H2Dates.Add(curDate);
                headerInfo.H2LabelRects.Add(new RectangleF((float)left, h2LabelTop, width, Header2Height));
                headerInfo.Columns.Add(new RectangleF((float)left, columnsTop, width, viewport.ViewRectangle.Height));
                curDate = NextTime(curDate);
                left += width;
            }
        }
        float GetColumnWidth(DateTime time)
        {
            if (timeDisplayScale == TimeDisplayScale.Month)
            {
                return DateTime.DaysInMonth(time.Year, time.Month) * 24 * perHourWidth;
            }
            else if (timeDisplayScale == TimeDisplayScale.Hour)
            {
                return perHourWidth;
            }
            else
            {
                return perHourWidth * 24;
            }
        }
        DateTime CalculateViewStartTime(IViewport viewport)
        {
            var start = startTime.AddHours(viewport.X / perHourWidth);
            switch (DisplayScale)
            {
                case TimeDisplayScale.Hour:
                    return start.Date.AddHours(start.Hour);
                case TimeDisplayScale.Day:
                    return start.Date;
                case TimeDisplayScale.Month://获取该月的第一天
                    return start.Date.AddDays(-start.Day + 1);
                default:
                    return StartTime;
            }
        }
        DateTime NextTime(DateTime time)
        {
            switch (timeDisplayScale)
            {
                case TimeDisplayScale.Hour:
                    return time.AddHours(1);
                case TimeDisplayScale.Day:
                    return time.AddDays(1);
                case TimeDisplayScale.Month:
                    return time.AddMonths(1);
                default:
                    return time;
            }
        }
        void GenerateHeader1(IViewport viewport)
        {
            if (headerInfo.H2LabelRects.Count <= 0)
                return;
            if (timeDisplayScale == TimeDisplayScale.Day)
            {
                var day = headerInfo.H2Dates[0].Day - 1;
                var curDate = headerInfo.H2Dates[0].AddDays(-day);
                var x = (curDate - startTime).TotalHours * perHourWidth;
                while (x < viewport.ViewRectangle.Right)
                {
                    headerInfo.H1Dates.Add(curDate);
                    var days = DateTime.DaysInMonth(curDate.Year, curDate.Month);
                    var width = days * perHourWidth * 24;
                    headerInfo.H1LabelRects.Add(new RectangleF((float)x, viewport.Y, width, Header1Height));
                    curDate = curDate.AddDays(days);
                    x += width;
                }
            }
            else if (timeDisplayScale == TimeDisplayScale.Hour)
            {
                var curDate = headerInfo.H2Dates[0].Date;
                var width = perHourWidth * 24;
                for (var x = GetTimePeriod(curDate) * perHourWidth; x < viewport.ViewRectangle.Right; x += width)
                {
                    headerInfo.H1Dates.Add(curDate);
                    headerInfo.H1LabelRects.Add(new RectangleF((float)x, viewport.Y, width, Header1Height));
                    curDate = curDate.AddDays(1);
                }
            }
            else
            {
                var curDate = new DateTime(headerInfo.H2Dates[0].Year, 1, 1);
                var x = (curDate - startTime).TotalHours * perHourWidth;
                while (x < viewport.ViewRectangle.Right)
                {
                    headerInfo.H1Dates.Add(curDate);
                    var width = (DateTime.IsLeapYear(curDate.Year) ? 366 : 365) * 24 * perHourWidth;
                    headerInfo.H1LabelRects.Add(new RectangleF((float)x, viewport.Y, width, Header1Height));
                    curDate = curDate.AddYears(1);
                    x += width;
                }
            }
        }
        internal void DrawHeader(Graphics g)
        {
            g.FillRectangles(SystemBrushes.Control, new[] { headerInfo.H1Rect, headerInfo.H2Rect });
            g.DrawRectangles(borderPen, new[] { headerInfo.H1Rect, headerInfo.H2Rect });
            DrawHeader1(g);
            DrawHeader2(g);
        }
        void DrawHeader1(Graphics graphics)
        {
            if (headerInfo.H1LabelRects.Count > 0)
                graphics.DrawRectangles(borderPen, headerInfo.H1LabelRects.ToArray());
            for (int i = 0; i < headerInfo.H1LabelRects.Count; i++)
            {
                var text = GetHeader1Text(headerInfo.H1Dates[i]);
                graphics.DrawString(text, font, fontBrush, headerInfo.H1LabelRects[i], head1Format);
            }
        }
        string GetHeader1Text(DateTime time)
        {
            switch (DisplayScale)
            {
                case TimeDisplayScale.Hour:
                    return time.ToString("yyyy年M月d日");
                case TimeDisplayScale.Day:
                    return time.ToString("yyyy年M月");
                case TimeDisplayScale.Month:
                    return time.ToString("yyyy年");
                default:
                    return string.Empty;
            }
        }
        void DrawHeader2(Graphics graphics)
        {
            for (int i = 0; i < headerInfo.H2LabelRects.Count; i++)
            {
                var text = GetHeader2Text(headerInfo.H2Dates[i]);
                graphics.DrawString(text, font, fontBrush, headerInfo.H2LabelRects[i], head2Format);
            }
        }
        GregorianCalendar calendar = new GregorianCalendar(GregorianCalendarTypes.Localized);
        string GetHeader2Text(DateTime time)
        {
            switch (DisplayScale)
            {
                case TimeDisplayScale.Hour:
                    return time.ToString("H时");
                case TimeDisplayScale.Day:
                    return time.ToString("d日");
                case TimeDisplayScale.Month:
                    return time.ToString("M月");
                default:
                    return string.Empty;
            }
        }
        internal void DrawColumns(Graphics graphics)
        {
            //画列的边框
            if (headerInfo.Columns.Count > 0)
                graphics.DrawRectangles(borderPen, headerInfo.Columns.ToArray());
            if (timeDisplayScale == TimeDisplayScale.Month)//时间刻度为月，则不绘制周末
                return;
            //用不同的纹理画出周末的列
            using (var pattern = new HatchBrush(HatchStyle.Percent20, borderPen.Color, Color.Transparent))
            {
                var rects = headerInfo.Columns.Where((c, i) =>
                {
                    var weekDay = headerInfo.H2Dates[i].DayOfWeek;
                    return weekDay == DayOfWeek.Sunday || weekDay == DayOfWeek.Saturday;
                }).ToArray();
                if (rects.Length > 0)
                    graphics.FillRectangles(pattern, rects);
            }
        }

        public int GetDeviceColumnAt(PointF location)
        {
            return headerInfo.Columns.IndexOf(col => col.Contains(location));
        }
        public DateTime GetTimeInColumn(int col)
        {
            return headerInfo.H2Dates[col];
        }
        public bool Contains(PointF location)
        {
            return headerInfo.H1Rect.Contains(location) || headerInfo.H2Rect.Contains(location);
        }
        public DateTime GetTime(double period)
        {
            switch (timeDisplayScale)
            {
                case TimeDisplayScale.Hour:
                    return this.StartTime.AddHours(period);
                case TimeDisplayScale.Day:
                    return this.StartTime.AddDays(period);
                case TimeDisplayScale.Month:
                    if (period >= 0)
                        return this.StartTime.AddMonths((int)Math.Ceiling(period));
                    else
                        return this.StartTime.AddMonths((int)Math.Floor(period));
                default:
                    return DateTime.Now;
            }
        }
        public float GetOffset(DateTime time)
        {
            return (float)(time - startTime).TotalHours * perHourWidth;
        }
        public float GetOffset(TimeSpan span)
        {
            return (float)span.TotalHours * perHourWidth;
        }
        public TimeSpan GetOffset(float offset)
        {
            return TimeSpan.FromHours(offset / perHourWidth);
        }
        public double GetTimePeriod(DateTime time)//均以小时为周期
        {
            return (time - startTime).TotalHours;
            //var span = time - startTime;
            //switch (timeDisplayScale)
            //{
            //    case TimeDisplayScale.Hour:
            //        return span.TotalHours;
            //    case TimeDisplayScale.Day:
            //        return span.TotalDays;
            //    case TimeDisplayScale.Month:
            //        var t1 = time
            //        return (time.Year - startTime.Year) * 12 + time.Month - startTime.Month;
            //    default:
            //        return 0;
            //}
        }
    }

    public enum TimeDisplayScale
    {
        Hour,
        Day,
        Month
    }
}
