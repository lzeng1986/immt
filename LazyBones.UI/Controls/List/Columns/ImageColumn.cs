using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace LazyBones.UI.Controls.List.Columns
{
    public class ImageColumn : Column
    {
        int imageIndex = -1;
        [Category("Appearance"), DefaultValue(-1)]
        public int ImageIndex
        {
            get { return imageIndex; }
            set { imageIndex = value; }
        }

        [Category("Appearance"), DefaultValue((ImageList)null)]
        public ImageList ImageList { get; set; }
    }
}
