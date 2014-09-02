using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LazyBones.UI.Controls.TextEditor
{
    class HighlightingManager
    {
        Dictionary<string, TextDrawInfo> environmentColors = new Dictionary<string, TextDrawInfo>();

        public IDictionary<string, TextDrawInfo> EnvironmentColors
        {
            get { return environmentColors; }
        }
    }
}
