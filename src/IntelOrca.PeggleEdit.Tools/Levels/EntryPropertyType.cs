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
using System.ComponentModel;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
	/// <summary>
	/// Specifies the type of property for the entry.
	/// </summary>
	enum EntryPropertyType
	{
		Attribute,
		Element,
		SubElement,
	}

	/// <summary>
	/// Specifies an entry property type and a default value for an entry property.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	class EntryPropertyAttribute : Attribute
	{
		EntryPropertyType mType;
		object mDefaultValue;

		public EntryPropertyAttribute(EntryPropertyType type, object defaultValue)
		{
			mType = type;
			mDefaultValue = defaultValue;
		}

		public EntryPropertyAttribute(EntryPropertyType type, Type valueType, string value)
		{
			mDefaultValue = TypeDescriptor.GetConverter(valueType).ConvertFromInvariantString(value);
		}

		public EntryPropertyType Type
		{
			get
			{
				return mType;
			}
		}

		public object DefaultValue
		{
			get
			{
				return mDefaultValue;
			}
		}
	}
}
