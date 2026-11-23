// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2026. http://tedtycoon.co.uk
//
// PeggleEdit is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// PeggleEdit is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with PeggleEdit. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels.Xml
{
    /// <summary>
    /// Converts level files (.dat) to and from a lossless XML representation of the binary format.
    /// </summary>
    /// <remarks>
    /// The conversion works directly on the bytes of the level file, not on the <see cref="Level"/> model, so that
    /// <c>Import(Export(data))</c> gives back exactly <c>data</c> for any level:
    /// <list type="bullet">
    /// <item>Flag bits are not written as numbers. Each bit is implied by the presence of an optional field, is a
    /// named boolean attribute (omitted when it has its usual value), or is kept in an <c>extraFlags</c> attribute
    /// when its meaning is unknown.</item>
    /// <item>Floats are written as the shortest decimal that parses back to the same bits, or as their bits in hex
    /// (<c>0x…</c>) for NaN and infinity, as there are many different NaNs.</item>
    /// </list>
    /// Files that are not levels, or are damaged (an unknown entry type, a truncated file, text that is not valid
    /// UTF-8 or has characters XML can not hold), can not be exported. PeggleEdit's own data after the last entry
    /// (<see cref="EditorData"/>) is in a &lt;PeggleEdit&gt; element at the end; any other data after the last entry is
    /// not part of the level, so it is not in the XML.
    /// Import is strict: unknown attributes or elements, missing fields, out of range values and fields that the
    /// file version can not hold are errors, so that a typo never silently drops data.
    /// </remarks>
    public static class LevelXml
    {
        public const int SchemaVersion = 1;

        const string RootName = "PeggleLevel";
        const string EntryRefName = "EntryRef";
        const string PropertiesName = "Properties";
        const string ExtraFlags = "extraFlags";

        static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        #region Public API

        /// <summary>
        /// Converts the contents of a level file to XML.
        /// </summary>
        /// <exception cref="InvalidDataException">The data is not a level, or is damaged.</exception>
        public static string Export(byte[] data)
        {
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), ToXElement(data));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineHandling = NewLineHandling.Entitize,
            };
            using (var sw = new Utf8StringWriter())
            {
                using (var xw = XmlWriter.Create(sw, settings))
                {
                    doc.Save(xw);
                }
                return sw.ToString() + Environment.NewLine;
            }
        }

        /// <summary>
        /// Converts XML produced by <see cref="Export"/> (and possibly edited) back to the contents of a level file.
        /// </summary>
        /// <exception cref="LevelXmlException">The XML is not valid level XML.</exception>
        public static byte[] Import(string xml)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Parse(xml, LoadOptions.SetLineInfo);
            }
            catch (XmlException ex)
            {
                throw new LevelXmlException(ex.Message, ex.LineNumber, ex.LinePosition);
            }
            return FromXElement(doc.Root);
        }

        /// <summary>
        /// Converts the contents of a level file to an XML element.
        /// </summary>
        /// <exception cref="InvalidDataException">The data is not a level, or is damaged.</exception>
        public static XElement ToXElement(byte[] data)
        {
            if (data.Length < 9)
                throw new InvalidDataException("The file is too short to be a level.");

            var r = new BinReader(data);
            int version = r.I32();
            byte headerByte = r.U8();
            int count = r.I32();
            if (headerByte != 1)
                throw new InvalidDataException($"The file is not a level: its header byte is {headerByte}, not 1.");
            if (count < 0)
                throw new InvalidDataException($"The file is not a level: it has {count} entries.");

            var root = new XElement(RootName, new XAttribute("schema", SchemaVersion));
            root.Add(new XAttribute("fileVersion", "0x" + version.ToString("X2", Inv)));
            for (int i = 0; i < count; i++)
            {
                try
                {
                    root.Add(ReadEntry(r, version));
                }
                catch (Exception ex) when (ex is LevelFormatException || ex is OverflowException)
                {
                    throw new InvalidDataException($"Entry {i}: {ex.Message}.", ex);
                }
            }
            // PeggleEdit's own data after the last entry is kept; any other data there is not part of the level
            int realEntries = root.Elements().Count(x => x.Name != EntryRefName);
            var editorData = ReadEditorData(data, r.Position, version, realEntries);
            if (editorData != null)
                root.Add(editorData);
            return root;
        }

        const string EditorDataName = "PeggleEdit";
        // The entries a generator generated, which are in the level in its place
        const string FirstEntryName = "firstEntry";
        const string EntryCountName = "entryCount";
        // A hash of those entries, so that PeggleEdit notices if they were changed
        const string EntryHashName = "entryHash";

        const string LinkName = "Link";

        /// <summary>
        /// Reads PeggleEdit's data after the last entry, if it is there, belongs to this level, and can be read.
        /// </summary>
        /// <param name="entryCount">The number of entries, not counting references to earlier objects.</param>
        static XElement ReadEditorData(byte[] data, int levelEnd, int version, int entryCount)
        {
            if (EditorData.Find(data, levelEnd, out var chunks) != EditorData.Status.Valid)
                return null;
            int generatorsChunk = chunks.FindIndex(x => x.Tag == EditorData.GeneratorsTag);
            if (generatorsChunk < 0)
                return null;

            var result = new XElement(EditorDataName);
            try
            {
                var chunk = chunks[generatorsChunk];
                var r = new BinReader(data.Skip(chunk.Offset).Take(chunk.Length).ToArray());
                int count = r.I32();
                if (count < 0)
                    return null;
                int next = 0;
                for (int i = 0; i < count; i++)
                {
                    int first = r.I32();
                    int generated = r.I32();
                    ulong hash = r.U32() | ((ulong)r.U32() << 32);
                    if (first < next || generated < 0 || generated > entryCount - first)
                        return null;
                    next = first + generated;

                    var el = ReadEntry(r, version);
                    if (!GeneratorNames.Contains(el.Name.LocalName))
                        return null;
                    var attributes = el.Attributes().ToList();
                    el.RemoveAttributes();
                    el.Add(new XAttribute(FirstEntryName, first), new XAttribute(EntryCountName, generated),
                        new XAttribute(EntryHashName, "0x" + hash.ToString("X16", Inv)), attributes);
                    result.Add(el);
                }
                if (r.Remaining != 0)
                    return null;

                int linksChunk = chunks.FindIndex(x => x.Tag == EditorData.LinksTag);
                if (linksChunk >= 0)
                {
                    foreach (var link in EditorData.ReadLinks(data, chunks[linksChunk]))
                    {
                        var el = new XElement(LinkName);
                        if (link.HolderGenerator < 0)
                            el.Add(new XAttribute("holderMover", link.HolderMoverId));
                        else
                            el.Add(new XAttribute("holderGenerator", link.HolderGenerator), new XAttribute("holderDepth", link.HolderDepth));
                        el.Add(new XAttribute("generator", link.Generator), new XAttribute("depth", link.Depth));
                        result.Add(el);
                    }
                }
            }
            catch (Exception ex) when (ex is LevelFormatException || ex is OverflowException || ex is InvalidDataException || ex is EndOfStreamException)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// Writes PeggleEdit's data, from a &lt;PeggleEdit&gt; element, for the level written so far.
        /// </summary>
        /// <param name="entryCount">The number of entries, not counting references to earlier objects.</param>
        static byte[] WriteEditorData(Node n, byte[] levelPart, int version, int entryCount)
        {
            var w = new BinWriter();
            var children = n.RemainingChildren();
            var generators = children.Where(x => x.Name != LinkName).ToList();
            var links = new List<EditorData.LinkRecord>();
            w.I32(generators.Count);
            int next = 0;
            foreach (var child in children)
            {
                n.UseChild(child.Element);
                if (child.Name == LinkName)
                {
                    links.Add(ReadLinkElement(child, generators.Count));
                    continue;
                }
                if (links.Count != 0)
                    throw child.Error($"<{LinkName}> elements must come after the generators.");
                if (!GeneratorNames.Contains(child.Name))
                    throw child.Error($"<{EditorDataName}> can only hold generators and links, not <{child.Name}>.");
                int first = child.Int32(FirstEntryName);
                int count = child.Int32(EntryCountName);
                if (first < next || first > entryCount)
                    throw child.Error($"'{FirstEntryName}' must be from {next} to {entryCount}: generators are in entry order and do not overlap.", FirstEntryName);
                if (count < 0 || count > entryCount - first)
                    throw child.Error($"'{EntryCountName}' must be from 0 to {entryCount - first}.", EntryCountName);
                next = first + count;
                var hashText = child.String(EntryHashName);
                if (!hashText.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ||
                    !ulong.TryParse(hashText.Substring(2), NumberStyles.AllowHexSpecifier, Inv, out var hash))
                    throw child.Error($"'{EntryHashName}' must be a hex number, e.g. 0x0123456789ABCDEF.", EntryHashName);
                w.I32(first);
                w.I32(count);
                w.U32((uint)hash);
                w.U32((uint)(hash >> 32));
                WriteEntry(child, w, version);
            }
            n.Done();
            var chunks = new List<KeyValuePair<string, byte[]>> { new KeyValuePair<string, byte[]>(EditorData.GeneratorsTag, w.ToArray()) };
            if (links.Count != 0)
                chunks.Add(new KeyValuePair<string, byte[]>(EditorData.LinksTag, EditorData.WriteLinks(links)));
            return EditorData.Build(levelPart, chunks);
        }

        static EditorData.LinkRecord ReadLinkElement(Node n, int generatorCount)
        {
            var link = new EditorData.LinkRecord { HolderGenerator = -1 };
            if (n.Has("holderMover"))
            {
                link.HolderMoverId = n.Int32("holderMover");
                if (n.Has("holderGenerator") || n.Has("holderDepth"))
                    throw n.Error("A link is held by either a mover ('holderMover') or a generator ('holderGenerator'), not both.");
            }
            else
            {
                link.HolderGenerator = n.Int32("holderGenerator");
                link.HolderDepth = n.Int32("holderDepth");
                if (link.HolderGenerator < 0 || link.HolderGenerator >= generatorCount)
                    throw n.Error($"'holderGenerator' must be from 0 to {generatorCount - 1}.", "holderGenerator");
            }
            link.Generator = n.Int32("generator");
            link.Depth = n.Int32("depth");
            if (link.Generator < 0 || link.Generator >= generatorCount)
                throw n.Error($"'generator' must be from 0 to {generatorCount - 1}.", "generator");
            n.Done();
            return link;
        }

        /// <summary>
        /// Converts an XML element produced by <see cref="ToXElement"/> back to the contents of a level file.
        /// </summary>
        /// <exception cref="LevelXmlException">The XML is not valid level XML.</exception>
        public static byte[] FromXElement(XElement rootElement)
        {
            var root = new Node(rootElement);
            if (rootElement.Name != RootName)
                throw root.Error($"Expected root element <{RootName}>, found <{rootElement.Name}>.");
            int schema = root.Int32("schema");
            if (schema != SchemaVersion)
                throw root.Error($"Unsupported schema version {schema}, expected {SchemaVersion}.", "schema");

            int version = root.Int32("fileVersion");
            var children = rootElement.Elements().ToList();
            XElement editorData = null;
            if (children.Count != 0 && children[children.Count - 1].Name == EditorDataName)
            {
                editorData = children[children.Count - 1];
                children.RemoveAt(children.Count - 1);
            }
            var w = new BinWriter();
            w.I32(version);
            w.U8(1);
            w.I32(children.Count);
            foreach (var child in children)
            {
                root.UseChild(child);
                if (child.Name == EditorDataName)
                    throw new Node(child).Error($"<{EditorDataName}> must be the last element.");
                WriteEntry(new Node(child), w, version);
            }
            var data = w.ToArray();
            if (editorData != null)
            {
                root.UseChild(editorData);
                var levelPart = data;
                var trailer = WriteEditorData(new Node(editorData), levelPart, version, children.Count(x => x.Name != EntryRefName));
                data = levelPart.Concat(trailer).ToArray();
            }
            root.Done();
            return data;
        }

        #endregion

        #region Entries

        static readonly Dictionary<int, string> EntryNames = new Dictionary<int, string>
        {
            { LevelEntryTypes.Line, "Line" },
            { LevelEntryTypes.Poly, "Poly" },
            { LevelEntryTypes.Ball, "Ball" },
            { LevelEntryTypes.Brick, "Brick" },
            { LevelEntryTypes.Hole, "Hole" },
            { LevelEntryTypes.Emitter, "Emitter" },
            { LevelEntryTypes.PegGenerator, "PegGenerator" },
            { LevelEntryTypes.BrickGenerator, "BrickGenerator" },
            { LevelEntryTypes.PegCurveGenerator, "PegCurveGenerator" },
            { LevelEntryTypes.BrickCurveGenerator, "BrickCurveGenerator" },
        };
        static readonly Dictionary<string, int> EntryTypes = EntryNames.ToDictionary(kv => kv.Value, kv => kv.Key);
        static readonly HashSet<string> GeneratorNames = new HashSet<string>
        {
            "PegGenerator", "BrickGenerator", "PegCurveGenerator", "BrickCurveGenerator",
        };

        enum Kind { F32, I32, U8, Str, Argb, FlipperFlags }

        struct ValueField
        {
            public int Bit;
            public string Name;
            public Kind Kind;
            public ValueField(int bit, string name, Kind kind) { Bit = bit; Name = name; Kind = kind; }
        }

        struct BoolField
        {
            public int Bit;
            public string Name;
            public bool Default;
            public BoolField(int bit, string name, bool def) { Bit = bit; Name = name; Default = def; }
        }

        // Generic entry data, in the order it is stored after the flags
        static readonly ValueField[] GenericValues =
        {
            new ValueField(0, "rolly", Kind.F32),
            new ValueField(1, "bouncy", Kind.F32),
            new ValueField(4, "moveTime", Kind.I32),
            new ValueField(8, "solidColor", Kind.Argb),
            new ValueField(9, "outlineColor", Kind.Argb),
            new ValueField(10, "image", Kind.Str),
            new ValueField(11, "imageDx", Kind.F32),
            new ValueField(12, "imageDy", Kind.F32),
            new ValueField(13, "imageRot", Kind.F32),
            new ValueField(16, "disappearTimer", Kind.I32),
            new ValueField(17, "id", Kind.Str),
            new ValueField(18, "stopMovingTimer", Kind.I32),
            new ValueField(19, "sound", Kind.U8),
            new ValueField(21, "logic", Kind.Str),
            new ValueField(23, "maxBounceVel", Kind.F32),
            new ValueField(26, "subId", Kind.I32),
            new ValueField(27, "flipperFlags", Kind.FlipperFlags),
        };

        // physobj_flipperflags choices in LevelEditor_Choices.cfg
        static readonly string[] FlipperFlagValues = { "default", "off", "on" };

        static readonly BoolField[] GenericBools =
        {
            new BoolField(5, "collidable", true),
            new BoolField(6, "visible", true),
            new BoolField(7, "moving", true),
            new BoolField(14, "background", false),
            new BoolField(15, "baseObj", false),
            new BoolField(20, "ballStopReset", false),
            new BoolField(22, "foreground", false),
            new BoolField(24, "drawSort", false),
            new BoolField(25, "foreground2", false),
            new BoolField(28, "drawFloat", false),
        };

        const int GenericPegBit = 2;
        const int GenericMovementBit = 3;
        const int GenericShadowBit = 30;
        const int GenericShadowVersion = 0x50;
        const int GenericFlags32Version = 0x0F;

        // Hole destinations and parent movers nest; limit how deep, so that a crafted file can not overflow the stack
        [ThreadStatic]
        static int sNesting;

        static T Nested<T>(Func<T> read, Func<Exception> tooDeep)
        {
            if (sNesting >= NestingLimit.MaxDepth)
                throw tooDeep();
            sNesting++;
            try
            {
                return read();
            }
            finally
            {
                sNesting--;
            }
        }

        static string TooDeepMessage => $"objects are nested more than {NestingLimit.MaxDepth} deep";

        static XElement ReadEntry(BinReader r, int version) =>
            Nested(() => ReadEntryCore(r, version), () => new LevelFormatException(TooDeepMessage));

        static XElement ReadEntryCore(BinReader r, int version)
        {
            // Entries are links: 1 = the entry follows inline, anything else refers to an earlier object (0, no
            // object, only appears in files that are not levels)
            int link = r.I32();
            if (link == 0)
                throw new LevelFormatException("empty entry");
            if (link != 1)
                return new XElement(EntryRefName, new XAttribute("link", link));

            int type = r.I32();
            if (!EntryNames.TryGetValue(type, out var name))
                throw new LevelFormatException($"unknown entry type {type}");
            var el = new XElement(name);

            // Generic data
            uint f = version < GenericFlags32Version ? (uint)(r.U16() | (r.U8() << 16)) : r.U32();
            var props = new XElement(PropertiesName);
            uint used = Bit(GenericPegBit) | Bit(GenericMovementBit);
            foreach (var field in GenericValues)
            {
                used |= Bit(field.Bit);
                if (IsSet(f, field.Bit))
                    ReadValue(r, props, field.Name, field.Kind);
            }
            foreach (var field in GenericBools)
            {
                used |= Bit(field.Bit);
                bool value = IsSet(f, field.Bit);
                if (value != field.Default)
                    props.Add(new XAttribute(field.Name, value ? "true" : "false"));
            }
            if (version >= GenericShadowVersion)
            {
                used |= Bit(GenericShadowBit);
                if (!IsSet(f, GenericShadowBit))
                    props.Add(new XAttribute("drawShadow", "false"));
            }
            AddExtraFlags(props, ExtraFlags, f, used);
            if (props.HasAttributes)
                el.Add(props);

            if (IsSet(f, GenericPegBit))
                el.Add(ReadPeg(r));
            if (IsSet(f, GenericMovementBit))
                ReadMovementLink(r, version, el);

            switch (type)
            {
                case LevelEntryTypes.Line: ReadLine(r, el); break;
                case LevelEntryTypes.Poly: ReadPoly(r, version, el); break;
                case LevelEntryTypes.Ball: ReadBall(r, version, el); break;
                case LevelEntryTypes.Brick: ReadBrick(r, version, el); break;
                case LevelEntryTypes.Hole: ReadHole(r, version, el); break;
                case LevelEntryTypes.Emitter: ReadEmitter(r, el); break;
                case LevelEntryTypes.PegGenerator: ReadPegGenerator(r, el); break;
                case LevelEntryTypes.BrickGenerator: ReadBrickGenerator(r, el); break;
                case LevelEntryTypes.PegCurveGenerator:
                case LevelEntryTypes.BrickCurveGenerator: ReadCurveGenerator(r, el); break;
            }
            return el;
        }

        static void WriteEntry(Node n, BinWriter w, int version) =>
            Nested(() => { WriteEntryCore(n, w, version); return 0; }, () => n.Error("Objects are " + TooDeepMessage + "."));

        static void WriteEntryCore(Node n, BinWriter w, int version)
        {
            if (n.Name == EntryRefName)
            {
                int link = n.Int32("link");
                if (link == 0 || link == 1)
                    throw n.Error("Link 0 is not an object, and link 1 means the entry follows inline.", "link");
                w.I32(link);
                n.Done();
                return;
            }
            if (!EntryTypes.TryGetValue(n.Name, out int type))
                throw n.Error($"Unknown entry type <{n.Name}>.");

            w.I32(1);
            w.I32(type);

            // Generic data
            var props = n.OptionalChild(PropertiesName) ?? Node.Empty(n);
            uint named = Bit(GenericPegBit) | Bit(GenericMovementBit);
            uint f = 0;
            foreach (var field in GenericValues)
            {
                named |= Bit(field.Bit);
                if (props.Has(field.Name))
                    f |= Bit(field.Bit);
            }
            foreach (var field in GenericBools)
            {
                named |= Bit(field.Bit);
                if (props.Bool(field.Name, field.Default))
                    f |= Bit(field.Bit);
            }
            if (version >= GenericShadowVersion)
            {
                named |= Bit(GenericShadowBit);
                if (props.Bool("drawShadow", true))
                    f |= Bit(GenericShadowBit);
            }
            else if (props.Has("drawShadow"))
            {
                throw props.Error($"'drawShadow' is not stored before file version 0x{GenericShadowVersion:X2}; use extraFlags.", "drawShadow");
            }
            f |= props.ExtraFlags(ExtraFlags, named);

            var peg = n.OptionalChild("PegInfo");
            var movement = n.OptionalChild("Mover");
            var movementRef = n.OptionalChild("MoverRef");
            if (movement != null && movementRef != null)
                throw movementRef.Error("An entry can not have both <Mover> and <MoverRef>.");
            if (peg != null)
                f |= Bit(GenericPegBit);
            if (movement != null || movementRef != null)
                f |= Bit(GenericMovementBit);

            if (version < GenericFlags32Version)
            {
                if ((f & 0xFF000000) != 0)
                    throw props.Error($"Flags above bit 23 can not be stored before file version 0x{GenericFlags32Version:X2}.");
                w.U16((ushort)f);
                w.U8((byte)(f >> 16));
            }
            else
            {
                w.U32(f);
            }

            foreach (var field in GenericValues)
            {
                if (IsSet(f, field.Bit))
                    WriteValue(props, w, field.Name, field.Kind);
            }
            props.Done();

            if (peg != null)
                WritePeg(peg, w);
            if (movement != null || movementRef != null)
                WriteMovementLink(n, movement, movementRef, w, version);

            switch (type)
            {
                case LevelEntryTypes.Line: WriteLine(n, w); break;
                case LevelEntryTypes.Poly: WritePoly(n, w, version); break;
                case LevelEntryTypes.Ball: WriteBall(n, w, version); break;
                case LevelEntryTypes.Brick: WriteBrick(n, w, version); break;
                case LevelEntryTypes.Hole: WriteHole(n, w, version); break;
                case LevelEntryTypes.Emitter: WriteEmitter(n, w); break;
                case LevelEntryTypes.PegGenerator: WritePegGenerator(n, w); break;
                case LevelEntryTypes.BrickGenerator: WriteBrickGenerator(n, w); break;
                case LevelEntryTypes.PegCurveGenerator:
                case LevelEntryTypes.BrickCurveGenerator: WriteCurveGenerator(n, w); break;
            }
            n.Done();
        }

        #endregion

        #region Peg

        // peg_type choices in LevelEditor_Choices.cfg: goal = orange, score = purple, powerup = green
        static readonly string[] PegTypes = { "none", "normal", "goal", "score", "powerup" };

        // Everything except type, variable and crumble is game state saved with the level, not a level setting.
        static XElement ReadPeg(BinReader r)
        {
            byte type = r.U8();
            var el = new XElement("PegInfo", new XAttribute("type", type < PegTypes.Length ? PegTypes[type] : type.ToString(Inv)));
            byte f = r.U8();
            if (IsSet(f, 0)) el.Add(new XAttribute("hit", "true"));
            if (!IsSet(f, 1)) el.Add(new XAttribute("variable", "false"));
            if (IsSet(f, 2)) el.Add(new XAttribute("hitTime", r.I32()));
            if (IsSet(f, 3)) el.Add(new XAttribute("crumble", "true"));
            if (IsSet(f, 4)) el.Add(new XAttribute("glowTimer", r.I32()));
            if (IsSet(f, 5)) el.Add(new XAttribute("glowState", r.U8()));
            if (IsSet(f, 6)) el.Add(new XAttribute("scored", "true"));
            if (IsSet(f, 7)) el.Add(new XAttribute("crumbleTimer", r.U8()));
            return el;
        }

        static void WritePeg(Node n, BinWriter w)
        {
            uint f = 0;
            if (n.Bool("hit", false)) f |= Bit(0);
            if (n.Bool("variable", true)) f |= Bit(1);
            if (n.Has("hitTime")) f |= Bit(2);
            if (n.Bool("crumble", false)) f |= Bit(3);
            if (n.Has("glowTimer")) f |= Bit(4);
            if (n.Has("glowState")) f |= Bit(5);
            if (n.Bool("scored", false)) f |= Bit(6);
            if (n.Has("crumbleTimer")) f |= Bit(7);
            w.U8(n.Choice("type", PegTypes));
            w.U8((byte)f);
            if (n.Has("hitTime")) w.I32(n.Int32("hitTime"));
            if (n.Has("glowTimer")) w.I32(n.Int32("glowTimer"));
            if (n.Has("glowState")) w.U8(n.UInt8("glowState"));
            if (n.Has("crumbleTimer")) w.U8(n.UInt8("crumbleTimer"));
            n.Done();
        }

        #endregion

        #region Movement

        // mover_type choices in LevelEditor_Choices.cfg. 10 is not implemented by the game (the object stays still);
        // 14 and 15 are implemented but are not in the editor, so are named after what they do.
        static readonly string[] MovementShapes =
        {
            "noMovement", "verticalCycle", "horizontalCycle", "circle", "horizontalInfinity", "verticalInfinity",
            "horizontalArc", "verticalArc", "rotate", "rotateBackAndForth", "unused", "verticalWrap",
            "horizontalWrap", "rotateAroundCircle", "swingAroundCircle", "flowerLoop",
        };

        enum MKind { I16, U8, F32 }

        struct MovementField
        {
            public int Bit;
            public string Name;
            public MKind Kind;
            public MovementField(int bit, string name, MKind kind) { Bit = bit; Name = name; Kind = kind; }
        }

        // Optional movement values, in the order they are stored
        static readonly MovementField[] MovementFields =
        {
            new MovementField(0, "offset", MKind.I16),
            new MovementField(1, "radius", MKind.I16),
            new MovementField(2, "phase", MKind.F32),
            new MovementField(3, "moveRotation", MKind.F32),
            new MovementField(4, "radius2", MKind.I16),
            new MovementField(5, "pause1", MKind.I16),
            new MovementField(6, "pause2", MKind.I16),
            new MovementField(7, "phase1", MKind.U8),
            new MovementField(8, "phase2", MKind.U8),
            new MovementField(9, "postDelayPhase", MKind.F32),
            new MovementField(10, "maxAngle", MKind.F32),
            new MovementField(11, "currentAngle", MKind.F32),
            new MovementField(14, "rotation", MKind.F32),
        };

        const int MovementParentBit = 12;
        const int MovementObjectBit = 13;

        static void ReadMovementLink(BinReader r, int version, XElement parent)
        {
            int link = r.I32();
            if (link == 1)
                parent.Add(ReadMovement(r, version));
            else
                parent.Add(new XElement("MoverRef", new XAttribute("link", link)));
        }

        static void WriteMovementLink(Node parent, Node movement, Node movementRef, BinWriter w, int version)
        {
            if (movement != null)
            {
                w.I32(1);
                WriteMovement(movement, w, version);
            }
            else if (movementRef != null)
            {
                int link = movementRef.Int32("link");
                if (link == 1)
                    throw movementRef.Error("Link 1 means the mover follows inline; use a <Mover> element instead.", "link");
                w.I32(link);
                movementRef.Done();
            }
            else
            {
                throw parent.Error("Expected a <Mover> or <MoverRef> element.");
            }
        }

        static XElement ReadMovement(BinReader r, int version) =>
            Nested(() => ReadMovementCore(r, version), () => new LevelFormatException(TooDeepMessage));

        static XElement ReadMovementCore(BinReader r, int version)
        {
            var el = new XElement("Mover");
            int shape = r.I8();
            int t = Math.Abs(shape);
            el.Add(new XAttribute("type", t < MovementShapes.Length ? MovementShapes[t] : t.ToString(Inv)));
            if (shape < 0)
                el.Add(new XAttribute("reverse", "true"));
            el.Add(new XAttribute("anchorX", r.F32()), new XAttribute("anchorY", r.F32()));
            el.Add(new XAttribute("time", r.I16()));
            ushort f = r.U16();
            uint used = Bit(MovementParentBit) | Bit(MovementObjectBit);
            foreach (var field in MovementFields)
            {
                used |= Bit(field.Bit);
                if (IsSet(f, field.Bit))
                {
                    switch (field.Kind)
                    {
                        case MKind.I16: el.Add(new XAttribute(field.Name, r.I16())); break;
                        case MKind.U8: el.Add(new XAttribute(field.Name, r.U8())); break;
                        default: el.Add(new XAttribute(field.Name, r.F32())); break;
                    }
                }
            }
            if (IsSet(f, MovementParentBit))
            {
                var sub = new XElement("ParentMover", new XAttribute("lastX", r.F32()), new XAttribute("lastY", r.F32()));
                ReadMovementLink(r, version, sub);
                el.Add(sub);
            }
            if (IsSet(f, MovementObjectBit))
                el.Add(new XAttribute("currentX", r.F32()), new XAttribute("currentY", r.F32()));
            AddExtraFlags(el, ExtraFlags, f, used);
            return el;
        }

        static void WriteMovement(Node n, BinWriter w, int version) =>
            Nested(() => { WriteMovementCore(n, w, version); return 0; }, () => n.Error("Objects are " + TooDeepMessage + "."));

        static void WriteMovementCore(Node n, BinWriter w, int version)
        {
            int shape = n.Choice("type", MovementShapes, 128);
            bool reverse = n.Bool("reverse", false);
            if (reverse && shape == 0)
                throw n.Error("Mover type 0 can not be reversed.", "reverse");
            int stored = reverse ? -shape : shape;
            if (stored < sbyte.MinValue || stored > sbyte.MaxValue)
                throw n.Error($"Mover type {shape} can only be stored reversed.", "type");
            w.I8((sbyte)stored);
            w.F32(n.Float("anchorX"));
            w.F32(n.Float("anchorY"));
            w.I16(n.Int16("time"));

            var sub = n.OptionalChild("ParentMover");
            uint named = Bit(MovementParentBit) | Bit(MovementObjectBit);
            uint f = 0;
            foreach (var field in MovementFields)
            {
                named |= Bit(field.Bit);
                if (n.Has(field.Name))
                    f |= Bit(field.Bit);
            }
            if (sub != null) f |= Bit(MovementParentBit);
            if (n.HasPair("currentX", "currentY")) f |= Bit(MovementObjectBit);
            f |= n.ExtraFlags(ExtraFlags, named, 16);
            w.U16((ushort)f);

            foreach (var field in MovementFields)
            {
                if (!IsSet(f, field.Bit))
                    continue;
                switch (field.Kind)
                {
                    case MKind.I16: w.I16(n.Int16(field.Name)); break;
                    case MKind.U8: w.U8(n.UInt8(field.Name)); break;
                    default: w.F32(n.Float(field.Name)); break;
                }
            }
            if (sub != null)
            {
                w.F32(sub.Float("lastX"));
                w.F32(sub.Float("lastY"));
                var subMover = sub.OptionalChild("Mover");
                var subMoverRef = sub.OptionalChild("MoverRef");
                if (subMover != null && subMoverRef != null)
                    throw subMoverRef.Error("A parent mover can not have both <Mover> and <MoverRef>.");
                WriteMovementLink(sub, subMover, subMoverRef, w, version);
                sub.Done();
            }
            if (IsSet(f, MovementObjectBit))
            {
                w.F32(n.Float("currentX"));
                w.F32(n.Float("currentY"));
            }
            n.Done();
        }

        #endregion

        #region CurvedVal

        // A PopCap CurvedVal: a constant (any nonzero first byte, normally 1) or a curve definition string.
        static void ReadCurvedVal(BinReader r, XElement parent, string name)
        {
            var el = new XElement(name);
            byte tag = r.U8();
            if (tag > 0)
            {
                el.Add(new XAttribute("value", r.F32()));
                if (tag != 1)
                    el.Add(new XAttribute("tag", tag));
            }
            else
            {
                AddString(el, "curve", r.Str());
            }
            parent.Add(el);
        }

        static void WriteCurvedVal(Node parent, BinWriter w, string name)
        {
            var n = parent.RequiredChild(name);
            if (n.Has("value"))
            {
                byte tag = n.Has("tag") ? n.UInt8("tag") : (byte)1;
                if (tag == 0)
                    throw n.Error("A tag of 0 means a curve; use 'curve' instead.", "tag");
                w.U8(tag);
                w.F32(n.Float("value"));
            }
            else if (n.Has("curve"))
            {
                w.U8(0);
                w.Str(n.Bytes("curve"));
            }
            else
            {
                throw n.Error("Expected a 'value' or 'curve' attribute.");
            }
            n.Done();
        }

        #endregion

        #region Line

        static void ReadLine(BinReader r, XElement el)
        {
            byte f = r.U8();
            el.Add(new XAttribute("ax", r.F32()), new XAttribute("ay", r.F32()),
                   new XAttribute("bx", r.F32()), new XAttribute("by", r.F32()));
            if (IsSet(f, 0)) el.Add(new XAttribute("velocityX", r.F32()));
            if (IsSet(f, 1)) el.Add(new XAttribute("velocityY", r.F32()));
            AddExtraFlags(el, ExtraFlags, f, Bits(0, 1));
        }

        static void WriteLine(Node n, BinWriter w)
        {
            uint f = n.ExtraFlags(ExtraFlags, Bits(0, 1), 8);
            if (n.Has("velocityX")) f |= Bit(0);
            if (n.Has("velocityY")) f |= Bit(1);
            w.U8((byte)f);
            w.F32(n.Float("ax"));
            w.F32(n.Float("ay"));
            w.F32(n.Float("bx"));
            w.F32(n.Float("by"));
            if (n.Has("velocityX")) w.F32(n.Float("velocityX"));
            if (n.Has("velocityY")) w.F32(n.Float("velocityY"));
        }

        #endregion

        #region Poly

        const int PolyFlagsBVersion = 0x23;

        // poly_normaldir choices in LevelEditor_Choices.cfg, stored as a signed byte
        static readonly string[] NormalDirs = { "both", "keepOut" };
        const string NormalDirKeepIn = "keepIn";

        /// <summary>
        /// Reads the fields shared by Poly and Brick (which extends Poly).
        /// </summary>
        static void ReadPolyFields(BinReader r, int version, XElement el, string rotationName, bool hasPoints)
        {
            byte fa = r.U8();
            byte fb = version >= PolyFlagsBVersion ? r.U8() : (byte)0;
            if (IsSet(fa, 0)) el.Add(new XAttribute("isBox", "true"));
            if (IsSet(fa, 6)) el.Add(new XAttribute("imageTile", "true"));
            if (IsSet(fa, 7)) el.Add(new XAttribute("mirror", "true"));
            if (IsSet(fa, 2)) el.Add(new XAttribute(rotationName, r.F32()));
            if (IsSet(fa, 3)) el.Add(new XAttribute("angularVelocity", r.F32()));
            if (IsSet(fa, 5)) el.Add(new XAttribute("scale", r.F32()));
            if (IsSet(fa, 1))
            {
                sbyte normalDir = r.I8();
                el.Add(new XAttribute("normalDir", normalDir == -1 ? NormalDirKeepIn : normalDir == 0 || normalDir == 1 ? NormalDirs[normalDir] : normalDir.ToString(Inv)));
            }
            if (IsSet(fa, 4)) el.Add(new XAttribute("x", r.F32()), new XAttribute("y", r.F32()));
            if (hasPoints)
            {
                int numPoints = r.I32();
                if (numPoints < 0 || numPoints > r.Remaining / 8)
                    throw new LevelFormatException($"invalid number of polygon points {numPoints}");
                var points = new StringBuilder();
                for (int i = 0; i < numPoints; i++)
                {
                    if (i != 0) points.Append(' ');
                    points.Append(r.F32()).Append(',').Append(r.F32());
                }
                el.Add(new XAttribute("points", points.ToString()));
            }
            if (IsSet(fb, 0)) el.Add(new XAttribute("growTimer", r.U8()));
            if (IsSet(fb, 1)) el.Add(new XAttribute("growType", r.I32()));
            if (IsSet(fb, 2)) el.Add(new XAttribute("flashTimer", r.I16()));
            if (version >= PolyFlagsBVersion && !IsSet(fb, 3)) el.Add(new XAttribute("flashOnGrow", "false"));
            AddExtraFlags(el, "extraFlagsB", fb, Bits(0, 1, 2, 3));
        }

        static void WritePolyFields(Node n, BinWriter w, int version, string rotationName, bool hasPoints)
        {
            uint fa = 0;
            if (n.Bool("isBox", false)) fa |= Bit(0);
            if (n.Has("normalDir")) fa |= Bit(1);
            if (n.Has(rotationName)) fa |= Bit(2);
            if (n.Has("angularVelocity")) fa |= Bit(3);
            if (n.HasPair("x", "y")) fa |= Bit(4);
            if (n.Has("scale")) fa |= Bit(5);
            if (n.Bool("imageTile", false)) fa |= Bit(6);
            if (n.Bool("mirror", false)) fa |= Bit(7);
            uint fb = n.ExtraFlags("extraFlagsB", Bits(0, 1, 2, 3), 8);
            if (n.Has("growTimer")) fb |= Bit(0);
            if (n.Has("growType")) fb |= Bit(1);
            if (n.Has("flashTimer")) fb |= Bit(2);
            if (version >= PolyFlagsBVersion)
            {
                if (n.Bool("flashOnGrow", true)) fb |= Bit(3);
            }
            else if (fb != 0 || n.Has("flashOnGrow"))
            {
                throw n.Error($"'growTimer', 'growType', 'flashTimer', 'flashOnGrow' and 'extraFlagsB' are not stored before file version 0x{PolyFlagsBVersion:X2}.");
            }

            w.U8((byte)fa);
            if (version >= PolyFlagsBVersion)
                w.U8((byte)fb);
            if (n.Has(rotationName)) w.F32(n.Float(rotationName));
            if (n.Has("angularVelocity")) w.F32(n.Float("angularVelocity"));
            if (n.Has("scale")) w.F32(n.Float("scale"));
            if (n.Has("normalDir"))
            {
                string text = n.String("normalDir");
                int value = text == NormalDirKeepIn ? -1 : Array.IndexOf(NormalDirs, text);
                if (value == -1 && text != NormalDirKeepIn && (!int.TryParse(text, NumberStyles.AllowLeadingSign, Inv, out value) || value < sbyte.MinValue || value > sbyte.MaxValue))
                    throw n.Error($"'normalDir': unknown value '{text}'. Expected keepOut, keepIn, both, or a number from -128 to 127.", "normalDir");
                w.I8((sbyte)value);
            }
            if (IsSet(fa, 4))
            {
                w.F32(n.Float("x"));
                w.F32(n.Float("y"));
            }
            if (hasPoints)
            {
                var points = n.Points("points", 2);
                w.I32(points.Count);
                foreach (var p in points)
                {
                    w.F32(p[0]);
                    w.F32(p[1]);
                }
            }
            if (n.Has("growTimer")) w.U8(n.UInt8("growTimer"));
            if (n.Has("growType")) w.I32(n.Int32("growType"));
            if (n.Has("flashTimer")) w.I16(n.Int16("flashTimer"));
        }

        static void ReadPoly(BinReader r, int version, XElement el) => ReadPolyFields(r, version, el, "rotation", true);

        static void WritePoly(Node n, BinWriter w, int version) => WritePolyFields(n, w, version, "rotation", true);

        #endregion

        #region Ball

        const int BallFlagsBVersion = 0x52;

        static void ReadBall(BinReader r, int version, XElement el)
        {
            byte fa = r.U8();
            byte fb = version >= BallFlagsBVersion ? r.U8() : (byte)0;
            if (!IsSet(fa, 0)) el.Add(new XAttribute("isStatic", "false"));
            if (!IsSet(fa, 4)) el.Add(new XAttribute("ballCollide", "false"));
            if (IsSet(fa, 1)) el.Add(new XAttribute("x", r.F32()), new XAttribute("y", r.F32()));
            el.Add(new XAttribute("radius", r.F32()));
            AddExtraFlags(el, "extraFlagsA", fa, Bits(0, 1, 4));
            AddExtraFlags(el, "extraFlagsB", fb, 0);
        }

        static void WriteBall(Node n, BinWriter w, int version)
        {
            uint fa = n.ExtraFlags("extraFlagsA", Bits(0, 1, 4), 8);
            uint fb = n.ExtraFlags("extraFlagsB", 0, 8);
            if (n.Bool("isStatic", true)) fa |= Bit(0);
            if (n.HasPair("x", "y")) fa |= Bit(1);
            if (n.Bool("ballCollide", true)) fa |= Bit(4);
            w.U8((byte)fa);
            if (version >= BallFlagsBVersion)
                w.U8((byte)fb);
            else if (fb != 0)
                throw n.Error($"'extraFlagsB' is not stored before file version 0x{BallFlagsBVersion:X2}.", "extraFlagsB");
            if (IsSet(fa, 1))
            {
                w.F32(n.Float("x"));
                w.F32(n.Float("y"));
            }
            w.F32(n.Float("radius"));
        }

        #endregion

        #region Brick

        // brick_type choices in LevelEditor_Choices.cfg
        static readonly string[] BrickTypes = { "normalCurved", "outerCurved", "outerCurved90", "innerCurved", "innerCurved90", "straight" };
        // brick_anchor choices: bit 0 = left, bit 1 = right
        static readonly string[] BrickAnchors = { "none", "left", "right", "both" };

        static void ReadBrick(BinReader r, int version, XElement el)
        {
            // A brick is a Poly without points; its Poly rotation is separate from the brick's own rotation
            ReadPolyFields(r, version, el, "polyRotation", false);
            ushort fc = r.U16();
            int anchor = fc & 3;
            if (anchor != 0) el.Add(new XAttribute("anchor", BrickAnchors[anchor]));
            if (IsSet(fc, 8)) el.Add(new XAttribute("pivotOffsetX", r.F32()));
            if (IsSet(fc, 9)) el.Add(new XAttribute("pivotOffsetY", r.F32()));
            if (IsSet(fc, 2))
            {
                byte type = r.U8();
                el.Add(new XAttribute("type", type < BrickTypes.Length ? BrickTypes[type] : type.ToString(Inv)));
            }
            if (IsSet(fc, 3)) el.Add(new XAttribute("subdivisions", r.U8()));
            if (IsSet(fc, 5)) el.Add(new XAttribute("leftAngle", r.F32()));
            if (IsSet(fc, 6))
            {
                // The game writes the right angle twice; on read the second one wins
                uint rightAngle = r.U32(), rightAngle2 = r.U32();
                el.Add(new XAttribute("rightAngle", FormatFloat(rightAngle)));
                if (rightAngle2 != rightAngle)
                    el.Add(new XAttribute("rightAngle2", FormatFloat(rightAngle2)));
            }
            if (IsSet(fc, 4)) el.Add(new XAttribute("angle", r.F32()));
            if (IsSet(fc, 7)) el.Add(new XAttribute("thickness", r.F32()));
            if (IsSet(fc, 10)) el.Add(new XAttribute("texFlip", "true"));
            el.Add(new XAttribute("radius", r.F32()), new XAttribute("rotation", r.F32()));
            el.Add(new XAttribute("outlineIndices", $"{r.U8()} {r.U8()} {r.U8()} {r.U8()}"));
            AddExtraFlags(el, "extraFlagsC", fc, 0x7FF);
        }

        static void WriteBrick(Node n, BinWriter w, int version)
        {
            WritePolyFields(n, w, version, "polyRotation", false);

            uint fc = n.ExtraFlags("extraFlagsC", 0x7FF, 16);
            if (n.Has("anchor")) fc |= (uint)n.Choice("anchor", BrickAnchors, 3);
            if (n.Has("type")) fc |= Bit(2);
            if (n.Has("subdivisions")) fc |= Bit(3);
            if (n.Has("angle")) fc |= Bit(4);
            if (n.Has("leftAngle")) fc |= Bit(5);
            if (n.Has("rightAngle")) fc |= Bit(6);
            if (n.Has("thickness")) fc |= Bit(7);
            if (n.Has("pivotOffsetX")) fc |= Bit(8);
            if (n.Has("pivotOffsetY")) fc |= Bit(9);
            if (n.Bool("texFlip", false)) fc |= Bit(10);
            if (n.Has("rightAngle2") && !n.Has("rightAngle"))
                throw n.Error("'rightAngle2' needs 'rightAngle'.", "rightAngle2");

            w.U16((ushort)fc);
            if (n.Has("pivotOffsetX")) w.F32(n.Float("pivotOffsetX"));
            if (n.Has("pivotOffsetY")) w.F32(n.Float("pivotOffsetY"));
            if (n.Has("type")) w.U8(n.Choice("type", BrickTypes));
            if (n.Has("subdivisions")) w.U8(n.UInt8("subdivisions"));
            if (n.Has("leftAngle")) w.F32(n.Float("leftAngle"));
            if (n.Has("rightAngle"))
            {
                uint rightAngle = n.Float("rightAngle");
                w.F32(rightAngle);
                w.F32(n.Has("rightAngle2") ? n.Float("rightAngle2") : rightAngle);
            }
            if (n.Has("angle")) w.F32(n.Float("angle"));
            if (n.Has("thickness")) w.F32(n.Float("thickness"));
            w.F32(n.Float("radius"));
            w.F32(n.Float("rotation"));
            var indices = n.Tuples("outlineIndices", 1, 1);
            if (indices.Count != 4)
                throw n.Error("'outlineIndices' must have 4 numbers.", "outlineIndices");
            foreach (var index in indices)
                w.U8(n.ParseByte(index[0], "outlineIndices"));
        }

        #endregion

        #region Hole

        static void ReadHole(BinReader r, int version, XElement el)
        {
            byte f = r.U8();
            if (IsSet(f, 0)) el.Add(new XAttribute("circular", "true"));
            el.Add(new XAttribute("width", r.I32()), new XAttribute("height", r.I32()));
            if (IsSet(f, 1)) el.Add(new XAttribute("activeTimer", r.I16()));
            if (IsSet(f, 3)) el.Add(new XAttribute("score", r.I32()));
            if (IsSet(f, 5)) el.Add(new XAttribute("outDelay", r.I32()));
            if (IsSet(f, 4))
            {
                var destination = new XElement("Destination");
                if (r.PeekI32() == 1)
                    destination.Add(ReadEntry(r, version));
                else
                    destination.Add(new XAttribute("link", r.I32()));
                el.Add(destination);
            }
            if (IsSet(f, 2)) el.Add(new XAttribute("x", r.F32()), new XAttribute("y", r.F32()));
            if (IsSet(f, 6)) el.Add(new XAttribute("velocityX", r.F32()), new XAttribute("velocityY", r.F32()));
            AddExtraFlags(el, ExtraFlags, f, 0x7F);
        }

        static void WriteHole(Node n, BinWriter w, int version)
        {
            var destination = n.OptionalChild("Destination");
            uint f = n.ExtraFlags(ExtraFlags, 0x7F, 8);
            if (n.Bool("circular", false)) f |= Bit(0);
            if (n.Has("activeTimer")) f |= Bit(1);
            if (n.HasPair("x", "y")) f |= Bit(2);
            if (n.Has("score")) f |= Bit(3);
            if (destination != null) f |= Bit(4);
            if (n.Has("outDelay")) f |= Bit(5);
            if (n.HasPair("velocityX", "velocityY")) f |= Bit(6);
            w.U8((byte)f);
            w.I32(n.Int32("width"));
            w.I32(n.Int32("height"));
            if (n.Has("activeTimer")) w.I16(n.Int16("activeTimer"));
            if (n.Has("score")) w.I32(n.Int32("score"));
            if (n.Has("outDelay")) w.I32(n.Int32("outDelay"));
            if (destination != null)
            {
                var entries = destination.RemainingChildren();
                if (destination.Has("link"))
                {
                    if (entries.Count != 0)
                        throw destination.Error("A destination has either a 'link' or an entry, not both.");
                    int link = destination.Int32("link");
                    if (link == 1)
                        throw destination.Error("Link 1 means the entry follows inline; put the entry inside <Destination> instead.", "link");
                    w.I32(link);
                }
                else
                {
                    if (entries.Count != 1 || entries[0].Name == EntryRefName)
                        throw destination.Error("A destination must have a 'link' attribute or exactly one entry.");
                    destination.UseChild(entries[0].Element);
                    WriteEntry(entries[0], w, version);
                }
                destination.Done();
            }
            if (IsSet(f, 2))
            {
                w.F32(n.Float("x"));
                w.F32(n.Float("y"));
            }
            if (IsSet(f, 6))
            {
                w.F32(n.Float("velocityX"));
                w.F32(n.Float("velocityY"));
            }
        }

        #endregion

        #region Emitter

        const int EmitterPosCurvesBit = 13;

        // Flag bits that are plain booleans
        static readonly (int Bit, string Name)[] EmitterBools =
        {
            (0, "circular"), (1, "attached"), (2, "additive"), (3, "orientRotToVel"), (4, "randomStart"), (14, "triggerSleep"),
        };

        // Optional groups of values, each present when its flag bit is set: (bit, curved values, floats)
        static readonly (int Bit, string[] Curves, string[] Floats)[] EmitterGroups =
        {
            (12, new[] { "Rotation", "RotationVel" }, new[] { "rotationRand" }),
            (7, new[] { "Scale", "ScaleVel" }, new[] { "scaleRand" }),
            (8, new[] { "ColorR", "ColorG", "ColorB" }, new string[0]),
            (9, new[] { "Alpha" }, new string[0]),
            (10, new[] { "VelX", "VelY" }, new[] { "velXRand", "velYRand", "accX", "gravity" }),
            (11, new string[0], new[] { "polarVel", "polarVelRand", "polarAcc", "velRot", "velRotRand" }),
            (6, new string[0], new[] { "velocityX", "velocityY" }),
        };

        static void ReadEmitter(BinReader r, XElement el)
        {
            int dataVersion = r.I32();
            el.Add(new XAttribute("dataVersion", dataVersion));
            ushort f = r.U16();
            foreach (var (bit, name) in EmitterBools)
            {
                if (IsSet(f, bit)) el.Add(new XAttribute(name, "true"));
            }
            byte[] image = r.Str();
            AddString(el, "image", image);
            el.Add(new XAttribute("width", r.I32()), new XAttribute("height", r.I32()));
            var holder = new XElement("Holder");
            if (dataVersion >= 2)
            {
                el.Add(new XAttribute("fastForward", r.I32()), new XAttribute("hardwareRate", r.F32()));
                AddString(el, "triggerObj", r.Str());
                el.Add(new XAttribute("rateIsLooping", r.U8()));
                if (IsSet(f, EmitterPosCurvesBit))
                {
                    ReadCurvedVal(r, holder, "PosX");
                    ReadCurvedVal(r, holder, "PosY");
                }
            }
            else if (IsSet(f, EmitterPosCurvesBit))
            {
                el.Add(new XAttribute("hasPosCurves", "true"));
            }
            if (IsSet(f, 5)) el.Add(new XAttribute("x", r.F32()), new XAttribute("y", r.F32()));
            // The game writes the image twice; on read the second one wins
            byte[] image2 = r.Str();
            if (!image2.SequenceEqual(image))
                AddString(el, "image2", image2);
            el.Add(new XAttribute("randRate", r.F32()), new XAttribute("softwareRate", r.F32()), new XAttribute("spiralCount", r.F32()));
            el.Add(new XAttribute("max", r.I32()));
            el.Add(new XAttribute("alphaFadeAt", r.F32()), new XAttribute("alphaInto", r.F32()), new XAttribute("duration", r.F32()));
            ReadCurvedVal(r, holder, "Rate");
            ReadCurvedVal(r, holder, "Distrib");
            foreach (var (bit, groupCurves, floats) in EmitterGroups)
            {
                if (!IsSet(f, bit))
                    continue;
                foreach (var curve in groupCurves)
                    ReadCurvedVal(r, holder, curve);
                foreach (var name in floats)
                    el.Add(new XAttribute(name, r.F32()));
            }
            el.Add(holder.Elements());
            uint named = Bits(5, 6, 7, 8, 9, 10, 11, 12, EmitterPosCurvesBit);
            foreach (var (bit, _) in EmitterBools)
                named |= Bit(bit);
            AddExtraFlags(el, ExtraFlags, f, named);
        }

        static void WriteEmitter(Node n, BinWriter w)
        {
            int dataVersion = n.Int32("dataVersion");
            uint named = Bits(5, 6, 7, 8, 9, 10, 11, 12, EmitterPosCurvesBit);
            uint f = 0;
            foreach (var (bit, name) in EmitterBools)
            {
                named |= Bit(bit);
                if (n.Bool(name, false)) f |= Bit(bit);
            }
            f |= n.ExtraFlags(ExtraFlags, named, 16);
            if (n.HasPair("x", "y")) f |= Bit(5);
            foreach (var (bit, curves, floats) in EmitterGroups)
            {
                if (EmitterGroupPresent(n, curves, floats)) f |= Bit(bit);
            }
            bool posCurves = EmitterGroupPresent(n, new[] { "PosX", "PosY" }, new string[0]);
            if (dataVersion >= 2)
            {
                if (n.Has("hasPosCurves"))
                    throw n.Error("With dataVersion 2 or later, flag 13 is set by <PosX> and <PosY> elements.", "hasPosCurves");
                if (posCurves) f |= Bit(EmitterPosCurvesBit);
            }
            else
            {
                if (posCurves)
                    throw n.Error("<PosX> and <PosY> are only stored with dataVersion 2 or later; use hasPosCurves instead.");
                if (n.Bool("hasPosCurves", false)) f |= Bit(EmitterPosCurvesBit);
            }

            w.I32(dataVersion);
            w.U16((ushort)f);
            byte[] image = n.Bytes("image");
            w.Str(image);
            w.I32(n.Int32("width"));
            w.I32(n.Int32("height"));
            if (dataVersion >= 2)
            {
                w.I32(n.Int32("fastForward"));
                w.F32(n.Float("hardwareRate"));
                w.Str(n.Bytes("triggerObj"));
                w.U8(n.UInt8("rateIsLooping"));
                if (posCurves)
                {
                    WriteCurvedVal(n, w, "PosX");
                    WriteCurvedVal(n, w, "PosY");
                }
            }
            if (IsSet(f, 5))
            {
                w.F32(n.Float("x"));
                w.F32(n.Float("y"));
            }
            w.Str(n.Has("image2") ? n.Bytes("image2") : image);
            w.F32(n.Float("randRate"));
            w.F32(n.Float("softwareRate"));
            w.F32(n.Float("spiralCount"));
            w.I32(n.Int32("max"));
            w.F32(n.Float("alphaFadeAt"));
            w.F32(n.Float("alphaInto"));
            w.F32(n.Float("duration"));
            WriteCurvedVal(n, w, "Rate");
            WriteCurvedVal(n, w, "Distrib");
            foreach (var (bit, curves, floats) in EmitterGroups)
            {
                if (!IsSet(f, bit))
                    continue;
                foreach (var curve in curves)
                    WriteCurvedVal(n, w, curve);
                foreach (var name in floats)
                    w.F32(n.Float(name));
            }
        }

        /// <summary>
        /// Whether a group of emitter values is present; a group must be given completely or not at all.
        /// </summary>
        static bool EmitterGroupPresent(Node n, string[] curves, string[] floats)
        {
            var present = curves.Where(n.HasChild).Concat(floats.Where(n.Has)).ToList();
            if (present.Count != 0 && present.Count != curves.Length + floats.Length)
            {
                var missing = curves.Where(c => !n.HasChild(c)).Select(c => $"<{c}>").Concat(floats.Where(v => !n.Has(v)).Select(v => $"'{v}'"));
                throw n.Error($"{string.Join(", ", present)} must be given together with {string.Join(", ", missing)}.");
            }
            return present.Count != 0;
        }

        #endregion

        #region PeggleEdit generators

        static void ReadGeneratorPosition(BinReader r, XElement el)
        {
            byte hasPosition = r.U8();
            if (hasPosition > 0)
            {
                el.Add(new XAttribute("x", r.F32()), new XAttribute("y", r.F32()));
                if (hasPosition != 1)
                    el.Add(new XAttribute("positionTag", hasPosition));
            }
        }

        static void WriteGeneratorPosition(Node n, BinWriter w)
        {
            if (n.HasPair("x", "y"))
            {
                byte tag = n.Has("positionTag") ? n.UInt8("positionTag") : (byte)1;
                if (tag == 0)
                    throw n.Error("'positionTag' can not be 0 when there is a position.", "positionTag");
                w.U8(tag);
                w.F32(n.Float("x"));
                w.F32(n.Float("y"));
            }
            else
            {
                w.U8(0);
            }
        }

        static void ReadPegGenerator(BinReader r, XElement el)
        {
            ReadGeneratorPosition(r, el);
            el.Add(new XAttribute("count", r.I32()), new XAttribute("maxCount", r.I32()));
            el.Add(new XAttribute("radiusX", r.F32()), new XAttribute("radiusY", r.F32()), new XAttribute("angularOffset", r.F32()));
        }

        static void WritePegGenerator(Node n, BinWriter w)
        {
            WriteGeneratorPosition(n, w);
            w.I32(n.Int32("count"));
            w.I32(n.Int32("maxCount"));
            w.F32(n.Float("radiusX"));
            w.F32(n.Float("radiusY"));
            w.F32(n.Float("angularOffset"));
        }

        static void ReadBrickGenerator(BinReader r, XElement el)
        {
            ReadGeneratorPosition(r, el);
            el.Add(new XAttribute("count", r.I32()), new XAttribute("maxCount", r.I32()));
            el.Add(new XAttribute("innerRadius", r.F32()), new XAttribute("angularOffset", r.F32()), new XAttribute("brickWidth", r.F32()));
        }

        static void WriteBrickGenerator(Node n, BinWriter w)
        {
            WriteGeneratorPosition(n, w);
            w.I32(n.Int32("count"));
            w.I32(n.Int32("maxCount"));
            w.F32(n.Float("innerRadius"));
            w.F32(n.Float("angularOffset"));
            w.F32(n.Float("brickWidth"));
        }

        static void ReadCurveGenerator(BinReader r, XElement el)
        {
            int count = r.U16();
            if ((count & CurveGenerator.HasPositionFlag) != 0)
                el.Add(new XAttribute("x", r.F32()), new XAttribute("y", r.F32()));
            int numPoints = count & CurveGenerator.PointCountMask;
            var path = new StringBuilder();
            for (int i = 0; i < numPoints; i++)
            {
                if (i != 0) path.Append(' ');
                path.Append(r.F32()).Append(',').Append(r.F32()).Append(',').Append(r.U8().ToString(Inv));
                byte padding = r.U8();
                if (padding != 0)
                    path.Append(',').Append(padding.ToString(Inv));
            }
            el.Add(new XAttribute("path", path.ToString()));
            if ((count & CurveGenerator.HasIntervalFlag) != 0)
                el.Add(new XAttribute("interval", r.I32()));
        }

        static void WriteCurveGenerator(Node n, BinWriter w)
        {
            var points = n.Tuples("path", 3, 4);
            if (points.Count > CurveGenerator.PointCountMask)
                throw n.Error("Too many path points.", "path");
            bool hasInterval = n.Has("interval");
            bool hasPosition = n.HasPair("x", "y");
            w.U16((ushort)(points.Count | (hasInterval ? CurveGenerator.HasIntervalFlag : 0) | (hasPosition ? CurveGenerator.HasPositionFlag : 0)));
            if (hasPosition)
            {
                w.F32(n.Float("x"));
                w.F32(n.Float("y"));
            }
            foreach (var p in points)
            {
                w.F32(n.ParseFloat(p[0], "path"));
                w.F32(n.ParseFloat(p[1], "path"));
                w.U8(n.ParseByte(p[2], "path"));
                w.U8(p.Length > 3 ? n.ParseByte(p[3], "path") : (byte)0);
            }
            if (hasInterval)
                w.I32(n.Int32("interval"));
        }

        #endregion

        #region Value helpers

        static void ReadValue(BinReader r, XElement el, string name, Kind kind)
        {
            switch (kind)
            {
                case Kind.F32: el.Add(new XAttribute(name, r.F32())); break;
                case Kind.I32: el.Add(new XAttribute(name, r.I32())); break;
                case Kind.U8: el.Add(new XAttribute(name, r.U8())); break;
                case Kind.Argb: el.Add(new XAttribute(name, "#" + r.U32().ToString("X8", Inv))); break;
                case Kind.Str: AddString(el, name, r.Str()); break;
                case Kind.FlipperFlags:
                    byte value = r.U8();
                    el.Add(new XAttribute(name, value < FlipperFlagValues.Length ? FlipperFlagValues[value] : value.ToString(Inv)));
                    break;
            }
        }

        static void WriteValue(Node n, BinWriter w, string name, Kind kind)
        {
            switch (kind)
            {
                case Kind.F32: w.F32(n.Float(name)); break;
                case Kind.I32: w.I32(n.Int32(name)); break;
                case Kind.U8: w.U8(n.UInt8(name)); break;
                case Kind.Argb: w.U32(n.Colour(name)); break;
                case Kind.Str: w.Str(n.Bytes(name)); break;
                case Kind.FlipperFlags: w.U8(n.Choice(name, FlipperFlagValues)); break;
            }
        }

        static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        static void AddString(XElement el, string name, byte[] raw)
        {
            string text;
            try
            {
                text = StrictUtf8.GetString(raw);
                XmlConvert.VerifyXmlChars(text);
            }
            catch (Exception ex) when (ex is DecoderFallbackException || ex is XmlException)
            {
                text = null;
            }
            if (text == null || !StrictUtf8.GetBytes(text).SequenceEqual(raw))
                throw new LevelFormatException($"'{name}' on <{el.Name}> has text that is not valid UTF-8 or has characters XML can not hold");
            el.Add(new XAttribute(name, text));
        }

        static void AddExtraFlags(XElement el, string name, uint flags, uint named)
        {
            uint extra = flags & ~named;
            if (extra != 0)
                el.Add(new XAttribute(name, "0x" + extra.ToString("X", Inv)));
        }

        static uint Bit(int bit) => 1u << bit;

        static uint Bits(params int[] bits)
        {
            uint result = 0;
            foreach (var bit in bits)
                result |= Bit(bit);
            return result;
        }

        static bool IsSet(uint flags, int bit) => (flags & Bit(bit)) != 0;

        static string Hex(uint value, int digits) => "0x" + value.ToString("X" + digits, Inv);

        /// <summary>
        /// Formats a float as the shortest plain decimal that parses back to exactly the same bits, or as its bits in
        /// hex for NaN and infinity.
        /// </summary>
        internal static string FormatFloat(uint bits)
        {
            if ((bits & 0x7F800000) == 0x7F800000)
                return Hex(bits, 8);
            if (bits == 0x80000000)
                return "-0";
            float value = BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
            for (int precision = 1; precision <= 9; precision++)
            {
                string s = value.ToString("G" + precision, Inv);
                if (TryParseFloatBits(s, out uint parsed) && parsed == bits)
                {
                    string plain = ToPlainDecimal(s);
                    return TryParseFloatBits(plain, out parsed) && parsed == bits ? plain : s;
                }
            }
            return Hex(bits, 8);
        }

        internal static bool TryParseFloatBits(string s, out uint bits)
        {
            bits = 0;
            s = s.Trim();
            // A decimal never starts with 0x, so this is the float's bits
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                return uint.TryParse(s.Substring(2), NumberStyles.AllowHexSpecifier, Inv, out bits);
            if (s.Length == 0 || char.IsLetter(s[s.Length - 1]) && s[s.Length - 1] != 'e' && s[s.Length - 1] != 'E')
                return false;
            float value;
            try
            {
                if (!float.TryParse(s, NumberStyles.Float, Inv, out value) || float.IsInfinity(value) || float.IsNaN(value))
                    return false;
            }
            catch (OverflowException)
            {
                return false;
            }
            bits = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // .NET Framework parses "-0" as positive zero
            if (value == 0 && s.StartsWith("-", StringComparison.Ordinal))
                bits = 0x80000000;
            return true;
        }

        static string ToPlainDecimal(string s)
        {
            int e = s.IndexOfAny(new[] { 'E', 'e' });
            if (e == -1)
                return s;
            bool negative = s[0] == '-';
            string mantissa = s.Substring(negative ? 1 : 0, e - (negative ? 1 : 0));
            int exponent = int.Parse(s.Substring(e + 1), NumberStyles.AllowLeadingSign, Inv);
            int point = mantissa.IndexOf('.');
            string digits = mantissa.Replace(".", "");
            int newPoint = (point == -1 ? mantissa.Length : point) + exponent;
            string result;
            if (newPoint <= 0)
                result = "0." + new string('0', -newPoint) + digits;
            else if (newPoint >= digits.Length)
                result = digits + new string('0', newPoint - digits.Length);
            else
                result = digits.Substring(0, newPoint) + "." + digits.Substring(newPoint);
            if (result.Contains('.'))
                result = result.TrimEnd('0').TrimEnd('.');
            return (negative ? "-" : "") + result;
        }

        #endregion

        #region Binary reader / writer

        class LevelFormatException : Exception
        {
            public LevelFormatException(string message) : base(message) { }
        }

        class BinReader
        {
            readonly byte[] mData;

            public int Position { get; private set; }
            public int Remaining => mData.Length - Position;

            public BinReader(byte[] data) { mData = data; }

            void Need(int n)
            {
                if (Position + n > mData.Length)
                    throw new LevelFormatException($"unexpected end of file at offset 0x{Position:X}");
            }

            public byte U8() { Need(1); return mData[Position++]; }
            public sbyte I8() => (sbyte)U8();
            public ushort U16() { Need(2); var v = BitConverter.ToUInt16(mData, Position); Position += 2; return v; }
            public short I16() => (short)U16();
            public uint U32() { Need(4); var v = BitConverter.ToUInt32(mData, Position); Position += 4; return v; }
            public int I32() => (int)U32();
            public int PeekI32() { Need(4); return BitConverter.ToInt32(mData, Position); }
            public string F32() => FormatFloat(U32());

            public byte[] Str()
            {
                int length = I16();
                if (length < 0)
                    throw new LevelFormatException($"negative string length at offset 0x{Position - 2:X}");
                Need(length);
                var result = new byte[length];
                Array.Copy(mData, Position, result, 0, length);
                Position += length;
                return result;
            }
        }

        class BinWriter
        {
            readonly MemoryStream mStream = new MemoryStream();

            public void U8(byte v) => mStream.WriteByte(v);
            public void I8(sbyte v) => mStream.WriteByte((byte)v);
            public void U16(ushort v) => Bytes(BitConverter.GetBytes(v));
            public void I16(short v) => Bytes(BitConverter.GetBytes(v));
            public void U32(uint v) => Bytes(BitConverter.GetBytes(v));
            public void I32(int v) => Bytes(BitConverter.GetBytes(v));
            public void F32(uint bits) => U32(bits);
            public void Bytes(byte[] b) => mStream.Write(b, 0, b.Length);

            public void Str(byte[] b)
            {
                I16((short)b.Length);
                Bytes(b);
            }

            public byte[] ToArray() => mStream.ToArray();
        }

        #endregion

        #region XML reader

        /// <summary>
        /// Reads attributes and child elements of an element, tracking which have been used so that anything left
        /// over (e.g. a misspelt attribute) is reported as an error.
        /// </summary>
        class Node
        {
            readonly HashSet<XName> mUsedAttributes = new HashSet<XName>();
            readonly HashSet<XElement> mUsedChildren = new HashSet<XElement>();
            readonly bool mEmpty;

            public XElement Element { get; }
            public string Name => Element.Name.LocalName;

            public Node(XElement element) : this(element, false) { }

            Node(XElement element, bool empty)
            {
                Element = element;
                mEmpty = empty;
                if (element.Name.Namespace != XNamespace.None)
                    throw Error($"Unexpected namespace '{element.Name.NamespaceName}'.");
            }

            /// <summary>
            /// A node with no attributes, for an optional element that is not present.
            /// </summary>
            public static Node Empty(Node parent) => new Node(new XElement(PropertiesName), true) { mParent = parent };

            Node mParent;

            public LevelXmlException Error(string message, string attribute = null)
            {
                IXmlLineInfo info = attribute != null ? (IXmlLineInfo)Element.Attribute(attribute) : null;
                if (info == null || !info.HasLineInfo())
                    info = mEmpty ? mParent.Element : Element;
                var where = info.HasLineInfo() ? $" (line {info.LineNumber}, column {info.LinePosition})" : "";
                return new LevelXmlException($"<{Name}>: {message}{where}", info.LineNumber, info.LinePosition);
            }

            public bool Has(string name) => Element.Attribute(name) != null;

            public bool HasPair(string a, string b)
            {
                if (Has(a) != Has(b))
                    throw Error($"'{a}' and '{b}' must both be present or both be absent.", Has(a) ? a : b);
                return Has(a);
            }

            public bool HasChild(string name) => Element.Element(name) != null;

            public string String(string name)
            {
                var attribute = Element.Attribute(name);
                if (attribute == null)
                    throw Error($"Missing attribute '{name}'.");
                mUsedAttributes.Add(name);
                return attribute.Value;
            }

            public byte[] Bytes(string name)
            {
                byte[] result = Encoding.UTF8.GetBytes(String(name));
                if (result.Length > short.MaxValue)
                    throw Error($"'{name}' is too long.", name);
                return result;
            }

            public uint Float(string name) => ParseFloat(String(name), name);

            public uint ParseFloat(string text, string name)
            {
                if (!TryParseFloatBits(text, out uint bits))
                    throw Error($"'{name}': '{text}' is not a valid number.", name);
                return bits;
            }

            public bool Bool(string name, bool defaultValue)
            {
                if (!Has(name))
                    return defaultValue;
                switch (String(name))
                {
                    case "true": return true;
                    case "false": return false;
                    default: throw Error($"'{name}' must be 'true' or 'false'.", name);
                }
            }

            long Integer(string name, long min, long max)
            {
                string text = String(name).Trim();
                bool hex = text.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
                bool ok = hex
                    ? long.TryParse(text.Substring(2), NumberStyles.AllowHexSpecifier, Inv, out long value)
                    : long.TryParse(text, NumberStyles.AllowLeadingSign, Inv, out value);
                // Hex is a bit pattern, so a signed field can be given as e.g. 0xFFFFFFFF for -1
                long range = max - min + 1;
                if (ok && hex && min < 0 && value > max && value < range)
                    value -= range;
                if (!ok || value < min || value > max)
                    throw Error($"'{name}': '{text}' is not a whole number between {min} and {max}.", name);
                return value;
            }

            public byte UInt8(string name) => (byte)Integer(name, byte.MinValue, byte.MaxValue);
            public short Int16(string name) => (short)Integer(name, short.MinValue, short.MaxValue);
            public int Int32(string name) => (int)Integer(name, int.MinValue, int.MaxValue);
            public uint UInt32(string name) => (uint)Integer(name, uint.MinValue, uint.MaxValue);

            public byte ParseByte(string text, string name)
            {
                if (!byte.TryParse(text, NumberStyles.None, Inv, out byte value))
                    throw Error($"'{name}': '{text}' is not a whole number between 0 and 255.", name);
                return value;
            }

            /// <summary>
            /// Reads a value given by name from a list of choices, or as a number.
            /// </summary>
            public byte Choice(string name, string[] choices) => (byte)Choice(name, choices, byte.MaxValue);

            public int Choice(string name, string[] choices, int max)
            {
                string text = String(name);
                int value = Array.IndexOf(choices, text);
                if (value == -1 && (!int.TryParse(text, NumberStyles.None, Inv, out value) || value > max))
                    throw Error($"'{name}': unknown value '{text}'. Expected one of {string.Join(", ", choices)}, or a number up to {max}.", name);
                return value;
            }

            public uint Colour(string name)
            {
                string text = String(name);
                if (text.Length != 9 || text[0] != '#' || !uint.TryParse(text.Substring(1), NumberStyles.AllowHexSpecifier, Inv, out uint value))
                    throw Error($"'{name}': '{text}' is not a colour of the form #AARRGGBB.", name);
                return value;
            }

            /// <summary>
            /// Reads an extraFlags attribute, which must not contain bits that are given by other attributes.
            /// </summary>
            public uint ExtraFlags(string name, uint namedBits, int width = 32)
            {
                if (!Has(name))
                    return 0;
                uint flags = UInt32(name);
                if ((flags & namedBits) != 0)
                    throw Error($"'{name}' contains bits that are set by other attributes: 0x{flags & namedBits:X}.", name);
                if (width < 32 && (flags >> width) != 0)
                    throw Error($"'{name}' has bits above bit {width - 1}.", name);
                return flags;
            }

            public List<uint[]> Points(string name, int arity)
            {
                return Tuples(name, arity, arity).Select(p => p.Select(v => ParseFloat(v, name)).ToArray()).ToList();
            }

            public List<string[]> Tuples(string name, int minArity, int maxArity)
            {
                var result = new List<string[]>();
                foreach (var item in String(name).Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var parts = item.Split(',');
                    if (parts.Length < minArity || parts.Length > maxArity)
                        throw Error($"'{name}': '{item}' should have {(minArity == maxArity ? minArity.ToString(Inv) : $"{minArity} to {maxArity}")} comma separated values.", name);
                    result.Add(parts);
                }
                return result;
            }

            public Node OptionalChild(string name)
            {
                var children = Element.Elements(name).ToList();
                if (children.Count > 1)
                    throw new Node(children[1]).Error($"Only one <{name}> element is allowed here.");
                if (children.Count == 0)
                    return null;
                mUsedChildren.Add(children[0]);
                return new Node(children[0]);
            }

            public Node RequiredChild(string name) => OptionalChild(name) ?? throw Error($"Missing element <{name}>.");

            public List<Node> RemainingChildren() =>
                Element.Elements().Where(e => !mUsedChildren.Contains(e)).Select(e => new Node(e)).ToList();

            public void UseChild(XElement child) => mUsedChildren.Add(child);

            /// <summary>
            /// Checks that every attribute and child element has been used.
            /// </summary>
            public void Done()
            {
                foreach (var attribute in Element.Attributes())
                {
                    if (attribute.IsNamespaceDeclaration)
                        continue;
                    if (!mUsedAttributes.Contains(attribute.Name))
                        throw Error($"Unknown attribute '{attribute.Name}'.", attribute.Name.LocalName);
                }
                foreach (var child in Element.Elements())
                {
                    if (!mUsedChildren.Contains(child))
                        throw new Node(child).Error($"Unexpected element <{child.Name}> inside <{Name}>.");
                }
                if (Element.Nodes().OfType<XText>().Any(t => !string.IsNullOrWhiteSpace(t.Value)))
                    throw Error("Unexpected text.");
            }
        }

        #endregion

        class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter() : base(CultureInfo.InvariantCulture) { }
            public override Encoding Encoding => new UTF8Encoding(false);
        }
    }

    /// <summary>
    /// The exception thrown when level XML can not be imported.
    /// </summary>
    public class LevelXmlException : Exception
    {
        public int LineNumber { get; }
        public int LinePosition { get; }

        public LevelXmlException(string message, int lineNumber, int linePosition)
            : base(message)
        {
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }
    }
}
