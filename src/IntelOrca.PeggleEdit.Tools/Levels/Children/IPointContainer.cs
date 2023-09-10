using System.Drawing;

namespace IntelOrca.PeggleEdit.Tools.Levels.Children
{
    /// <summary>
    /// An entry containing points that can be moved in the editor.
    /// </summary>
    public interface IPointContainer
    {
        int InteractionPointCount { get; }

        PointF GetInteractionPoint(int index);
        void SetInteractionPoint(int index, PointF value);
    }
}
