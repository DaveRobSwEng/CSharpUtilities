using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sepura.Utilities;

namespace Map27Decode
{
    internal class EllisysRawPacketDecoder : IMap27PacketDecoder
    {
        public event HandleReceivedData OnTxData;

        public event HandleReceivedData OnRxData;

        public static bool CanDecodeLogFile(string fileName)
        {
            // Read the first line of the file to determine the decoder type
            string firstLine = File.ReadLines(fileName).FirstOrDefault();
            return firstLine.StartsWith("Ellisys USB Packets file", StringComparison.Ordinal);
        }

        public void ReadFile(FileInfo rawPacketFile)
        {
            m_ProcessPacket = ProcessPacketInIdle;

            string lineHeader = "RawPacket data<";
            int lineHeaderLength = lineHeader.Length;

            using (FileStream stream = rawPacketFile.OpenRead())
            using (TextReader reader = new StreamReader(stream))
            {
                string lineIn = reader.ReadLine();

                // Skip the first few header lines
                while (lineIn != null && !lineIn.StartsWith(lineHeader, StringComparison.Ordinal))
                {
                    lineIn = reader.ReadLine();
                }

                while (lineIn != null)
                {
                    // Line looks like:
                    // RawPacket data<4B 16 10 02 01 10 10 01 82 10 03 D3 E3 2B EB> speed<FS> time<0.317 615 479>
                    Match m = s_RawPacketRegex.Match(lineIn);

                    if (m.Success)
                    {
                        double timestamp = ParseTimestamp(m.Groups[2].Value);
                        byte[] data = Formatting.HexToBin(m.Groups[1].Value);
                        m_ProcessPacket(data, m.Groups[2].Value, timestamp);
                    }

                    lineIn = reader.ReadLine();
                }
            }
        }

        private double ParseTimestamp(string value)
        {
            // Timestamp looks like:
            // 0.317 615 479

            int pointPos = value.IndexOf('.');
            string spacesRemoved = string.Join("", new string[] {
                value.Substring (0, pointPos + 4),
                value.Substring (pointPos + 5, 3),
                value.Substring (pointPos + 9, 3)});

            return double.Parse(spacesRemoved);
        }

        private void ProcessPacketInIdle(byte[] data, string logTime, double timestamp)
        {
            switch ((UsbPacketId)data[0])
            {
                case UsbPacketId.Out:
                    m_ProcessPacket = ProcessPacketInReceivedOut;
                    break;
                case UsbPacketId.In:
                    m_ProcessPacket = ProcessPacketInReceivedIn;
                    break;
                case UsbPacketId.Data0:
                case UsbPacketId.Data1:
                case UsbPacketId.Ack:
                default:
                    // Not expected in this state - reset
                    Debug.WriteLine("Error - received signal {0} 0x{0:x} in state Idle", (UsbPacketId)data[0]);
                    m_ProcessPacket = ProcessPacketInIdle;
                    break;
            }
        }

        private void ProcessPacketInReceivedIn(byte[] data, string logTime, double timestamp)
        {
            switch ((UsbPacketId)data[0])
            {
                case UsbPacketId.Data0:
                case UsbPacketId.Data1:
                    // Treat Data0 and Data1 the same because we're ignoring the 0-1 flip-flop
                    ReceiveInData(data, logTime, timestamp);
                    m_ProcessPacket = ProcessPacketInReceivedData_0_1;
                    break;
                case UsbPacketId.Ack:
                case UsbPacketId.Out:
                case UsbPacketId.In:
                default:
                    // Not expected in this state - reset
                    Debug.WriteLine("Error - received signal {0} 0x{0:x} in state ReceivedIn", (UsbPacketId)data[0]);
                    m_ProcessPacket = ProcessPacketInIdle;
                    break;
            }
        }

        private void ProcessPacketInReceivedOut(byte[] data, string logTime, double timestamp)
        {
            switch ((UsbPacketId)data[0])
            {
                case UsbPacketId.Data0:
                case UsbPacketId.Data1:
                    // Treat Data0 and Data1 the same because we're ignoring the 0-1 flip-flop
                    ReceiveOutData(data, logTime, timestamp);
                    m_ProcessPacket = ProcessPacketInReceivedData_0_1;
                    break;
                case UsbPacketId.Ack:
                case UsbPacketId.Out:
                case UsbPacketId.In:
                default:
                    // Not expected in this state - reset
                    Debug.WriteLine("Error - received signal {0} 0x{0:x} in state ReceivedOut", (UsbPacketId)data[0]);
                    m_ProcessPacket = ProcessPacketInIdle;
                    break;
            }
        }

        private void ProcessPacketInReceivedData_0_1(byte[] data, string logTime, double timestamp)
        {
            switch ((UsbPacketId)data[0])
            {
                case UsbPacketId.Ack:
                    m_ProcessPacket = ProcessPacketInIdle;
                    break;
                case UsbPacketId.In:
                case UsbPacketId.Out:
                case UsbPacketId.Data0:
                case UsbPacketId.Data1:
                default:
                    // Not expected in this state - reset
                    Debug.WriteLine("Error - received signal {0} 0x{0:x} in state ReceivedData_0_1", (UsbPacketId)data[0]);
                    m_ProcessPacket = ProcessPacketInIdle;
                    break;
            }
        }

        private void ReceiveInData(byte[] data, string logTime, double timestamp)
        {
            // Trim the first byte and last 2 bytes. The first byte is the USB packet type and the last 2 are the USB CRC
            OnRxData(data, 1, data.Length - 3, logTime, timestamp);
        }

        private void ReceiveOutData(byte[] data, string logTime, double timestamp)
        {
            // Trim the first byte and last 2 bytes. The first byte is the USB packet type and the last 2 are the USB CRC
            OnTxData(data, 1, data.Length - 3, logTime, timestamp);
        }

        /// <summary>
        ///  Regular expression built for C# on: Wed, Jul 30, 2014, 09:23:31 AM
        ///  Using Expresso Version: 3.0.2766, http://www.ultrapico.com
        ///  
        ///  A description of the regular expression:
        ///  
        ///  ^.*\<
        ///      Beginning of line or string
        ///      Any character, any number of repetitions
        ///      Literal <
        ///  [1]: A numbered capture group. [[0-9A-F\s]+]
        ///      Any character in this class: [0-9A-F\s], one or more repetitions
        ///  \>\sspeed\<FS\>\s*time\<
        ///      Literal >
        ///      Whitespace
        ///      speed
        ///      Literal <
        ///      FS
        ///      Literal >
        ///      Whitespace, any number of repetitions
        ///      time
        ///      Literal <
        ///  [2]: A numbered capture group. [[0-9]+\.[0-9 ]+]
        ///      [0-9]+\.[0-9 ]+
        ///          Any character in this class: [0-9], one or more repetitions
        ///          Literal .
        ///          Any character in this class: [0-9 ], one or more repetitions
        ///  
        ///
        /// </summary>
        private static Regex s_RawPacketRegex = new Regex(
              "^.*\\<([0-9A-F\\s]+)\\>\\sspeed\\<FS\\>\\s*time\\<([0-9]+\\." +
              "[0-9 ]+)",
            RegexOptions.IgnoreCase
            | RegexOptions.CultureInvariant
            | RegexOptions.IgnorePatternWhitespace
            | RegexOptions.Compiled
            );


        private delegate void ProcessPacket(byte[] data, string logTime, double timestamp);

        private ProcessPacket m_ProcessPacket;

        private enum UsbPacketId
        {
            Out = 0xe1,
            In = 0x69,
            Data0 = 0xc3,
            Data1 = 0x4b,
            Ack = 0xd2,
            Nak = 0x5a,
            Split = 0x78,
            Setup = 0x2d
        }
    }
}
