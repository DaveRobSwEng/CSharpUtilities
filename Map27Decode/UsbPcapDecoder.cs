using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Map27Decode
{
    public class UsbPcapDecoder : IMap27PacketDecoder
    {
        #region IMap27PacketDecoder Members

        public event HandleReceivedData OnTxData;

        public event HandleReceivedData OnRxData;

        public static bool CanDecodeLogFile(string fileName)
        {
            uint magicNumber = ReadMagicNumber(fileName);
            return magicNumber == MagicNumberBE || magicNumber == MagicNumberLE;
        }

        private static uint ReadMagicNumber(string fileName)
        {
            uint magicNumber = 0;

            // Read the first 4 bytes of the file to determine the decoder type
            using (var stream = File.OpenRead(fileName))
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    var magicNumberRaw = binaryReader.ReadBytes(4);
                    magicNumber = magicNumberRaw[0] +
                        (uint)(magicNumberRaw[1] << 8) +
                        (uint)(magicNumberRaw[2] << 16) +
                        (uint)(magicNumberRaw[3] << 24);
                }
            }
            return magicNumber;
        }

        public void ReadFile(FileInfo pcapFile)
        {
            using (var stream = pcapFile.OpenRead())
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    // read the magic number and determine endianness of the file
                    uint magicNumber = GetUint32BE(binaryReader);

                    m_IsBE = magicNumber == MagicNumberBE;

                    m_Read32 = m_IsBE ? new Func<BinaryReader, uint>((BinaryReader r) => GetUint32BE(r)) : new Func<BinaryReader, uint>((BinaryReader r) => GetUint32LE(r));
                    m_Read16 = m_IsBE ? new Func<BinaryReader, uint>((BinaryReader r) => GetUint16BE(r)) : new Func<BinaryReader, uint>((BinaryReader r) => GetUint16LE(r));

                    uint major = m_Read16(binaryReader);
                    uint minor = m_Read16(binaryReader);
                    Console.WriteLine("PCAP Version {0}.{1}", major, minor);
                    Console.WriteLine("Timezone offset {0}", m_Read32(binaryReader));
                    Console.WriteLine("Timestamp Accuracy {0}", m_Read32(binaryReader));

                    uint maxPacketLength = m_Read32(binaryReader);
                    Console.WriteLine("Max Packet Length offset {0}", maxPacketLength);
                    PcapLinkLayerHeaderType linkLayerHeaderType = (PcapLinkLayerHeaderType)m_Read32(binaryReader);
                    Console.WriteLine("Link Layer Header Type {0}", linkLayerHeaderType);

                    if (linkLayerHeaderType != PcapLinkLayerHeaderType.LINKTYPE_USBPCAP)
                    {
                        throw new Exception(string.Format("Link layer header type is {0} - must be {1}", linkLayerHeaderType, PcapLinkLayerHeaderType.LINKTYPE_USBPCAP));
                    }

                    double? startTime = null;

                    // Require min 16 bytes for each packet
                    while (stream.Length - stream.Position >= 16)
                    {
                        uint timestampSeconds = m_Read32(binaryReader);
                        uint timestampMicroseconds = m_Read32(binaryReader);

                        if (!startTime.HasValue)
                        {
                            startTime = (double)timestampSeconds + ((double)timestampMicroseconds / 1e6);
                        }

                        uint storedPacketLength = m_Read32(binaryReader);
                        uint capturedPacketLength = m_Read32(binaryReader);

                        if (storedPacketLength != capturedPacketLength)
                        {
                            Console.WriteLine("Truncated packet stored in file: Captured {0}, stored {1}", capturedPacketLength, storedPacketLength);
                        }

                        ProcessUsbPcapPacket(binaryReader, startTime.Value, timestampSeconds, timestampMicroseconds);
                    }
                }
            }
        }

        /// <summary>
        /// Processes the usb pcap packet.
        /// See http://desowin.org/usbpcap/captureformat.html for packet format
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="timestampSeconds">The timestamp seconds since Unix epoch start</param>
        /// <param name="timestampMicroseconds">The timestamp microseconds.</param>
        private void ProcessUsbPcapPacket(BinaryReader reader, double startTime, uint timestampSeconds, uint timestampMicroseconds)
        {
            uint headerLen = m_Read16(reader);
            ulong irpId;
            if (m_IsBE)
            {
                ulong msWord = m_Read32(reader);
                ulong lsWord = m_Read32(reader);
                irpId = (msWord << 32) + lsWord;
            }
            else
            {
                ulong lsWord = m_Read32(reader);
                ulong msWord = m_Read32(reader);
                irpId = (msWord << 32) + lsWord;
            }

            uint status = m_Read32(reader);
            uint function = m_Read16(reader);
            byte info = reader.ReadByte();
            uint bus = m_Read16(reader);
            uint device = m_Read16(reader);
            byte endpoint = reader.ReadByte();
            UsbPcapTranferType transfer = (UsbPcapTranferType)reader.ReadByte();

            uint datalength = m_Read32(reader);

            // Only interested in BULK transfer
            if (transfer == UsbPcapTranferType.USBPCAP_TRANSFER_BULK)
            {
                double timestampTime = (double)timestampSeconds + ((double)timestampMicroseconds / 1e6);

                byte[] rawData = reader.ReadBytes((int)datalength);

                DateTime timestampDateTime = s_UnixEpochStart + TimeSpan.FromSeconds(timestampTime);

                if (((UsbPcapEndpointDirection)endpoint & UsbPcapEndpointDirection.IN) == UsbPcapEndpointDirection.IN)
                {
                    OnRxData(rawData, 0, (int)datalength, timestampDateTime.ToString("HH:mm:ss:fff"), timestampTime - startTime);
                }
                else
                {
                    OnTxData(rawData, 0, (int)datalength, timestampDateTime.ToString("HH:mm:ss:fff"), timestampTime - startTime);
                }
            }
        }

        private static uint GetUint32LE(byte[] magicNumberRaw)
        {
            uint magicNumber = magicNumberRaw[0] +
                (uint)(magicNumberRaw[1] << 8) +
                (uint)(magicNumberRaw[2] << 16) +
                (uint)(magicNumberRaw[3] << 24);
            return magicNumber;
        }

        private static uint GetUint32LE(BinaryReader reader)
        {
            return GetUint32LE(reader.ReadBytes(4));
        }

        private static uint GetUint32BE(byte[] magicNumberRaw)
        {
            uint magicNumber = magicNumberRaw[3] +
                (uint)(magicNumberRaw[2] << 8) +
                (uint)(magicNumberRaw[1] << 16) +
                (uint)(magicNumberRaw[0] << 24);
            return magicNumber;
        }

        private static uint GetUint32BE(BinaryReader reader)
        {
            return GetUint32BE(reader.ReadBytes(4));
        }

        private static uint GetUint16LE(BinaryReader reader)
        {
            return GetUint16LE(reader.ReadBytes(2));
        }

        private static uint GetUint16LE(byte[] magicNumberRaw)
        {
            uint magicNumber = magicNumberRaw[0] +
                (uint)(magicNumberRaw[1] << 8);
            return magicNumber;
        }

        private static uint GetUint16BE(BinaryReader reader)
        {
            return GetUint16BE(reader.ReadBytes(2));
        }

        private static uint GetUint16BE(byte[] magicNumberRaw)
        {
            uint magicNumber = magicNumberRaw[1] +
                (uint)(magicNumberRaw[0] << 8);
            return magicNumber;
        }

        private bool m_IsBE;

        private const uint MagicNumberBE = 0xa1b2c3d4;

        private const uint MagicNumberLE = 0xd4c3b2a1;

        private Func<BinaryReader, uint> m_Read32;

        private Func<BinaryReader, uint> m_Read16;

        private static readonly DateTime s_UnixEpochStart = new DateTime(1970, 1, 1);

        #endregion
    }

    /// <summary>
    /// http://www.tcpdump.org/linktypes.html
    /// </summary>
    public enum PcapLinkLayerHeaderType
    {
        LINKTYPE_USBPCAP = 249
    }

    /// <summary>
    /// See http://desowin.org/usbpcap/captureformat.html
    /// </summary>
    public enum UsbPcapTranferType
    {
        USBPCAP_TRANSFER_ISOCHRONOUS = 0,
        USBPCAP_TRANSFER_INTERRUPT = 1,
        USBPCAP_TRANSFER_CONTROL = 2,
        USBPCAP_TRANSFER_BULK = 3
    }

    [Flags]
    public enum UsbPcapEndpointDirection
    {
        IN = 0x80,
        OUT = 0
    }
}
