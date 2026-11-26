using System.Drawing;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Movers start where the game puts them. The expected positions are the ones PopCap's editor saved in Peggle
    /// Nights' own levels (the current position it writes with each mover).
    /// </summary>
    public class MoverTests
    {
        static Mover NewMover(MoverType type, float x, float y, short time, short radius) => new Mover(new Level())
        {
            Type = type,
            AnchorPointX = x,
            AnchorPointY = y,
            Time = time,
            Radius = radius,
        };

        static void AssertAt(float x, float y, PointF actual)
        {
            Assert.True(new PointF(x, y).GetLength(actual) < 0.01f, $"expected {x},{y}, was {actual}");
        }

        [Fact]
        public void HorizontalArc_SwingsThroughTheMaxAngle()
        {
            // Death Defying: pegs swinging below the anchor, eased over 100° rather than going round half a circle
            var mover = NewMover(MoverType.HorizontalArc, 328, 30, 1500, -280);
            mover.Phase = 0.5f;
            mover.MoveRotation = 9;
            mover.MaxAngle = 1.7453293f;
            AssertAt(419.1591f, 294.74518f, mover.GetStartPosition());
        }

        [Fact]
        public void HorizontalCycle_StartsOnTheRight()
        {
            AssertAt(556, 396, NewMover(MoverType.HorizontalCycle, 546, 396, 900, 10).GetStartPosition());
        }

        [Fact]
        public void VerticalCycle_GoesUpFirst()
        {
            // Hu2: a quarter of the way round (by the offset), turned by the path rotation
            var mover = NewMover(MoverType.VerticalCycle, 228, 323, 280, 40);
            mover.Offset = -50;
            mover.MoveRotation = 6;
            AssertAt(231.76708f, 358.84134f, mover.GetStartPosition());
        }

        [Fact]
        public void FlowerLoop_Moves()
        {
            AssertAt(494, 468, NewMover(MoverType.FlowerLoop, 487, 468, 2000, 7).GetStartPosition());
        }

        [Fact]
        public void Phase_IsAWholeTickAsTheGameWorksItOut()
        {
            // Pego's car: 0.7 is 0.69999999 as a float, which the game's single precision makes 700 ticks of 1000
            var mover = NewMover(MoverType.HorizontalWrap, -26, 476, 1000, 700);
            mover.Phase = 0.7f;
            AssertAt(464, 476, mover.GetStartPosition());

            // ... and 0.099999994 is 99 ticks, not 100
            mover.Phase = 0.099999994f;
            AssertAt(43.3f, 476, mover.GetStartPosition());

            // The phase and the clock are added before the fraction is dropped: 0.5 - 100 ticks is -99 ticks
            mover.Phase = 0.0005f;
            mover.Offset = -100;
            AssertAt(-26 - 69.3f, 476, mover.GetStartPosition());
        }

        [Fact]
        public void NoTime_LeavesTheObjectUnturned()
        {
            var mover = NewMover(MoverType.RotateAroundCircle, 300, 250, 0, 40);
            mover.Radius2 = 60;
            mover.Rotation = 30;
            Assert.Equal(40, mover.GetEstimatedMoveAngle(10), 3);
        }
    }
}
