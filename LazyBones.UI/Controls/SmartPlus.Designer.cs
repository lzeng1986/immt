namespace LazyBones.UI.Controls
{
    partial class SmartPlus
    {
        /// <summary>
        /// Required LazyBones variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component LazyBones generated code

        /// <summary>
        /// Required method for LazyBones support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.animationTimer = new System.Windows.Forms.Timer(this.components);
            this.lockTimer = new System.Windows.Forms.Timer(this.components);
            // 
            // animationTimer
            // 
            this.animationTimer.Interval = 10;
            this.animationTimer.Tick += new System.EventHandler(this.animationTimer_Tick);
            // 
            // lockTimer
            // 
            this.lockTimer.Interval = 1000;
            this.lockTimer.Tick += new System.EventHandler(this.lockTimer_Tick);

        }

        #endregion

        private System.Windows.Forms.Timer animationTimer;
        private System.Windows.Forms.Timer lockTimer;
    }
}
