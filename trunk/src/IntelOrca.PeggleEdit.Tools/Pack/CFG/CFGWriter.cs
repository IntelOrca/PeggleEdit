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

using System;
using System.Reflection;
using System.Text;

namespace IntelOrca.PeggleEdit.Tools.Pack.CFG
{
	/// <summary>
	/// Represents a writer for CFGDocuments.
	/// </summary>
	public class CFGWriter
	{
		CFGDocument mDocument;

		public CFGWriter(CFGDocument document)
		{
			mDocument = document;
		}

		public string GetText()
		{
			StringBuilder sb = new StringBuilder();

			WriteApplicationSigniture(sb);

			if (mDocument.Blocks.Count > 0)
				WriteBlock(sb, mDocument.Blocks[0], 0, true);

			return sb.ToString();
		}

		private void WriteApplicationSigniture(StringBuilder sb)
		{
			sb.AppendLine("//Created by " + Assembly.GetExecutingAssembly().GetName().Name);
			sb.AppendLine("//At " + DateTime.Now.ToString());
			sb.AppendLine("//http://tedtycoon.co.uk for more information.");
			sb.AppendLine();
		}

		private void WriteBlock(StringBuilder sb, CFGBlock block, int level, bool noCasing = false)
		{
			if (!noCasing) {
				if (block.HasValue)
					sb.AppendLine(String.Format("{0}{1} \"{2}\"", GetTabSpacing(level), block.Name, GetValue(block.Value)));
				else
					sb.AppendLine(String.Format("{0}{1}", new String('\t', level), block.Name));

				sb.AppendLine(String.Format("{0}{{", GetTabSpacing(level)));
			}

			int newLevel = level;
			if (!noCasing)
				newLevel++;

			foreach (CFGBlock b in block.Blocks) {
				WriteBlock(sb, b, newLevel);
				sb.AppendLine();
			}

			foreach (CFGProperty property in block)
				WriteProperty(sb, property, newLevel);

			if (!noCasing) {
				sb.AppendLine(String.Format("{0}}}", GetTabSpacing(level)));
			}
		}

		private void WriteProperty(StringBuilder sb, CFGProperty property, int level)
		{
			sb.AppendLine(String.Format("{0}{1}: {2}", GetTabSpacing(level), property.Name, GetValue(property.Values.ToArray())));
		}

		private string GetValue(string value)
		{
			if (value.Contains(" ") || value.Contains(","))
				return String.Format("\"{0}\"", value);
			else
				return value;
		}

		private string GetValue(string[] values)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < values.Length; i++) {
				sb.Append(GetValue(values[i]));

				if (i != values.Length - 1)
					sb.Append(",");
			}

			return sb.ToString();
		}

		private string GetTabSpacing(int level)
		{
			return new String('\t', level);
		}
	}
}
