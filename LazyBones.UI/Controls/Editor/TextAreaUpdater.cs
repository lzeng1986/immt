using System;

namespace LazyBones.UI.Controls.Editor
{
    public class TextAreaUpdater
    {
        public TextAreaUpdateType UpdateType { get; private set; }
        public TextLocation Position { get; private set; }
        public TextAreaUpdater(TextAreaUpdateType type, int singleLine)
            : this(type, new TextLocation(0, singleLine))
        {
        }
        public TextAreaUpdater(TextAreaUpdateType type, int startLine, int endLine)
            : this(type, new TextLocation(startLine, endLine))
        {
        }
        public TextAreaUpdater(TextAreaUpdateType type)
            : this(type, TextLocation.Zero)
        {
        }
        public TextAreaUpdater(TextAreaUpdateType type, TextLocation location)
        {
            UpdateType = type;
            Position = location;
        }
        public override string ToString()
        {
            return String.Format("[TextAreaUpdate: Type={0}, Location={1}]", UpdateType, Position);
        }
    }
    public enum TextAreaUpdateType
    {
        WholeTextArea,
        SingleLine,
        SinglePosition,
        PositionToLineEnd,
        PositionToEnd,
        BetweenLines
    }
}
