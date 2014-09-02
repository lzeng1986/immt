namespace LazyBones.UI.Controls
{
    /// <summary>
    /// 提供Form的全屏显示功能
    /// </summary>
    class FullScreen
    {
        System.Windows.Forms.FormBorderStyle borderStyle;
        System.Drawing.Rectangle bounds;
        bool topMost;
        readonly System.Windows.Forms.Form targetForm = null;
        public FullScreen(System.Windows.Forms.Form form)
        {
            if (form == null)
                throw new System.ArgumentNullException("form", "form值不能为空");
            this.targetForm = form;
            this.targetForm.KeyPreview = true;
        }
        void FillScreen()
        {
            var screen = System.Windows.Forms.Screen.FromControl(targetForm);
            bounds = targetForm.Bounds;
            borderStyle = targetForm.FormBorderStyle;
            topMost = targetForm.TopMost;
            targetForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            targetForm.TopMost = true;
            targetForm.Bounds = screen.Bounds;
        }
        void Release()
        {
            targetForm.Bounds = bounds;
            targetForm.FormBorderStyle = borderStyle;
            targetForm.TopMost = topMost;
        }
        bool isFullScreen = false;
        public bool IsFullScreen
        {
            get
            {
                return isFullScreen;
            }
            set
            {
                if (isFullScreen == value)
                    return;
                isFullScreen = value;
                if (isFullScreen)
                {
                    FillScreen();
                } 
                else
                {
                    Release();
                }
            }
        }
    }
}
