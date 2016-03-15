// ---------------------------------------------------------------------------
// Copyright Sepura Plc
// All Rights reserved. Reproduction in whole or part is prohibited without 
// written consent of the copyright owner.
//
// Formatting.cs
//  Implementation of the Class Formatting
//
//  Original author: RobinsonD
//
// $Id:$ 
// ---------------------------------------------------------------------------

namespace Sepura.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// Utility class containing various helper functions for converting between binary and hex formats
    /// and converting between arrays and strings
    /// </summary>
    public static class Formatting
    {
        /// <summary>
        /// Convert an array of bytes to a string of hex chars
        /// </summary>
        /// <param name="binaryData"></param>
        /// <returns>String of hex characters representing the binary data</returns>
        public static string BinToHex(byte[] binaryData)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in binaryData)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert a collection of bytes to a string of hex chars
        /// </summary>
        /// <param name="binaryData"></param>
        /// <returns>String of hex characters representing the binary data</returns>
        public static string BinToHex(ReadOnlyCollection<byte> binaryData)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in binaryData)
            {
                sb.AppendFormat("{0:x2}", b);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert an array of bytes to a string of hex chars
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>
        /// String of hex characters representing the binary data
        /// </returns>
        public static string BinToHex(byte[] binaryData, char separator)
        {
            StringBuilder sb = new StringBuilder(binaryData.Length * 4);
            bool addSeparator = false;
            for (int i = 0; i < binaryData.Length; ++i)
            {
                if (addSeparator)
                {
                    sb.Append(separator);
                }

                sb.Append(s_BinToHexLower[binaryData[i]]);
                addSeparator = true;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Convert a collection of bytes to a string of hex chars
        /// </summary>
        /// <param name="binaryData">The binary data.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>
        /// String of hex characters representing the binary data
        /// </returns>
        public static string BinToHex(ReadOnlyCollection<byte> binaryData, char separator)
        {
            StringBuilder sb = new StringBuilder();
            bool addSeparator = false;
            for (int i = 0; i < binaryData.Count; ++i)
            {
                if (addSeparator)
                {
                    sb.Append(separator);
                }

                sb.Append(s_BinToHexLower[binaryData[i]]);
                addSeparator = true;
            }

            return sb.ToString();
        }

        /// <summary>
        /// The bin to lower-case hexadecimal conversion array
        /// </summary>
        private static string[] s_BinToHexLower;

        /// <summary>
        /// Initializes static members of the <see cref="Formatting"/> class.
        /// </summary>
        static Formatting()
        {
            s_BinToHexLower = new string[256];
            for (int i = 0; i < 256; ++i)
            {
                s_BinToHexLower[i] = string.Format("{0:x2}", i);
            }
        }

        /// <summary>
        /// Convert a string of hex chars to an array of bytes
        /// </summary>
        /// <param name="hexstring"></param>
        /// <returns>Array of bytes converted from the hex string</returns>
        public static byte[] HexToBin(string hexstring)
        {
            hexstring = hexstring.Trim();

            List<byte> bytes = new List<byte>();

            byte inByte = 0;
            bool readingLowNibble = false;
            foreach (var c in hexstring)
            {
                if (c == ' ')
                {
                    continue;
                }

                byte nibble = CharToNibble(c);
                if (readingLowNibble)
                {
                    inByte += nibble;
                    bytes.Add(inByte);
                }
                else
                {
                    inByte = (byte)(nibble << 4);
                }

                readingLowNibble = !readingLowNibble;
            }

            return bytes.ToArray();
        }

        /// <summary>
        /// Convert an ASCII-encoded byte array to a string
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns>String containing the raw data interpreted as ASCII characters</returns>
        public static string ByteArrayToString(byte[] rawData)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetString(rawData);
        }

        /// <summary>
        /// Convert a string to an ASCII-encoded byte array
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Array of bytes</returns>
        public static byte[] StringToByteArray(string text)
        {
            System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            return encoding.GetBytes(text);
        }

        /// <summary>
        /// Tries to parse an integer value from the string. Accept either decimal or hex (with 0 prefix)
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <param name="intValue">The integer value.</param>
        /// <returns>
        /// True if the text contains an integer value
        /// </returns>
        public static bool TryGetInt(string text, out int intValue)
        {
            bool parsed = false;
            if (text.ToLower().StartsWith("0x", StringComparison.Ordinal))
            {
                parsed |= int.TryParse(text.Substring(2), NumberStyles.AllowHexSpecifier, null, out intValue);
            }
            else
            {
                parsed |= int.TryParse(text, out intValue);
            }

            return parsed;
        }

        /// <summary>
        /// Tries to parse an integer value from the string. Accept either decimal or hex (with 0 prefix)
        /// </summary>
        /// <param name="text">The text to parse</param>
        /// <returns>
        /// The integer value parsed from the text
        /// </returns>
        public static int ParseInt(string text)
        {
            if (text.ToLower().StartsWith("0x", StringComparison.Ordinal))
            {
                return int.Parse(text.Substring(2), NumberStyles.AllowHexSpecifier, null);
            }
            else
            {
                return int.Parse(text, null);
            }
        }

        /// <summary>
        /// Converts a hex character to a nibble value (4 bits)
        /// </summary>
        /// <param name="c">The character to convert</param>
        /// <returns>Nibble value (4 bits)</returns>
        /// <exception cref="System.InvalidOperationException">Throws if an invalid character is supplied</exception>
        private static byte CharToNibble(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return (byte)(c - '0');
            }
            else if (c >= 'a' && c <= 'f')
            {
                return (byte)(c - 'a' + 10);
            }
            else if (c >= 'A' && c <= 'F')
            {
                return (byte)(c - 'A' + 10);
            }
            else
            {
                throw new InvalidOperationException(string.Format("Illegal hex char {0}", c));
            }
        }
    }
}