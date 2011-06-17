// This file is part of PeggleEdit.
// Copyright Ted John 2010 - 2011. http://tedtycoon.co.uk
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

using System.Collections.Generic;
using System.IO;

namespace IntelOrca.PeggleEdit.Tools.Pack.CFG
{
	/// <summary>
	/// Represents a reader for CFGDocuments.
	/// </summary>
	public class CFGReader
	{
		CFGDocument mDocument;

		public CFGReader()
		{
			mDocument = new CFGDocument();
		}

		public CFGReader(string inputText)
			: this()
		{
			//try {
				Stack<CFGBlock> blockStack = new Stack<CFGBlock>();
				blockStack.Push(new CFGBlock());

				StringReader reader = new StringReader(inputText);
				for (; ; ) {
					string line = reader.ReadLine();

					if (line == null)
						break;

					line = ParseLine(line);

					if (line.Length == 0)
						continue;

					int colonIndex = SpecialIndexOf(line, ':');
					if (colonIndex != -1) {
						//Its a property
						blockStack.Peek().Properties.Add(new CFGProperty(line));
					} else if (line.StartsWith("{")) {
						continue;
					} else if (line.StartsWith("}")) {
						CFGBlock block = blockStack.Pop();

						if (blockStack.Count > 0) {
							blockStack.Peek().Blocks.Add(block);
						} else {
							mDocument.Blocks.Add(block);
						}

					} else {
						//Block
						int space = SpecialIndexOf(line, ' ');
						int tab = SpecialIndexOf(line, '\t');

						int firstGap = space;
						if (tab != -1 && tab < space)
							firstGap = tab;

						if (firstGap != -1) {
							CFGBlock block = new CFGBlock();
							block.Name = line.Substring(0, firstGap);
							block.Value = Dequote(line.Substring(firstGap, line.Length - firstGap).Trim());

							blockStack.Push(block);
						} else {
							CFGBlock block = new CFGBlock();
							block.Name = line;

							blockStack.Push(block);
						}
					}
				}

				mDocument.Blocks.Add(blockStack.Pop());

			//} catch (Exception ex) {
			//    System.Diagnostics.StackTrace trace = new System.Diagnostics.StackTrace(ex, true);

			//    MessageBox.Show(trace.GetFrame(0).GetMethod().Name);
			//    MessageBox.Show("Line: " + trace.GetFrame(0).GetFileLineNumber());
			//    MessageBox.Show("Column: " + trace.GetFrame(0).GetFileColumnNumber());
			//}
		}

		public static int SpecialIndexOf(string str, char find)
		{
			return SpecialIndexOf(str, find, 0);
		}

		public static int SpecialIndexOf(string str, char find, int startIndex)
		{
			bool inQuotes = false;
			for (int i = startIndex; i < str.Length; i++) {
				char c = str[i];
				if (c == '"') {
					inQuotes = !inQuotes;
					continue;
				} else if (inQuotes && c == '\\') {
					i++;
					continue;
				} else if (!inQuotes && c == find) {
					return i;
				}
			}

			return -1;
		}

		private string ParseLine(string line)
		{
			int commentStart = -1;
			bool inQuotes = false;
			for (int i = 0; i < line.Length; i++) {
				char c = line[i];
				if (c == '"') {
					inQuotes = !inQuotes;
					continue;
				} else if (inQuotes && c == '\\') {
					i++;
					continue;
				} else if (!inQuotes && c == '/') {
					if (i == line.Length - 1)
						continue;

					if (line[i + 1] != '/')
						continue;

					commentStart = i;
					break;
				}
			}

			if (commentStart != -1) {
				line = line.Remove(commentStart);
			}

			return line.Trim();
		}

		public static string Dequote(string value)
		{
			if (value.StartsWith("\"")) {
				value = value.Remove(0, 1);
				if (value.EndsWith("\"")) {
					value = value.Remove(value.Length - 1, 1);
				}
			}

			return value;
		}

		public CFGDocument Document
		{
			get
			{
				return mDocument;
			}
		}
	}
}
