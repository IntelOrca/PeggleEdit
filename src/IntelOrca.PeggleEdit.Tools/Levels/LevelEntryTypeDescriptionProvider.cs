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
using System.Linq;
using IntelOrca.PeggleEdit.Tools.Levels.Children;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Shows the properties of an object's peg (Variable, Crumble and its type) in the property grid with the object's
    /// own, rather than under a row of their own.
    /// </summary>
    public sealed class LevelEntryTypeDescriptionProvider : TypeDescriptionProvider
    {
        private static readonly TypeDescriptionProvider Default = TypeDescriptor.GetProvider(typeof(object));

        public LevelEntryTypeDescriptionProvider()
            : base(Default)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new Descriptor(base.GetTypeDescriptor(objectType, instance), instance as LevelEntry);
        }

        private sealed class Descriptor : CustomTypeDescriptor
        {
            private readonly LevelEntry _entry;

            public Descriptor(ICustomTypeDescriptor parent, LevelEntry entry)
                : base(parent)
            {
                _entry = entry;
            }

            public override PropertyDescriptorCollection GetProperties() => AddPegProperties(base.GetProperties(), null);

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes) =>
                AddPegProperties(base.GetProperties(attributes), attributes);

            private PropertyDescriptorCollection AddPegProperties(PropertyDescriptorCollection properties, Attribute[] attributes)
            {
                if (_entry?.PegInfo == null)
                    return properties;

                var pegProperties = attributes == null
                    ? TypeDescriptor.GetProperties(typeof(PegInfo))
                    : TypeDescriptor.GetProperties(typeof(PegInfo), attributes);
                return new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>()
                    .Concat(pegProperties.Cast<PropertyDescriptor>().Where(x => x.IsBrowsable).Select(x => new PegPropertyDescriptor(x)))
                    .ToArray());
            }
        }

        /// <summary>
        /// A property of an object's peg, as a property of the object.
        /// </summary>
        private sealed class PegPropertyDescriptor : PropertyDescriptor
        {
            private readonly PropertyDescriptor _pegProperty;

            public PegPropertyDescriptor(PropertyDescriptor pegProperty)
                // Named as shown, as the grid sorts several objects' properties by name, but apart from the object's
                // own properties (e.g. a brick's Type)
                : base(pegProperty.Name == nameof(PegInfo.Type) ? "PegType" : pegProperty.Name, pegProperty.Attributes.Cast<Attribute>().ToArray())
            {
                _pegProperty = pegProperty;
            }

            public override string DisplayName => _pegProperty.Name == nameof(PegInfo.Type) ? "Peg Type" : _pegProperty.DisplayName;

            public override Type ComponentType => typeof(LevelEntry);

            public override bool IsReadOnly => _pegProperty.IsReadOnly;

            public override Type PropertyType => _pegProperty.PropertyType;

            public override TypeConverter Converter => _pegProperty.Converter;

            public override object GetEditor(Type editorBaseType) => _pegProperty.GetEditor(editorBaseType);

            private static PegInfo GetPeg(object component) => (component as LevelEntry)?.PegInfo;

            public override object GetValue(object component)
            {
                var peg = GetPeg(component);
                return peg == null ? null : _pegProperty.GetValue(peg);
            }

            public override void SetValue(object component, object value)
            {
                var peg = GetPeg(component);
                if (peg == null)
                    return;
                _pegProperty.SetValue(peg, value);
                OnValueChanged(component, EventArgs.Empty);
            }

            public override bool CanResetValue(object component)
            {
                var peg = GetPeg(component);
                return peg != null && _pegProperty.CanResetValue(peg);
            }

            public override void ResetValue(object component)
            {
                var peg = GetPeg(component);
                if (peg != null)
                    _pegProperty.ResetValue(peg);
            }

            public override bool ShouldSerializeValue(object component)
            {
                var peg = GetPeg(component);
                return peg != null && _pegProperty.ShouldSerializeValue(peg);
            }
        }
    }
}
