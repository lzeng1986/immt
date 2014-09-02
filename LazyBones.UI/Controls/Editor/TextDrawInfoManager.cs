using System.Collections.Generic;
using System.Drawing;

namespace LazyBones.UI.Controls.Editor
{
    //提供类似TextDrawInfo的工厂类的功能
    public class TextDrawInfoManager
    {
        Dictionary<string, TextDrawInfo> textColors = new Dictionary<string, TextDrawInfo>();
        public TextDrawInfo GetColor()
        {
            return null;
        }
        public TextDrawInfoManager()
            : this("default")
        { }
        public TextDrawInfoManager(string name)
        {
            Name = name;

            textColors["Default"] = new TextDrawInfo(SystemColors.WindowText, SystemColors.Window, false, false);
            textColors["Selection"] = new TextDrawInfo(SystemColors.HighlightText, SystemColors.Highlight, false, false);
            textColors["VRuler"] = new TextDrawInfo(SystemColors.ControlLight, SystemColors.Window, false, false);
            textColors["InvalidLines"] = new TextDrawInfo(Color.Red, false, false);
            textColors["CaretMarker"] = new TextDrawInfo(Color.Yellow, false, false);
            textColors["LineNumbers"] = new TextDrawInfo(SystemColors.ControlDark, SystemColors.Window, false, false);
            textColors["FoldLine"] = new TextDrawInfo(Color.FromArgb(0x80, 0x80, 0x80), Color.Black, false, false);
            textColors["FoldMarker"] = new TextDrawInfo(Color.FromArgb(0x80, 0x80, 0x80), Color.White, false, false);
            textColors["SelectedFoldLine"] = new TextDrawInfo(Color.Black, false, false);
            textColors["EOLMarkers"] = new TextDrawInfo(SystemColors.ControlLight, SystemColors.Window, false, false);
            textColors["SpaceMarkers"] = new TextDrawInfo(SystemColors.ControlLight, SystemColors.Window, false, false);
            textColors["TabMarkers"] = new TextDrawInfo(SystemColors.ControlLight, SystemColors.Window, false, false);
        }
        public TextDrawInfo this[string name]
        {
            get
            {
                TextDrawInfo color = null;
                if (textColors.TryGetValue(name, out color))
                    return color;
                return Default;
            }
            set
            {
                if (value == null)
                    return;
                textColors[name] = value;
            }
        }
        public string Name { get; private set; }
        public TextDrawInfo Digit { get; set; }

        public readonly static TextDrawInfo Default = new TextDrawInfo(SystemColors.WindowText);
    }
}
