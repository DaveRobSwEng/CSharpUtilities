using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sepura.DataDictionary;

namespace Map27Decode
{
    public static class Extensions
    {
        public static ulong LessOneMod256(this ulong i)
        {
            return i == 0 ? 255 : i - 1;
        }

        public static ulong PlusOneMod256(this ulong i)
        {
            return (i + 1) % 256;
        }

        public static ulong GetUlongValue(this Value theValue)
        {
            ulong integerValue = 0;
            BaseTypeValue theBaseTypeValue = theValue as BaseTypeValue;
            if (theBaseTypeValue != null)
            {
                integerValue = theBaseTypeValue.IsSigned ? (ulong) theBaseTypeValue.SignedValue : theBaseTypeValue.UnsignedValue;
            }
            else
            {
                throw new InvalidDataException(string.Format("Unsupported type {0}", theValue.GetType()));
            }

            return integerValue;
        }

    }
}
