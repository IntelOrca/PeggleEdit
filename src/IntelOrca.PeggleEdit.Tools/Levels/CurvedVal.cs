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
using System.IO;
using System.Text;

namespace IntelOrca.PeggleEdit.Tools.Levels
{
    /// <summary>
    /// Represents a variable float which can be a static value or a scripted variable value.
    /// </summary>
    [TypeConverterAttribute(typeof(CurvedValTypeConverter))]
    public class CurvedVal : ICloneable
    {
        bool mIsVariable;
        float mStaticValue;
        string mVariableValue;

        public CurvedVal()
        {
            mIsVariable = false;
            mStaticValue = 0.0f;
        }

        public CurvedVal(string variableValue)
        {
            if (float.TryParse(variableValue, out var f))
            {
                mIsVariable = false;
                mStaticValue = f;
                mVariableValue = default;
            }
            else
            {
                mIsVariable = true;
                mStaticValue = default;
                mVariableValue = variableValue;
            }
        }

        public CurvedVal(float staticValue)
        {
            mIsVariable = false;
            mStaticValue = staticValue;
            mVariableValue = default;
        }

        public CurvedVal(BinaryReader br)
        {
            ReadData(br);
        }

        public void ReadData(BinaryReader br)
        {
            byte var1 = br.ReadByte();
            if (var1 > 0)
            {
                mIsVariable = false;
                mStaticValue = br.ReadSingle();
            }
            else
            {
                mIsVariable = true;
                mVariableValue = ReadString(br);
            }
        }

        public void WriteData(BinaryWriter bw)
        {
            if (mIsVariable)
            {
                bw.Write((byte)0);
                WriteString(bw, mVariableValue);
            }
            else
            {
                bw.Write((byte)1);
                bw.Write(mStaticValue);
            }
        }

        private string ReadString(BinaryReader br)
        {
            short l = br.ReadInt16();
            return Encoding.UTF8.GetString(br.ReadBytes(l));
        }

        private void WriteString(BinaryWriter bw, string s)
        {
            if (String.IsNullOrEmpty(s))
            {
                bw.Write((short)0);
            }
            else
            {
                bw.Write((short)s.Length);
                bw.Write(Encoding.UTF8.GetBytes(s));
            }
        }

        public override string ToString()
        {
            if (mIsVariable)
                return mVariableValue;
            else
                return mStaticValue.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode() ^ 23;
        }

        public override bool Equals(object obj)
        {
            CurvedVal vf = obj as CurvedVal;
            if (vf == null)
                return false;

            if (vf.mIsVariable != mIsVariable)
                return false;

            if (mIsVariable)
            {
                return (vf.mVariableValue == mVariableValue);
            }
            else
            {
                return (vf.mStaticValue == mStaticValue);
            }
        }

        public static implicit operator CurvedVal(float f)
        {
            return new CurvedVal(f);
        }

        public static implicit operator CurvedVal(string s)
        {
            return new CurvedVal(s);
        }

        public object Clone()
        {
            CurvedVal vf = new CurvedVal();
            vf.mIsVariable = mIsVariable;
            vf.mStaticValue = mStaticValue;
            vf.mVariableValue = mVariableValue;

            return vf;
        }

        public bool IsVariable
        {
            get
            {
                return mIsVariable;
            }
            set
            {
                mIsVariable = value;
            }
        }

        public float StaticValue
        {
            get
            {
                return mStaticValue;
            }
            set
            {
                mStaticValue = value;
            }
        }

        public string VariableValue
        {
            get
            {
                return mVariableValue;
            }
            set
            {
                mVariableValue = value;
            }
        }
    }
}
