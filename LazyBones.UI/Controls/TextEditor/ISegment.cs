
namespace LazyBones.UI.Controls.TextEditor
{
    public interface ISegment
    {
        int Offset { get; set; }
        int Length { get; set; }
    }
}
