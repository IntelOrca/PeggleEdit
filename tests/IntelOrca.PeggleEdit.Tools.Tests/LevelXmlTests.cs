using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using Xunit;

namespace IntelOrca.PeggleEdit.Tools.Tests
{
    public class LevelXmlTests
    {
        /// <summary>
        /// A level using every entry type and (almost) every optional field, in combinations the game produces.
        /// </summary>
        internal const string EverythingXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<PeggleLevel schema=""1"" fileVersion=""0x52"">
  <!-- entry 0: a peg using every generic property -->
  <Ball x=""10"" y=""20"" radius=""10"" extraFlagsA=""0x4"" extraFlagsB=""0x3"">
    <Properties rolly=""0.5"" bouncy=""0.9"" moveTime=""7"" solidColor=""#FF102030"" outlineColor=""#00FFFFFF""
                image=""images/levels/peg"" imageDx=""1.5"" imageDy=""-2.5"" imageRot=""0.785"" disappearTimer=""3""
                id=""peg1"" stopMovingTimer=""4"" sound=""2"" logic=""café &amp; &lt;script&gt;&#9;&#10;"" maxBounceVel=""5""
                subId=""9"" flipperFlags=""on"" collidable=""false"" visible=""false"" moving=""false"" background=""true""
                baseObj=""true"" ballStopReset=""true"" foreground=""true"" drawSort=""true"" foreground2=""true""
                drawFloat=""true"" drawShadow=""false"" extraFlags=""0xA0000000"" />
    <PegInfo type=""goal"" hit=""true"" hitTime=""1"" crumble=""true"" glowTimer=""2"" glowState=""1"" scored=""true"" crumbleTimer=""3"" />
  </Ball>
  <!-- entry 1: movement using every field, with a sub movement -->
  <Ball isStatic=""false"" ballCollide=""false"" radius=""12.25"">
    <Mover type=""circle"" reverse=""true"" anchorX=""400"" anchorY=""300"" time=""600"" offset=""5"" radius=""80""
           phase=""0.25"" moveRotation=""-0.43633232"" radius2=""30"" pause1=""10"" pause2=""20"" phase1=""1""
           phase2=""2"" postDelayPhase=""0.5"" maxAngle=""1.5"" currentAngle=""0.08726647"" rotation=""0.1"" currentX=""480"" currentY=""300""
           extraFlags=""0x8000"">
      <ParentMover lastX=""-10"" lastY=""15"">
        <Mover type=""verticalCycle"" anchorX=""100"" anchorY=""200"" time=""300"" radius=""25"" />
      </ParentMover>
    </Mover>
  </Ball>
  <!-- entry 2: shares the movement of entry 1 -->
  <Ball radius=""10"">
    <MoverRef link=""5"" />
  </Ball>
  <Line ax=""1"" ay=""2"" bx=""3"" by=""4"" velocityX=""5"" velocityY=""6"" extraFlags=""0x4"" />
  <Poly isBox=""true"" imageTile=""true"" mirror=""true"" rotation=""0.5"" angularVelocity=""1"" scale=""2"" normalDir=""keepIn"" x=""100"" y=""200""
        points=""-19.5,-19.5 -19.5,19.5 19.5,19.5"" growTimer=""1"" growType=""263"" flashTimer=""-3"" flashOnGrow=""false"" extraFlagsB=""0x10"">
    <Properties image=""images/levels/twirl"" collidable=""false"" />
  </Poly>
  <Poly x=""0"" y=""0"" points="""" />
  <Brick isBox=""true"" imageTile=""true"" mirror=""true"" polyRotation=""1"" angularVelocity=""2"" scale=""3"" normalDir=""4"" x=""305"" y=""346""
         growTimer=""5"" growType=""6"" flashTimer=""-7"" anchor=""both"" pivotOffsetX=""8"" pivotOffsetY=""9"" type=""straight"" subdivisions=""4""
         leftAngle=""0.1"" rightAngle=""0.2"" rightAngle2=""0.3"" angle=""45"" thickness=""19.999996"" texFlip=""true"" radius=""30"" rotation=""-5""
         outlineIndices=""1 2 0 3"" extraFlagsC=""0x3800"">
    <PegInfo type=""normal"" variable=""false"" glowState=""1"" />
  </Brick>
  <Brick x=""200"" y=""200"" anchor=""left"" type=""normalCurved"" subdivisions=""4"" rightAngle=""0.5"" angle=""90"" radius=""40"" rotation=""10"" outlineIndices=""0 0 0 0"" />
  <Hole circular=""true"" width=""50"" height=""50"" activeTimer=""-1"" score=""10000"" outDelay=""20"" x=""341"" y=""362"" velocityX=""1"" velocityY=""2"" extraFlags=""0x80"">
    <Properties solidColor=""#0000FF00"" visible=""false"" />
    <Destination>
      <Poly isBox=""true"" x=""504"" y=""141"" points=""-19.5,-19.5 -19.5,19.5 19.5,19.5"">
        <Properties collidable=""false"" visible=""false"" />
      </Poly>
    </Destination>
  </Hole>
  <Hole width=""40"" height=""30"" x=""1"" y=""2"">
    <Destination link=""7"" />
  </Hole>
  <Emitter dataVersion=""2"" circular=""true"" attached=""true"" additive=""true"" orientRotToVel=""true"" randomStart=""true"" triggerSleep=""true""
           image=""images/levels/particlepink"" width=""80"" height=""80"" fastForward=""100"" hardwareRate=""1"" triggerObj=""peg1"" rateIsLooping=""1""
           x=""345"" y=""362"" image2=""images/levels/other"" randRate=""0.1"" softwareRate=""0"" spiralCount=""0.3"" max=""15"" alphaFadeAt=""0.5""
           alphaInto=""0.1"" duration=""1"" rotationRand=""0.5"" scaleRand=""0.3"" velXRand=""1"" velYRand=""2"" accX=""3"" gravity=""4""
           polarVel=""1"" polarVelRand=""2"" polarAcc=""3"" velRot=""4"" velRotRand=""5"" velocityX=""1"" velocityY=""2"" extraFlags=""0x8000"">
    <PosX value=""1"" />
    <PosY curve=""b+0,1,0.01,1,~###"" />
    <Rate value=""0.005"" />
    <Distrib curve=""b+0,1,0.01,1,~###"" />
    <Rotation value=""0.3"" />
    <RotationVel value=""0"" />
    <Scale value=""0.5"" />
    <ScaleVel value=""0"" />
    <ColorR value=""1"" />
    <ColorG value=""0.5"" />
    <ColorB curve=""b+0,1"" />
    <Alpha value=""0.5"" />
    <VelX value=""-1"" />
    <VelY value=""-2"" />
  </Emitter>
  <Emitter dataVersion=""1"" hasPosCurves=""true"" image="""" width=""10"" height=""10"" x=""1"" y=""2"" image2=""dot"" randRate=""0"" softwareRate=""0""
           spiralCount=""0"" max=""1"" alphaFadeAt=""0"" alphaInto=""0"" duration=""0"">
    <Rate value=""1"" />
    <Distrib value=""1"" />
  </Emitter>
  <PegGenerator x=""400"" y=""300"" count=""8"" maxCount=""16"" radiusX=""100"" radiusY=""50"" angularOffset=""0.5"" />
  <BrickGenerator x=""400"" y=""300"" count=""8"" maxCount=""16"" innerRadius=""100"" angularOffset=""0.5"" brickWidth=""20"" />
  <PegCurveGenerator path=""1,2,0 3,4,1 5,6,2"" />
  <EntryRef link=""3"" />
</PeggleLevel>
";

        [Fact]
        public void Everything_RoundTripsThroughXml()
        {
            var data = LevelXml.Import(EverythingXml);
            var xml = LevelXml.Export(data);
            Assert.Equal(data, LevelXml.Import(xml));
            Assert.Equal(xml, LevelXml.Export(LevelXml.Import(xml)));
        }

        [Fact]
        public void Everything_RoundTripsThroughLevelModel()
        {
            // The level model and the XML converter must agree on the format. Generators in the entries are from
            // levels saved by older versions; the model saves them as the entries they generate (see GeneratorTests).
            var xml = Regex.Replace(EverythingXml, @"\s*<[A-Za-z]*Generator [^>]*/>", "");
            var data = LevelXml.Import(xml);
            Assert.Equal(data, ModelRoundTrip(data));
        }

        [Fact]
        public void OldVersion_Uses24BitFlags_AndRoundTrips()
        {
            const string xml = @"<PeggleLevel schema=""1"" fileVersion=""0x04"">
  <Ball x=""1"" y=""2"" radius=""10""><Properties rolly=""0.5"" foreground=""true"" /><PegInfo type=""normal"" /></Ball>
  <Brick x=""1"" y=""2"" type=""straight"" radius=""30"" rotation=""0"" outlineIndices=""1 0 0 0"" />
  <Poly x=""1"" y=""2"" points=""0,0 1,1"" />
</PeggleLevel>";
            var data = LevelXml.Import(xml);
            // header (9), magic and type (8), then 3 flag bytes: rolly, peg, collidable, visible, moving, foreground
            Assert.Equal(new byte[] { 0xE5, 0x00, 0x40 }, data.Skip(9 + 8).Take(3).ToArray());
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
            Assert.Equal(data, ModelRoundTrip(data));
        }

        [Fact]
        public void OldVersion_RejectsFieldsItCanNotStore()
        {
            AssertImportError(@"<PeggleLevel schema=""1"" fileVersion=""0x04""><Ball radius=""1""><Properties drawSort=""true"" /></Ball></PeggleLevel>", "above bit 23");
            AssertImportError(@"<PeggleLevel schema=""1"" fileVersion=""0x22""><Poly points="""" growType=""1"" /></PeggleLevel>", "not stored before");
            AssertImportError(@"<PeggleLevel schema=""1"" fileVersion=""0x4F""><Ball radius=""1""><Properties drawShadow=""false"" /></Ball></PeggleLevel>", "drawShadow");
        }

        [Fact]
        public void EntryRefs_AreKept()
        {
            const string xml = @"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball x=""1"" y=""2"" radius=""1"" /><EntryRef link=""3"" /></PeggleLevel>";
            var data = LevelXml.Import(xml);
            Assert.Equal(2, BitConverter.ToInt32(data, 5));
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
            Assert.Equal(data, ModelRoundTrip(data));
        }

        [Theory]
        [InlineData(0x00000000u, "0")]
        [InlineData(0x80000000u, "-0")]
        [InlineData(0x3F800000u, "1")]
        [InlineData(0x433E0000u, "190")]
        [InlineData(0x3DCCCCCDu, "0.1")]
        [InlineData(0x7FC00000u, "0x7FC00000")]
        [InlineData(0xFFD8F46Bu, "0xFFD8F46B")]
        [InlineData(0x7F800000u, "0x7F800000")]
        [InlineData(0xFF800000u, "0xFF800000")]
        public void Floats_FormatAsExpected(uint bits, string expected)
        {
            Assert.Equal(expected, LevelXml.FormatFloat(bits));
        }

        [Theory]
        [InlineData(0x00000001u)]  // smallest denormal
        [InlineData(0x007FFFFFu)]  // largest denormal
        [InlineData(0x7F7FFFFFu)]  // float.MaxValue
        [InlineData(0xFF7FFFFFu)]  // float.MinValue
        [InlineData(0x35800000u)]  // 9.5367432E-07
        [InlineData(0x3EAAAAABu)]  // 1/3
        public void Floats_RoundTripExactly(uint bits)
        {
            var text = LevelXml.FormatFloat(bits);
            Assert.DoesNotContain("E", text);
            Assert.True(LevelXml.TryParseFloatBits(text, out uint parsed));
            Assert.Equal(bits, parsed);
        }

        [Fact]
        public void Floats_RandomBitPatternsRoundTrip()
        {
            var random = new Random(1234);
            var buffer = new byte[4];
            for (int i = 0; i < 100000; i++)
            {
                random.NextBytes(buffer);
                uint bits = BitConverter.ToUInt32(buffer, 0);
                Assert.True(LevelXml.TryParseFloatBits(LevelXml.FormatFloat(bits), out uint parsed));
                Assert.Equal(bits, parsed);
            }
        }

        [Theory]
        [InlineData(new byte[] { 0x63, 0x61, 0x66, 0xC3, 0xA9 })]  // café
        [InlineData(new byte[] { 0x09, 0x0D, 0x0A, 0x20 })]        // whitespace that XML would normalise
        [InlineData(new byte[] { 0xEF, 0xBB, 0xBF, 0x61 })]        // BOM inside string
        public void Strings_RoundTrip(byte[] raw)
        {
            var data = LevelWithId(raw);
            Assert.Equal(data, LevelXml.Import(LevelXml.Export(data)));
        }

        [Theory]
        [InlineData(new byte[] { 0xC3, 0x28 })]        // invalid UTF-8
        [InlineData(new byte[] { 0x63, 0xE9 })]        // Latin-1
        [InlineData(new byte[] { 0x61, 0x01, 0x62 })]  // control character
        [InlineData(new byte[] { 0xEF, 0xBF, 0xBF })]  // U+FFFF
        public void Strings_ThatXmlCanNotHold_AreErrors(byte[] raw)
        {
            AssertExportError(LevelWithId(raw), "'id' on <Properties> has text that is not valid UTF-8");
        }

        /// <summary>
        /// A level with one ball whose id is the given bytes.
        /// </summary>
        internal static byte[] LevelWithId(byte[] raw, string fileVersion = "0x52")
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""" + fileVersion + @"""><Ball x=""1"" y=""2"" radius=""1""><Properties id=""x"" /></Ball></PeggleLevel>");
            int idOffset = Encoding.ASCII.GetString(data).IndexOf("\x01\x00x", StringComparison.Ordinal);
            return data.Take(idOffset).Concat(BitConverter.GetBytes((short)raw.Length)).Concat(raw).Concat(data.Skip(idOffset + 3)).ToArray();
        }

        [Fact]
        public void ExcessData_IsNotExported()
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball x=""1"" y=""2"" radius=""1"" /></PeggleLevel>");
            var xml = LevelXml.Export(data.Concat(new byte[] { 1, 2, 3 }).ToArray());
            Assert.Equal(LevelXml.Export(data), xml);
            Assert.Equal(data, LevelXml.Import(xml));
        }

        [Fact]
        public void InvalidFiles_AreErrors()
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1"" /></PeggleLevel>");
            AssertExportError(data.Take(data.Length - 1).ToArray(), "Entry 0");
            AssertExportError(new byte[] { 0x52, 0, 0 }, "too short");

            var headerByte = (byte[])data.Clone();
            headerByte[4] = 220;
            AssertExportError(headerByte, "header byte is 220");

            var emptyEntry = (byte[])data.Clone();
            Array.Clear(emptyEntry, 9, 4);
            AssertExportError(emptyEntry, "empty entry");
        }

        static void AssertExportError(byte[] data, string expectedMessage)
        {
            var ex = Assert.Throws<InvalidDataException>(() => LevelXml.Export(data));
            Assert.Contains(expectedMessage, ex.Message);
        }

        [Fact]
        public void UnknownEntryType_IsAnError()
        {
            var data = LevelXml.Import(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1"" /><Ball radius=""2"" /></PeggleLevel>");
            int second = Encoding.ASCII.GetString(data).LastIndexOf("\x01\x00\x00\x00\x05\x00\x00\x00", StringComparison.Ordinal);
            data[second + 4] = 77;
            AssertExportError(data, "Entry 1: unknown entry type 77");
        }

        [Fact]
        public void AnyBytes_RoundTripOrAreRejected()
        {
            // Truncations and random corruptions of a real level, and random junk, must either be rejected or survive.
            var level = LevelXml.Import(EverythingXml);
            var random = new Random(42);
            for (int length = 0; length <= level.Length; length++)
                AssertRoundTrips(level.Take(length).ToArray());
            for (int i = 0; i < 3000; i++)
            {
                var corrupt = (byte[])level.Clone();
                for (int j = random.Next(1, 4); j > 0; j--)
                    corrupt[random.Next(corrupt.Length)] = (byte)random.Next(256);
                AssertRoundTrips(corrupt);
            }
            for (int i = 0; i < 500; i++)
            {
                var junk = new byte[random.Next(0, 200)];
                random.NextBytes(junk);
                AssertRoundTrips(junk);
            }
        }

        [Theory]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1"" raduis=""2"" /></PeggleLevel>", "Unknown attribute 'raduis'")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball /></PeggleLevel>", "Missing attribute 'radius'")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""big"" /></PeggleLevel>", "not a valid number")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1"" x=""1"" /></PeggleLevel>", "must both be present")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1""><PegInfo type=""normal"" /><PegInfo type=""goal"" /></Ball></PeggleLevel>", "Only one <PegInfo>")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1""><Wobble /></Ball></PeggleLevel>", "Unexpected element <Wobble>")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1"" extraFlagsA=""0x13"" /></PeggleLevel>", "set by other attributes")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1""><PegInfo type=""256"" /></Ball></PeggleLevel>", "unknown value '256'")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1""><Properties visible=""yes"" /></Ball></PeggleLevel>", "'true' or 'false'")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Triangle /></PeggleLevel>", "Unknown entry type <Triangle>")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1""><Mover type=""rotate"" anchorX=""0"" anchorY=""0"" time=""1""><ParentMover lastX=""0"" lastY=""0""><Mover type=""rotate"" anchorX=""0"" anchorY=""0"" time=""1"" /><MoverRef link=""7"" /></ParentMover></Mover></Ball></PeggleLevel>", "both <Mover> and <MoverRef>")]
        [InlineData(@"<PeggleLevel schema=""2"" fileVersion=""0x52"" />", "Unsupported schema version 2")]
        [InlineData(@"<Level schema=""1"" />", "Expected root element")]
        [InlineData(@"<PeggleLevel schema=""1"" fileVersion=""0x52""><Ball radius=""1"">", "")]
        public void Import_RejectsInvalidXml(string xml, string expectedMessage)
        {
            AssertImportError(xml, expectedMessage);
        }

        [Fact]
        public void Import_ReportsLineNumbers()
        {
            var ex = Assert.Throws<LevelXmlException>(() => LevelXml.Import("<PeggleLevel schema=\"1\" fileVersion=\"0x52\">\n  <Ball radius=\"1\" />\n  <Ball radius=\"1\" bogus=\"1\" />\n</PeggleLevel>"));
            Assert.Equal(3, ex.LineNumber);
            Assert.Contains("line 3", ex.Message);
        }

        static void AssertImportError(string xml, string expectedMessage)
        {
            var ex = Assert.Throws<LevelXmlException>(() => LevelXml.Import(xml));
            Assert.Contains(expectedMessage, ex.Message);
        }

        static void AssertRoundTrips(byte[] data)
        {
            string xml;
            try
            {
                xml = LevelXml.Export(data);
            }
            catch (InvalidDataException)
            {
                return;
            }
            // Exactly the level, which is the whole file unless there is data after the last entry
            var back = LevelXml.Import(xml);
            if (back.Length > data.Length || !back.SequenceEqual(data.Take(back.Length)) || LevelXml.Export(back) != xml)
                Assert.Fail("Round trip failed for " + BitConverter.ToString(data));
        }

        internal static byte[] ModelRoundTrip(byte[] data)
        {
            Level level;
            using (var reader = new LevelReader(data))
            {
                level = reader.Read();
                Assert.True(level != null, reader.Error);
            }
            var ms = new MemoryStream();
            new LevelWriter(ms).Write(level, level.FileVersion);
            return ms.ToArray();
        }
    }
}
