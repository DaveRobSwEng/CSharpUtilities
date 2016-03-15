using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sepura.Utilities;

namespace Map27Decode
{
    internal class Map27ApiLogDecoder : IMap27PacketDecoder
    {
        public event HandleReceivedData OnRxData;

        public event HandleReceivedData OnTxData;

        public static bool CanDecodeLogFile(string fileName)
        {
            // Read the first line of the file to determine the decoder type
            string firstLine = File.ReadLines(fileName).FirstOrDefault();
            return firstLine.Contains("DEBUG_Initialise");
        }

        public void ReadFile(FileInfo rawPacketFile)
        {
            using (FileStream stream = rawPacketFile.OpenRead())
            using (TextReader reader = new StreamReader(stream))
            {
                double logStartTime = 0;
                string lineIn = reader.ReadLine();
                if (lineIn != null)
                {
                    string logTime = lineIn.Substring(0, s_TimestampLength);
                    logStartTime = ParseTimestamp(logTime);
                }

                while (lineIn != null)
                {
                    // Line looks like:
                    // 15:26:23.612  SER_Rx: [30] 16 10 02 02 19 01 00 10 03 AF F9 16 10 02 04 18
                    if (lineIn.Length >= s_TimestampLength)
                    {

                        string trimmedLine = lineIn.Substring(s_TimestampLength);

                        if (trimmedLine.StartsWith("  SER_Rx:", StringComparison.Ordinal))
                        {
                            Match m = s_RawPacketRegex.Match(trimmedLine);

                            if (m.Success)
                            {
                                byte[] data = Formatting.HexToBin(m.Groups[1].Value);
                                string logTime = lineIn.Substring(0, s_TimestampLength);
                                OnRxData(data, 0, data.Length, logTime, ParseTimestamp(logTime) - logStartTime);
                            }
                        }
                        else if (trimmedLine.StartsWith("  SER_Tx:", StringComparison.Ordinal))
                        {
                            Match m = s_RawPacketRegex.Match(trimmedLine);

                            if (m.Success)
                            {
                                byte[] data = Formatting.HexToBin(m.Groups[1].Value);
                                string logTime = lineIn.Substring(0, s_TimestampLength);
                                OnTxData(data, 0, data.Length, logTime, ParseTimestamp(logTime) - logStartTime);
                            }
                        }
                    }

                    lineIn = reader.ReadLine();
                }
            }
        }

        private const int s_TimestampLength = 12;

        private double ParseTimestamp(string value)
        {
            // Timestamp looks like:
            // 15:27:03.181
            return TimeSpan.Parse(value).TotalSeconds;
        }

        private static Regex s_RawPacketRegex = new Regex(
              @".*\[\d+\]([0-9A-F\s]+)",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );

    }
}
