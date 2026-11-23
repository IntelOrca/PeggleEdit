using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests of copying entries to the clipboard and back.
    /// </summary>
    public class ClipboardTests
    {
        static Mover NewMover(Level level, MoverType type) =>
            new Mover(level) { Type = type, AnchorPointX = 100, AnchorPointY = 100, Time = 300, Radius = 20 };

        /// <summary>
        /// A hole whose destination follows <paramref name="target"/>'s mover.
        /// </summary>
        static Hole HoleFollowing(Level level, LevelEntry target)
        {
            var hole = new Hole(level);
            hole.DestinationEntry.MoverLink = new MoverLink(level) { InternalLinkId = 99, InternalMover = target.MoverLink.Mover };
            return hole;
        }

        [Fact]
        public void HoleDestination_KeepsItsLinkToACopiedEntry()
        {
            var level = new Level();
            var ball = new Ball(level) { MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.Circle) } };
            var hole = HoleFollowing(level, ball);

            var pasted = LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(new LevelEntry[] { ball, hole }));
            var pastedBall = pasted.OfType<Ball>().Single();
            var pastedHole = pasted.OfType<Hole>().Single();
            Assert.False(pastedHole.DestinationEntry.MoverLink.OwnsMover);
            Assert.Same(pastedBall.MoverLink.Mover, pastedHole.DestinationEntry.MoverLink.Mover);
        }

        [Fact]
        public void HoleDestination_KeepsItsLinkToACopiedGenerator()
        {
            var level = new Level();
            var generator = new PegGenerator(level) { MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.Rotate) } };
            var hole = HoleFollowing(level, generator);

            var pasted = LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(new LevelEntry[] { generator, hole }));
            var pastedGenerator = pasted.OfType<PegGenerator>().Single();
            var pastedHole = pasted.OfType<Hole>().Single();
            Assert.False(pastedHole.DestinationEntry.MoverLink.OwnsMover);
            Assert.Same(pastedGenerator.MoverLink.Mover, pastedHole.DestinationEntry.MoverLink.Mover);
        }

        [Fact]
        public void HoleDestination_FollowingAnEntryNotCopied_GetsItsOwnCopy()
        {
            var level = new Level();
            var ball = new Ball(level) { MoverLink = new MoverLink(level) { Mover = NewMover(level, MoverType.Circle) } };
            var hole = HoleFollowing(level, ball);

            var pasted = LevelEntryClipboard.Deserialize(LevelEntryClipboard.Serialize(new LevelEntry[] { hole }));
            var pastedHole = pasted.OfType<Hole>().Single();
            Assert.True(pastedHole.DestinationEntry.MoverLink.OwnsMover);
            Assert.Equal(MoverType.Circle, pastedHole.DestinationEntry.MoverLink.Mover.Type);
        }
    }
}
