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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using IntelOrca.PeggleEdit.Tools;
using IntelOrca.PeggleEdit.Tools.Levels;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;
using IntelOrca.PeggleEdit.Tools.Pack;

namespace IntelOrca.PeggleEdit.Cli
{
    internal static class Program
    {
        const int ExitSuccess = 0;
        const int ExitFailure = 1;
        const int ExitUsage = 2;

        const string Usage = @"Usage:
  peggleedit-cli to-xml <level.dat>... [-o <output>] [--force]
  peggleedit-cli to-dat <level.xml>... [-o <output>] [--force]
  peggleedit-cli render <level.dat|level.xml>... [-o <output>] [--force]
                        [--peggle <dir>] [--no-interface] [--time <seconds>]
                        [--no-particles] [--software] [--master <name>]
                        [--power] [--devices <list>]

to-xml and to-dat convert Pego, Peggle and Peggle Nights level files to and from
XML. Converting a .dat to XML and back gives a byte-for-byte identical file.
Files that are not levels, or are damaged, are reported as errors.

render draws levels to 800x600 .png images as the game shows them at the start
of play (all pegs blue), with each emitter shown as it looks once it has been
running. With --time, the level is run from the start for that long instead, as
the game would: moving objects move and emitters start from nothing. The free
ball bucket is shown, and with --power the devices of the master's power. The
background is the image next to the level with the same name (.jp2, .jpg or
.png). Images used by the level are found in the game directory, which is the
parent of the level's directory if that is called 'levels', or given with
--peggle.

Options:
  -o, --output <path>  Output file, or output directory when converting several
                       files. Defaults to the input path with the new extension.
  -f, --force          Overwrite output files that already exist.
  --peggle <dir>       The Peggle Nights (or Peggle) game directory (render).
  --no-interface       Leave out the game's interface around the level (render).
  --time <seconds>     Show the level this long after it starts (render).
  --no-particles       Leave out the particles of emitters (render).
  --software           Show emitters as the game does without 3D acceleration,
                       which leaves most of them out (render).
  --master <name>      The master the level is played as: bjorn, jimmy, kattut,
                       splork, claude, renfield, tula, warren, cinderbottom,
                       masterhu or marina (render, default bjorn).
  --power              Show the master's power active: Kat Tut's pyramid,
                       Claude's flippers, the lid of Renfield and Lord
                       Cinderbottom, or Marina's lightning rod (render).
  --devices <list>     The devices to show instead, separated by commas: bucket,
                       pyramid, flippers, flippersraised, cover, lightningrod,
                       or none (render).
  -h, --help           Show this help.";

        static int Main(string[] args)
        {
            try
            {
                return Run(args);
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine("error: " + ex.Message);
                Console.Error.WriteLine();
                Console.Error.WriteLine(Usage);
                return ExitUsage;
            }
        }

        static int Run(string[] args)
        {
            if (args.Length == 0 || args[0] == "-h" || args[0] == "--help" || args[0] == "help")
            {
                Console.WriteLine(Usage);
                return args.Length == 0 ? ExitUsage : ExitSuccess;
            }

            bool toXml = false;
            bool render = false;
            switch (args[0])
            {
                case "to-xml": toXml = true; break;
                case "to-dat": toXml = false; break;
                case "render": render = true; break;
                default: throw new UsageException($"unknown command '{args[0]}'");
            }

            var inputs = new List<string>();
            string output = null;
            string pegglePath = null;
            bool force = false;
            var renderOptions = new RenderOptions();
            for (int i = 1; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-o":
                    case "--output":
                        if (++i >= args.Length)
                            throw new UsageException($"{args[i - 1]} needs a path");
                        output = args[i];
                        break;
                    case "-f":
                    case "--force":
                        force = true;
                        break;
                    case "--peggle" when render:
                        if (++i >= args.Length)
                            throw new UsageException($"{args[i - 1]} needs a path");
                        pegglePath = args[i];
                        if (!Directory.Exists(pegglePath))
                            throw new UsageException($"{pegglePath} is not a directory");
                        break;
                    case "--no-interface" when render:
                        renderOptions.DrawInterface = false;
                        break;
                    case "--time" when render:
                        if (++i >= args.Length)
                            throw new UsageException($"{args[i - 1]} needs a number of seconds");
                        if (!double.TryParse(args[i], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds) || seconds < 0 || seconds > 3600)
                            throw new UsageException($"'{args[i]}' is not a time in seconds");
                        renderOptions.Ticks = (int)Math.Round(seconds * 100);
                        break;
                    case "--no-particles" when render:
                        renderOptions.ShowParticles = false;
                        break;
                    case "--software" when render:
                        renderOptions.Software = true;
                        break;
                    case "--master" when render:
                        if (++i >= args.Length)
                            throw new UsageException($"{args[i - 1]} needs a name");
                        renderOptions.Master = ParseMaster(args[i]);
                        break;
                    case "--power" when render:
                        renderOptions.Power = true;
                        break;
                    case "--devices" when render:
                        if (++i >= args.Length)
                            throw new UsageException($"{args[i - 1]} needs a list of devices");
                        renderOptions.Devices = ParseDevices(args[i]);
                        break;
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return ExitSuccess;
                    default:
                        if (args[i].StartsWith("-", StringComparison.Ordinal) && args[i].Length > 1)
                            throw new UsageException($"unknown option '{args[i]}'");
                        inputs.Add(args[i]);
                        break;
                }
            }
            if (inputs.Count == 0)
                throw new UsageException("no input files");

            bool outputIsDirectory = output != null && (inputs.Count > 1 || Directory.Exists(output));
            if (outputIsDirectory)
            {
                try
                {
                    Directory.CreateDirectory(output);
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException ||
                                           ex is NotSupportedException)
                {
                    Console.Error.WriteLine($"{output}: {ex.Message}");
                    return ExitFailure;
                }
            }

            string extension = render ? ".png" : toXml ? ".xml" : ".dat";
            int failures = 0;
            var targets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var input in inputs)
            {
                string target = output == null
                    ? Path.ChangeExtension(input, extension)
                    : outputIsDirectory ? Path.Combine(output, Path.GetFileNameWithoutExtension(input) + extension) : output;
                try
                {
                    // Two inputs with the same name would be written to the same file
                    if (!targets.Add(Path.GetFullPath(target)))
                        throw new IOException($"{target} is also the output of an earlier input");
                    if (render)
                        Render(input, target, pegglePath, renderOptions, force);
                    else
                        Convert(input, target, toXml, force);
                    Console.WriteLine($"{input} -> {target}");
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is LevelXmlException ||
                                           ex is InvalidDataException || ex is ArgumentException || ex is NotSupportedException)
                {
                    Console.Error.WriteLine($"{input}: {ex.Message}");
                    failures++;
                }
            }
            return failures == 0 ? ExitSuccess : ExitFailure;
        }

        static void Convert(string input, string target, bool toXml, bool force)
        {
            if (Path.GetFullPath(input).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                throw new IOException("the output would overwrite the input");
            if (!force && File.Exists(target))
                throw new IOException($"{target} already exists (use --force to overwrite it)");

            if (toXml)
            {
                var data = File.ReadAllBytes(input);
                var xml = LevelXml.Export(data);
                File.WriteAllText(target, xml, new UTF8Encoding(false));
            }
            else
            {
                var xml = File.ReadAllText(input);
                var data = LevelXml.Import(xml);
                File.WriteAllBytes(target, data);
            }
        }

        class RenderOptions
        {
            public bool DrawInterface { get; set; } = true;
            public int? Ticks { get; set; }
            public bool ShowParticles { get; set; } = true;
            public bool Software { get; set; }
            public Master Master { get; set; } = Master.Bjorn;
            public bool Power { get; set; }
            public GameDevices? Devices { get; set; }

            public GameDevices GetDevices() =>
                Devices ?? (GameDevices.Bucket | (Power ? MasterInfo.Get(Master).PowerDevices : GameDevices.None));
        }

        static Master ParseMaster(string name)
        {
            var key = name.Replace(" ", "").Replace("-", "");
            foreach (var info in MasterInfo.All)
            {
                if (info.Master.ToString().Equals(key, StringComparison.OrdinalIgnoreCase) ||
                    info.Name.Replace(" ", "").Equals(key, StringComparison.OrdinalIgnoreCase))
                {
                    return info.Master;
                }
            }
            throw new UsageException($"unknown master '{name}'");
        }

        static GameDevices ParseDevices(string list)
        {
            var result = GameDevices.None;
            foreach (var name in list.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (name.Trim().Equals("none", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!Enum.TryParse<GameDevices>(name.Trim(), true, out var device) || !Enum.IsDefined(typeof(GameDevices), device))
                    throw new UsageException($"unknown device '{name}'");
                result |= device;
            }
            return result;
        }

        static void Render(string input, string target, string pegglePath, RenderOptions options, bool force)
        {
            if (Path.GetFullPath(input).Equals(Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
                throw new IOException("the output would overwrite the input");
            if (!force && File.Exists(target))
                throw new IOException($"{target} already exists (use --force to overwrite it)");

            var data = Path.GetExtension(input).Equals(".xml", StringComparison.OrdinalIgnoreCase)
                ? LevelXml.Import(File.ReadAllText(input))
                : File.ReadAllBytes(input);

            Level level;
            using (var reader = new LevelReader(data))
            {
                level = reader.Read();
                if (level == null)
                    throw new InvalidDataException(reader.Error ?? "not a level");
            }

            var directory = Path.GetDirectoryName(Path.GetFullPath(input));
            if (pegglePath == null && Path.GetFileName(directory).Equals("levels", StringComparison.OrdinalIgnoreCase))
                pegglePath = Path.GetDirectoryName(directory);
            // Registered for every input, so that one input's game is not used for the next
            LevelPack.RegisterPegglePath(pegglePath);
            LevelPack.Current = new LevelPack();

            var name = Path.GetFileNameWithoutExtension(input);
            var background = new[] { ".jp2", ".jpg", ".png" }
                .Select(x => Path.Combine(directory, name + x))
                .FirstOrDefault(File.Exists);
            if (background != null)
                level.Background = new PakImage(background);

            level.ShowParticles = options.ShowParticles;
            level.SoftwareRendering = options.Software;
            level.Devices = options.GetDevices();
            using (var image = level.RenderGameView(options.DrawInterface, options.Ticks))
            using (var stream = File.Create(target))
            {
                // Saved to a stream, so that a bad path is an IOException rather than a GDI+ error
                image.Save(stream, ImageFormat.Png);
            }
        }

        class UsageException : Exception
        {
            public UsageException(string message) : base(message) { }
        }
    }
}
