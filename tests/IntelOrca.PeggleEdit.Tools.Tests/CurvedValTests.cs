using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// Tests that PopCap curve strings are evaluated as the game does.
    /// </summary>
    public class CurvedValTests
    {
        static readonly float[] Inputs = { 0, 0.05f, 0.25f, 0.4f, 0.5f, 0.73f, 1, 1.2f };

        [Theory]
        // Stock curves (all Hermite with flat slopes)
        [InlineData("b+0,1,0.01,1,####    x~###     )#P##", new[] { 0.0, 0.02873, 0.51013, 0.90617, 0.99949, 0.54927, 0.0, 0.0 })]
        [InlineData("b+0,1.5,0.01,1,####    {P###     &#P##", new[] { 0.0, 0.02127, 0.37878, 0.67583, 0.7499, 0.4159, 0.0, 0.0 })]
        [InlineData("b+0,-4,0.01,1,~###         ~3P##", new[] { -4.0, -3.97616, -3.48611, -2.84231, -2.35556, -1.30092, -0.71111, -0.71111 })]
        [InlineData("b+0,0.7,0.01,1,#### KB### 0h### K/### `J###q.###D'### 24###<*### ]#P##", new[] { 0.0, 0.06667, 0.52395, 0.09402, 0.22332, 0.05605, 0.0, 0.0 })]
        [InlineData("b+0,0,0.01,1,####  t:###   @####   k#P##", new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 })]
        [InlineData("b+1,4,0.01,1,/###         ~~P##", new[] { 1.4, 1.41885, 1.80625, 2.3152, 2.7, 3.53373, 4.0, 4.0 })]
        public void EvaluatesHermiteCurves(string curve, double[] expected)
        {
            var value = new CurvedVal(curve);
            Assert.True(value.IsVariable);
            for (int i = 0; i < Inputs.Length; i++)
                Assert.Equal(expected[i], value.Evaluate(Inputs[i]), 3);
        }

        [Theory]
        // Curves without the Hermite flag go through the game's B-spline table
        [InlineData("a0,1,0.01,#~~~#", new[] { 0.0, 0.72191, 1.0, 0.72158, 0.0, 0.0, 0.0 })]
        [InlineData("b#0,2,0.02,2,#~~~#~P", new[] { 0.0, 0.71997, 1.32042, 1.77711, 1.78999, 0.0, 0.0 })]
        public void EvaluatesTableCurves(string curve, double[] expected)
        {
            var inputs = new[] { 0, 0.05f, 0.1f, 0.15f, 0.25f, 0.4f, 1 };
            var value = new CurvedVal(curve);
            for (int i = 0; i < inputs.Length; i++)
                Assert.Equal(expected[i], value.Evaluate(inputs[i]), 3);
        }

        [Fact]
        public void ReadsCurvePoints()
        {
            var curve = CurveData.Parse("b+0,1,0.01,1,####    x~###     )#P##");
            Assert.True(curve.Hermite);
            Assert.False(curve.NoClip);
            Assert.Equal(0.01f, curve.IncRate);
            Assert.Equal(new[] { 0.0, 0.493, 1.0 }, curve.Points.Select(p => System.Math.Round(p.X, 3)).ToArray());
            Assert.Equal(new[] { 0.0, 1.0, 0.0 }, curve.Points.Select(p => System.Math.Round(p.Y, 3)).ToArray());
            Assert.All(curve.Points, p => Assert.Equal(0.0, p.Slope, 4));
        }

        [Fact]
        public void StaticValuesAreTheSameEverywhere()
        {
            var value = new CurvedVal(0.25f);
            Assert.Equal(0.25f, value.Evaluate(0));
            Assert.Equal(0.25f, value.Evaluate(0.7f));
            Assert.True(value.IsStatic(0.25f));
        }

        [Fact]
        public void BadCurvesEvaluateToZero()
        {
            Assert.Equal(0, new CurvedVal("not a curve").Evaluate(0.5f));
            Assert.Equal(0, new CurvedVal("b+").Evaluate(0.5f));
        }

        [Fact]
        public void OddCurvesDoNotBreak()
        {
            // A step below '#' goes backwards, and very many points would take long to solve
            Assert.InRange(new CurvedVal("a0,1,0,#!#").Evaluate(0.5f), 0, 1);
            var sw = System.Diagnostics.Stopwatch.StartNew();
            new CurvedVal("a0,1,0," + string.Concat(Enumerable.Repeat("~#", 300))).Evaluate(0.5f);
            Assert.True(sw.ElapsedMilliseconds < 2000);
        }

        [Fact]
        public void CurveFollowsEditedString()
        {
            var value = new CurvedVal("b+0,1,0.01,1,~###         ~~P##");
            Assert.Equal(1, value.Evaluate(0.5f), 3);
            value.VariableValue = "b+0,2,0.01,1,~###         ~~P##";
            Assert.Equal(2, value.Evaluate(0.5f), 3);
        }

        [Fact]
        public void SteppedRateLoops()
        {
            var stepper = new CurvedValStepper(new CurvedVal("b+0,1,0.25,1,####    x~###     )#P##"), looping: true);
            var values = Enumerable.Range(0, 7).Select(_ => System.Math.Round(stepper.Next(), 4)).ToArray();
            Assert.Equal(new[] { 0.0, 0.5101, 0.9995, 0.4901, 0.0, 0.5101, 0.9995 }, values);
        }

        [Fact]
        public void SteppedRateStopsAtTheEnd()
        {
            var stepper = new CurvedValStepper(new CurvedVal("b+0,1,0.25,1,####    x~###     )#P##"), looping: false);
            var values = Enumerable.Range(0, 7).Select(_ => System.Math.Round(stepper.Next(), 4)).ToArray();
            Assert.Equal(new[] { 0.0, 0.5101, 0.9995, 0.4901, 0.0, 0.0, 0.0 }, values);
        }
    }
}
