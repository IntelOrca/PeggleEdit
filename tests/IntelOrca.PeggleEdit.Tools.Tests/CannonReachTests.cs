using System;
using System.Drawing;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using IntelOrca.PeggleEdit.Tools.Pack;
using IntelOrca.PeggleEdit.Tools.Pack.Challenge;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    public class CannonReachTests
    {
        // Where the highest shot to the right is after t ticks, as the game's guide draws it
        static PointF HighestShot(double t)
        {
            double a = 7 * Math.PI / 180;
            double x = Cannon.Pivot.X + (92 * Math.Cos(a)) + (CannonReach.BallSpeed * Math.Cos(a) * t);
            double y = Cannon.Pivot.Y - (92 * Math.Sin(a)) - (CannonReach.BallSpeed * Math.Sin(a) * t) + (CannonReach.Gravity * t * t / 2);
            return new PointF((float)x, (float)y);
        }

        static Ball Peg(float x, float y, float radius = 10)
        {
            var peg = new Ball(new Level()) { X = x, Y = y, Radius = radius };
            peg.PegInfo = new PegInfo(peg);
            return peg;
        }

        [Fact]
        public void TheHighestShot_IsWhereTheBallCanBe()
        {
            foreach (var t in new[] { 10.0, 40.0, 55.0 })
            {
                var p = HighestShot(t);
                var highest = CannonReach.GetHighest((int)Math.Round(p.X));
                // Up to half a tick of the pull higher, and a column away
                Assert.InRange(highest, p.Y - 3, p.Y + 0.5);
            }
        }

        [Fact]
        public void BelowTheCannon_CanBeReached()
        {
            Assert.False(CannonReach.IsUnreachable(Peg(325, 300)));
            Assert.False(CannonReach.IsUnreachable(Peg(40, 500)));
            Assert.False(CannonReach.IsUnreachable(Peg(610, 500)));
        }

        [Fact]
        public void TheTopCorners_CanNotBeReached()
        {
            Assert.True(CannonReach.IsUnreachable(Peg(20, 25)));
            Assert.True(CannonReach.IsUnreachable(Peg(630, 25)));
        }

        [Fact]
        public void APegTheHighestShotPassesClose_CanBeReached()
        {
            var p = HighestShot(40);
            // The ball's edge is just inside the peg's
            Assert.False(CannonReach.IsUnreachable(Peg(p.X, p.Y - 10 - CannonReach.BallRadius + 1)));
            // and clear of it
            Assert.True(CannonReach.IsUnreachable(Peg(p.X, p.Y - 10 - CannonReach.BallRadius - 6)));
        }

        static Brick NewBrick(float x, float y, float length, float rotation, bool curved = false, float angle = 0)
        {
            var brick = new Brick(new Level()) { X = x, Y = y, Radius = length, Rotation = rotation, Curved = curved };
            if (curved)
                brick.Angle = angle;
            return brick;
        }

        [Fact]
        public void ALongBrick_IsReachableIfItsEndIs()
        {
            // Upright in the top left corner, its bottom end comes down to where the ball can go
            Assert.False(CannonReach.IsUnreachable(NewBrick(20, 40, 60, 90)));
            Assert.False(CannonReach.IsUnreachable(NewBrick(20, 40, 60, 0)));
            // Short and in the corner
            Assert.True(CannonReach.IsUnreachable(NewBrick(15, 15, 10, 0)));
        }

        [Fact]
        public void ACurvedBrick_IsReachableIfAnyOfItsArcIs()
        {
            var brick = NewBrick(20, 20, 60, 0, true, 90);
            // The arc reaches about 58 from its middle, to an outer corner
            Assert.InRange(CannonReach.GetEnclosingRadius(brick), 58, 62);
            Assert.False(CannonReach.IsUnreachable(brick));
            Assert.True(CannonReach.IsUnreachable(NewBrick(15, 12, 10, 0, true, 30)));
        }

        [Fact]
        public void LessPull_ReachesHigher()
        {
            var peg = Peg(40, 30);
            Assert.True(CannonReach.IsUnreachable(peg));
            // A challenge with half the pull, or a ball fired half as fast again
            Assert.False(CannonReach.IsUnreachable(peg, new[] { CannonReach.Shot.Normal, new CannonReach.Shot(CannonReach.BallSpeed, CannonReach.Gravity / 2) }));
            Assert.False(CannonReach.IsUnreachable(peg, new[] { new CannonReach.Shot(CannonReach.BallSpeed * 1.5f, CannonReach.Gravity) }));
            // More pull reaches less
            Assert.True(CannonReach.IsUnreachable(peg, new[] { new CannonReach.Shot(CannonReach.BallSpeed, CannonReach.Gravity * 2) }));
            // Pulled up, or fired backwards, the ball can go anywhere
            Assert.False(CannonReach.IsUnreachable(Peg(20, 25), new[] { CannonReach.Shot.Normal, new CannonReach.Shot(CannonReach.BallSpeed, -CannonReach.Gravity) }));
            Assert.False(CannonReach.IsUnreachable(Peg(20, 25), new[] { new CannonReach.Shot(-CannonReach.BallSpeed, CannonReach.Gravity) }));
        }

        [Fact]
        public void ACurvedBrickWithNoThickness_IsAsLargeAsItsArc()
        {
            // Turned inside out by a thickness below 0, its inner edge is the farthest from its middle
            var brick = NewBrick(20, 20, 60, 0, true, 90);
            brick.Thickness = -20;
            double inner = 60, middle = 50, half = Math.PI / 4;
            double far = Math.Sqrt((inner * inner) + (middle * middle) - (2 * inner * middle * Math.Cos(half)));
            Assert.True(CannonReach.GetEnclosingRadius(brick) >= far);
        }

        [Fact]
        public void GetShots_TakesTheChallengesThatPlayTheLevel()
        {
            var pack = new LevelPack();
            var level = new Level { Info = new LevelInfo("Level3", "Level 3", 0, 0) };
            var page = new ChallengePage();
            var plays = new Challenge { GravityMod = 0.5f, ProjectileSpeedMod = 1.25f };
            plays.Levels.Add(new Challenge.ChallengeLevel { Level = "level3" });
            var other = new Challenge { GravityMod = 0.25f };
            other.Levels.Add(new Challenge.ChallengeLevel { Level = "Level4" });
            var unchanged = new Challenge();
            unchanged.Levels.Add(new Challenge.ChallengeLevel { Level = "Level3" });
            page.Challenges.AddRange(new[] { plays, other, unchanged });
            pack.ChallengePages.Add(page);

            var shots = CannonReach.GetShots(pack, level);
            Assert.Equal(new[] { CannonReach.Shot.Normal, new CannonReach.Shot(CannonReach.BallSpeed * 1.25f, CannonReach.Gravity * 0.5f) }, shots);
        }

        [Fact]
        public void AboveTheCannon_CanNotBeReached()
        {
            Assert.True(CannonReach.IsUnreachable(Peg(325, 10, 5)));
        }

        [Fact]
        public void MovingPegsAndObjectsThatAreNotPegs_AreLeftAlone()
        {
            var level = new Level();
            var moving = Peg(20, 25);
            moving.MoverLink = new MoverLink(level);
            Assert.False(CannonReach.IsUnreachable(moving));

            var notAPeg = new Ball(level) { X = 20, Y = 25 };
            Assert.False(CannonReach.IsUnreachable(notAPeg));
        }
    }
}
