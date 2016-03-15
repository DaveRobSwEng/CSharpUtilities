using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Map27Decode
{
    public delegate void HandleReceivedData(byte[] data, int startPos, int length, string logTime, double timestamp);

    internal interface IMap27PacketDecoder
    {
        event HandleReceivedData OnTxData;

        event HandleReceivedData OnRxData;

        void ReadFile(FileInfo rawPacketFile);
    }
}
