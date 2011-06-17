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

using System.Reflection;
using System.Text;
using System.Xml;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Represents a writer that can write levels in XML form.
	/// </summary>
	class LevelXMLWriter
	{
		Level mLevel;

		public LevelXMLWriter(Level level)
		{
			mLevel = level;
		}

		public string GetXML()
		{
			StringBuilder sb = new StringBuilder();
			XmlWriter xmlWriter = XmlWriter.Create(sb, GetXMLSettings());

			xmlWriter.WriteStartDocument();

			WriteLevel(xmlWriter);

			xmlWriter.WriteEndDocument();

			xmlWriter.Close();

			return sb.ToString();
		}

		private XmlWriterSettings GetXMLSettings()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = ("\t");
			return settings;
		}

		private void WriteLevel(XmlWriter xmlWriter)
		{
			xmlWriter.WriteStartElement("Level");
			xmlWriter.WriteAttributeString("name", mLevel.Info.Name);

			//Write the entries
			foreach (LevelEntry le in mLevel.Entries)
				WriteEntry(xmlWriter, le);

			xmlWriter.WriteEndElement();
		}

		private void WriteEntry(XmlWriter xmlWriter, LevelEntry entry)
		{
			xmlWriter.WriteStartElement("Entry");

			xmlWriter.WriteAttributeString("type", entry.Type.ToString());

			WriteLevelChild(xmlWriter, entry);


			//Write properties

			xmlWriter.WriteEndElement();
		}

		private void WriteLevelChild(XmlWriter xmlWriter, object child)
		{
			WriteObjectProperties(xmlWriter, EntryPropertyType.Attribute, child);
			WriteObjectProperties(xmlWriter, EntryPropertyType.Element, child);
		}

		private void WriteObjectProperties(XmlWriter xmlWriter, EntryPropertyType type, object obj)
		{
			PropertyInfo[] properties = obj.GetType().GetProperties();
			foreach (PropertyInfo p in properties) {
				//Check to see if the property has an entry property attribute
				object[] attributes = p.GetCustomAttributes(typeof(EntryPropertyAttribute), true);
				if (attributes.Length == 0)
					continue;

				//Check to see if the property is an attribute
				EntryPropertyAttribute ep = (EntryPropertyAttribute)attributes[0];
				if (type == EntryPropertyType.Attribute) {
					if (ep.Type != type)
						continue;
				} else if (type == EntryPropertyType.Element) {
					if (ep.Type != EntryPropertyType.Element && ep.Type != EntryPropertyType.SubElement)
						continue;
				}

				//Get value
				object value = p.GetValue(obj, null);
				if (value == null)
					continue;

				//Check to see if the value is different from the default value
				if (ep.DefaultValue != null) {
					if (ep.DefaultValue.Equals(value))
						continue;
				}

				switch (ep.Type) {
					case EntryPropertyType.Attribute:
						xmlWriter.WriteAttributeString(p.Name, value.ToString());
						break;
					case EntryPropertyType.Element:
						xmlWriter.WriteElementString(p.Name, value.ToString());
						break;
					case EntryPropertyType.SubElement:
						xmlWriter.WriteStartElement(p.Name);
						WriteLevelChild(xmlWriter, value);
						xmlWriter.WriteEndElement();
						break;
				}
			}
		}
	}
}
