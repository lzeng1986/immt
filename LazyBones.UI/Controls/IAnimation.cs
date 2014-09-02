using System;
using System.Windows.Forms;
using System.Drawing;

namespace LazyBones.UI.Controls
{
    interface IAnimation
    {
        void Init(Form form,int stepTotal);
        void Step(Form form);
        void End(Form form);
        event EventHandler Finished;
    }
    class FadeIn : IAnimation
    {
        float interval;
        public void Init(Form form, int stepTotal)
        {
            form.Opacity = 0f;
            interval = 1f / stepTotal;
        }
        public void Step(Form form)
        {
            form.Opacity += interval;
            if (form.Opacity >= 1f)
            {
                End(form);
            }
        }
        public void End(Form form)
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
            form.Opacity = 1f;
        }
        public event EventHandler Finished;
    }
    class FadeOut : IAnimation
    {
        float interval;
        public void Init(Form form, int stepTotal)
        {
            form.Opacity = 1f;
            interval = 1f / stepTotal;
        }
        public void Step(Form form)
        {
            form.Opacity -= interval;
            if (form.Opacity <= 0f)
            {
                End(form);
            }
        }
        public void End(Form form)
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
            form.Opacity = 0f;
        }
        public event EventHandler Finished;
    }
    class SlideDown : IAnimation
    {
        Rectangle rect;
        int step;
        public void Init(Form form, int stepTotal)
        {
            step = form.Height / stepTotal;
            rect = new Rectangle(0, 0, form.Width, 0);
        }
        public void Step(Form form)
        {
            rect.Height += step;
            if (rect.Height >= form.Height)
            {
                End(form);
            }
            else
            {
                form.Region = new Region(rect);
            }
        }
        public void End(Form form)
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
            form.Region = null;
        }
        public event EventHandler Finished;
    }
    class SlideUp : IAnimation
    {
        Rectangle rect;
        int step;
        public void Init(Form form, int stepTotal)
        {
            step = form.Height / stepTotal;
            rect = new Rectangle(Point.Empty, form.Size);
        }
        public void Step(Form form)
        {
            rect.Height -= step;
            if (rect.Height <= 0 )
            {
                End(form);
            }
            else
            {
                form.Region = new Region(rect);
            }
        }
        public void End(Form form)
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
            form.Region = null;
        }
        public event EventHandler Finished;
    }
}
