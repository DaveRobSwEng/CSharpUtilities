using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Map27Decode
{
    class TapStreamDecoder
    {
        #region Public Enums

        public enum ProcessingResult
        {
            Ok,
            FcsError,
            FormatError,
            OutOfFrameBytesFound
        }

        #endregion Public Enums

        #region Public Delegates

        public delegate void ProcessingComplete(ProcessingResult result);

        #endregion Public Delegates

        #region Public Events

        public event ProcessingComplete OnProcessingComplete;

        #endregion Public Events

        #region Public Properties

        public ReadOnlyCollection<byte> Map27Packet
        {
            get { return new ReadOnlyCollection<byte>(m_Map27Packet); }
        }

        public ReadOnlyCollection<byte> OutOfFrameBytes
        {
            get { return new ReadOnlyCollection<byte>(m_OutOfFrameBytes); }
        }

        public double MessageEndTime
        {
            get { return m_MessageEndTime; }
        }

        public string MessageEndTimeText
        {
            get { return m_MessageEndTimeText; }
        }

        public double MessageStartTime
        {
            get { return m_MessageStartTime; }
        }
        public string MessageStartTimeText
        {
            get { return m_MessageStartTimeText; }
        }

        #endregion Public Properties

        #region Internal Methods

        internal void Decode(byte[] message, int startPos, int length, string logTime, double timeStamp)
        {
            for (int index = startPos, count = 0; count < length; index++, count++)
            {
                m_InputQueue.Enqueue(message[index]);
            }

            bool processedPayload = false;
            while (m_InputQueue.Count > 0)
            {
                switch (m_ProcessingState)
                {
                    case ProcessingState.Idle:
                        DoIdleProcess(logTime, timeStamp);
                        break;
                    case ProcessingState.Sync_Rx_SYN:
                        ProcessRxSyn();
                        break;
                    case ProcessingState.Sync_Rx_DLE:
                        ProcessRxDLE();
                        break;
                    case ProcessingState.Payload:
                        if (m_OutOfFrameBytes.Count > 0)
                        {
                            m_MessageStartTime = timeStamp;
                            m_MessageStartTimeText = logTime;
                            m_MessageEndTime = timeStamp;
                            m_MessageEndTimeText = logTime;
                            ReportOutOfFrameBytes();
                        }

                        ProcessPayload();
                        break;
                    case ProcessingState.Payload_Rx_DLE:
                        ProcessPayloadRxDLE();
                        break;
                    case ProcessingState.Tail:
                        processedPayload = true;
                        ProcessTail(logTime, timeStamp);
                        break;
                }                
            }

            if (!processedPayload && m_OutOfFrameBytes.Count > 0)
            {
                m_MessageStartTime = timeStamp;
                m_MessageStartTimeText = logTime;
                m_MessageEndTime = timeStamp;
                m_MessageEndTimeText = logTime;
                ReportOutOfFrameBytes();
            }
        }

        private void ReportOutOfFrameBytes()
        {
            OnProcessingComplete(ProcessingResult.OutOfFrameBytesFound);
            m_OutOfFrameBytes.Clear();
        }

        internal void Reset()
        {
            m_ProcessingState = ProcessingState.Idle;
            m_InputQueue.Clear();
        }

        #endregion Internal Methods

        #region Private Enums

        enum ProcessingState
        {
            Idle,
            Sync_Rx_SYN,
            Sync_Rx_DLE,
            Payload,
            Payload_Rx_DLE,
            Tail
        }

        #endregion Private Enums

        #region Private Methods

        private void DoIdleProcess(string logTime, double timestamp)
        {
            m_FcsError = false;

            byte inByte = m_InputQueue.Dequeue();
            if (inByte == SYN)
            {
                m_ProcessingState = ProcessingState.Sync_Rx_SYN;
                m_MessageStartTime = timestamp;
                m_MessageStartTimeText = logTime;
            }
            else
            {
                m_OutOfFrameBytes.Add(inByte);
            }
        }

        private void ProcessPayload()
        {
            // End of payload signalled by DLE ETX
            // DTE DTE is converted to DLE in payload
            while (m_InputQueue.Count > 0 && m_ProcessingState == ProcessingState.Payload)
            {
                byte inByte = m_InputQueue.Dequeue();

                if (inByte == DLE)
                {
                    m_ProcessingState = ProcessingState.Payload_Rx_DLE;
                }
                else
                {
                    m_Map27Packet.Add(inByte);
                }
            }
        }

        private void ProcessPayloadRxDLE()
        {
            // DLE DLE in packet is converted to DLE in payload (DLE stuffing)
            // End of payload signalled by DLE ETX
            // DLE <other> is stored as-is in payload
            byte inByte = m_InputQueue.Dequeue();
            if (inByte == DLE)
            {
                m_ProcessingState = ProcessingState.Payload;
                m_Map27Packet.Add(inByte);
            }
            else if (inByte == ETX)
            {
                m_ProcessingState = ProcessingState.Tail;
                m_Fcs.Clear();
            }
            else
            {
                m_Map27Packet.Add(DLE);

                m_Map27Packet.Add(inByte);
            }
        }

        private void ProcessRxDLE()
        {
            byte inByte = m_InputQueue.Dequeue();
            if (inByte == STX)
            {
                m_ProcessingState = ProcessingState.Payload;
                m_Map27Packet.Clear();
            }
            else
            {
                m_OutOfFrameBytes.Add(inByte);
                m_ProcessingState = ProcessingState.Idle;
            }
        }

        private void ProcessRxSyn()
        {
            byte inByte = m_InputQueue.Dequeue();
            if (inByte == DLE)
            {
                m_ProcessingState = ProcessingState.Sync_Rx_DLE;
            }
            else
            {
                m_OutOfFrameBytes.Add(inByte);
                m_ProcessingState = ProcessingState.Idle;
            }
        }

        private void ProcessTail(string logTime, double timeStamp)
        {
            while (m_InputQueue.Count > 0 && m_Fcs.Count < FcsLength)
            {
                m_Fcs.Add(m_InputQueue.Dequeue());
            }

            if (m_Fcs.Count == FcsLength)
            {
                ValidateFcs();
                if (OnProcessingComplete != null)
                {
                    if (m_FcsError)
                    {
                        OnProcessingComplete(ProcessingResult.FcsError);
                    }
                    else
                    {
                        m_MessageEndTime = timeStamp;
                        m_MessageEndTimeText = logTime;
                        OnProcessingComplete(ProcessingResult.Ok);
                    }
                }

                m_ProcessingState = ProcessingState.Idle;
            }
        }

        private void ValidateFcs()
        {
            // The packet body (header and data) and DLE ETX stop flag are included in the FCS 
            // calculation. The start sequence and all DLE control characters used to maintain 
            // data transparency are excluded from the FCS calculation.
            byte[] fcsBuffer = new byte[m_Map27Packet.Count + 2];
            int fcsBufferIndex = 0;
            foreach (byte b in m_Map27Packet)
            {
                fcsBuffer[fcsBufferIndex++] = b;
            }
            fcsBuffer[fcsBufferIndex++] = DLE;
            fcsBuffer[fcsBufferIndex++] = ETX;

            UInt16 fcsRead = (UInt16)((m_Fcs[0] << 8) + (UInt16)m_Fcs[1]);
            UInt16 fcsCalculated = m_Crc16.CalculateCrc(fcsBuffer);

            m_FcsError = (fcsRead != fcsCalculated);
        }

        #endregion Private Methods

        #region Private Fields

        const byte DLE = 0x10;
        const byte ETX = 0x03;
        const int FcsLength = 2;
        const int L2HeaderLength = 4;
        const byte STX = 0x02;
        const byte SYN = 0x16;
        Crc16 m_Crc16 = new Crc16();
        List<byte> m_Fcs = new List<byte>();
        bool m_FcsError;
        Queue<byte> m_InputQueue = new Queue<byte>();
        List<byte> m_Map27Packet = new List<byte>();
        double m_MessageEndTime;
        string m_MessageEndTimeText;
        double m_MessageStartTime;
        string m_MessageStartTimeText;
        private List<byte> m_OutOfFrameBytes = new List<byte>();
        private ProcessingState m_ProcessingState = ProcessingState.Idle;

        #endregion Private Fields
    }
}
