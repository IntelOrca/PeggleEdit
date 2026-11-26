using System.ComponentModel;
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Children;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    /// <summary>
    /// What the property grid shows of an object.
    /// </summary>
    public class PropertyGridTests
    {
        static PropertyDescriptorCollection Browsable(object o) =>
            TypeDescriptor.GetProperties(o, new System.Attribute[] { BrowsableAttribute.Yes });

        [Fact]
        public void APegsProperties_AreShownWithTheObjects()
        {
            var level = new Level();
            var ball = new Ball(level) { X = 10, Y = 20 };
            ball.PegInfo = new PegInfo(ball, true, false);
            var properties = Browsable(ball);

            Assert.Null(properties.Find("PegInfo", false));
            Assert.Null(properties.Find("TakenAwayByFlippers", false));
            Assert.Equal("Peg Type", properties["PegType"].DisplayName);
            Assert.Equal("Variable", properties["Variable"].DisplayName);
            Assert.NotNull(properties["X"]);

            properties["Crumble"].SetValue(ball, true);
            Assert.True(ball.PegInfo.Crumble);
            properties["PegType"].SetValue(ball, PegType.Goal);
            Assert.Equal(PegType.Goal, ball.PegInfo.Type);
            Assert.Equal(PegType.Goal, properties["PegType"].GetValue(ball));
            Assert.IsType<EnumConverter>(properties["PegType"].Converter);
        }

        [Fact]
        public void AnObjectWithoutAPeg_HasNoPegProperties()
        {
            var level = new Level();
            var brick = new Brick(level) { PegInfo = null };
            var properties = Browsable(brick);
            Assert.Null(properties.Find("PegType", false));
            Assert.Null(properties.Find("Variable", false));
            Assert.Null(properties.Find("PegParent", false));
            Assert.Equal("Type", properties["BrickType"].DisplayName);
        }
    }
}
