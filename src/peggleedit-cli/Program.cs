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
using System.IO;
using System.Text;
using IntelOrca.PeggleEdit.Tools.Levels.Xml;

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

Converts Pego, Peggle and Peggle Nights level files to and from XML. Converting
a .dat to XML and back gives a byte-for-byte identical file. Files that are not
levels, or are damaged, are reported as errors.

Options:
  -o, --output <path>  Output file, or output directory when converting several
                       files. Defaults to the input path with the new extension.
  -f, --force          Overwrite output files that already exist.
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

            bool toXml;
            switch (args[0])
            {
                case "to-xml": toXml = true; break;
                case "to-dat": toXml = false; break;
                default: throw new UsageException($"unknown command '{args[0]}'");
            }

            var inputs = new List<string>();
            string output = null;
            bool force = false;
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
                Directory.CreateDirectory(output);

            string extension = toXml ? ".xml" : ".dat";
            int failures = 0;
            foreach (var input in inputs)
            {
                string target = output == null
                    ? Path.ChangeExtension(input, extension)
                    : outputIsDirectory ? Path.Combine(output, Path.GetFileNameWithoutExtension(input) + extension) : output;
                try
                {
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

        class UsageException : Exception
        {
            public UsageException(string message) : base(message) { }
        }
    }
}
